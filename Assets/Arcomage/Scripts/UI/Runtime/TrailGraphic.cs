using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Serialization;
using System.Collections;
using UnityEngine.Pool;

// using NOAH.Utility;

namespace NOAH.UI
{
    public class TrailGraphic : MaskableGraphic
    {
        class Node
        {
            public Node()
            {
            }

            public float m_SpawnTime = 0.0f;
            public float m_Length = 0.0f; // magnitude from current node to previous node
            public float m_AccumulateLength = 0.0f; // accumulated length from current node to first node.
            public Vector2 m_Pos;
            public Vector2 m_LeftPos;
            public Vector2 m_RightPos;
        }

        [SerializeField] Texture m_Texture;

        [SerializeField]
        private float m_Width = 40;

        [SerializeField]
        private float m_Duration = 0.3f;

        private List<Node> m_Nodes = new List<Node>();

        private ObjectPool<Node> m_NodePool = new ObjectPool<Node>(null, null);

        public override Texture mainTexture
        {
            get
            {
                if (m_Texture == null)
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }
                    return s_WhiteTexture;
                }

                return m_Texture;
            }
        }

        public Texture texture
        {
            get
            {
                return m_Texture;
            }
            set
            {
                if (m_Texture == value)
                    return;

                m_Texture = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        public void AddNode(Vector2 pos)
        {
            var curNode = m_NodePool.Get();
            curNode.m_SpawnTime = Time.time;
            curNode.m_Pos = pos;
            m_Nodes.Add(curNode);

            if (m_Nodes.Count > 1)
            {
                var preNode = m_Nodes[m_Nodes.Count - 2];
                curNode.m_Length = (curNode.m_Pos - preNode.m_Pos).magnitude;

                var halfWidth = m_Width * 0.5f;
                var dir = curNode.m_Pos - preNode.m_Pos;
                var right = new Vector2(dir.y, -dir.x).normalized;

                curNode.m_RightPos = curNode.m_Pos + right * halfWidth;
                curNode.m_LeftPos = curNode.m_Pos - right * halfWidth;

                if (m_Nodes.Count == 2)
                {
                    var firstNode = m_Nodes[0];
                    var offset = m_Nodes[1].m_Pos - m_Nodes[0].m_Pos;
                    firstNode.m_RightPos = m_Nodes[1].m_RightPos - offset;
                    firstNode.m_LeftPos = m_Nodes[1].m_LeftPos - offset;
                }
            }

            UpdateLength();

            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            Texture tex = mainTexture;
            vh.Clear();
            if (tex != null && m_Nodes.Count > 1)
            {
                var totalLength = m_Nodes.Last().m_AccumulateLength;
                var color32 = color;
                for (int i = m_Nodes.Count - 1; i > 0; --i)
                {
                    var curNode = m_Nodes[i];
                    var preNode = m_Nodes[i - 1];

                    float curU = curNode.m_AccumulateLength / totalLength;
                    float preU = preNode.m_AccumulateLength / totalLength;

                    vh.AddVert(new Vector3(curNode.m_RightPos.x, curNode.m_RightPos.y), color32, new Vector2(curU, 1.0f));
                    vh.AddVert(new Vector3(preNode.m_RightPos.x, preNode.m_RightPos.y), color32, new Vector2(preU, 1.0f));
                    vh.AddVert(new Vector3(preNode.m_LeftPos.x, preNode.m_LeftPos.y), color32, new Vector2(preU, 0.0f));
                    vh.AddVert(new Vector3(curNode.m_LeftPos.x, curNode.m_LeftPos.y), color32, new Vector2(curU, 0.0f));

                    int indexOffset = (m_Nodes.Count - 1 - i) * 4;

                    vh.AddTriangle(indexOffset + 0, indexOffset + 1, indexOffset + 2);
                    vh.AddTriangle(indexOffset + 2, indexOffset + 3, indexOffset + 0);
                }
            }
        }

        void Update()
        {
            var dirty = false;
            while (m_Nodes.Count > 0)
            {
                var node = m_Nodes[0];
                if (node.m_SpawnTime + m_Duration < Time.time)
                {
                    m_NodePool.Release(node);
                    m_Nodes.RemoveAt(0);
                    dirty = true;
                }
                else
                {
                    break;
                }
            }

            if (m_Nodes.Count > 0)
            {
                m_Nodes[0].m_Length = 0.0f;
                m_Nodes[0].m_AccumulateLength = 0.0f;
            }

            if (dirty)
            {
                UpdateLength();

                SetVerticesDirty();
            }
        }

        void UpdateLength()
        {
            for (int i = 1; i < m_Nodes.Count; ++i)
            {
                m_Nodes[i].m_AccumulateLength = m_Nodes[i].m_Length + m_Nodes[i - 1].m_AccumulateLength;
            }
        }

        [ContextMenu("Clear")]
        public void Clear()
        {
            foreach (var node in m_Nodes)
            {
                m_NodePool.Release(node);
            }

            m_Nodes.Clear();

            SetVerticesDirty();
        }

#if UNITY_EDITOR
        private Coroutine m_MockCoro = null;

        [ContextMenu("Start Mock")]
        public void StartMock()
        {
            StartCoroutine(MockImpl(Vector2.zero));
        }

        private IEnumerator MockImpl(Vector2 startPos)
        {
            while (true)
            {
                AddNode(startPos);

                startPos = startPos + new Vector2(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f));

                yield return new WaitForSeconds(0.2f);
            }
        }

        [ContextMenu("Stop Mock")]
        public void StopMock()
        {
            if (m_MockCoro != null)
            {
                StopCoroutine(m_MockCoro);
                m_MockCoro = null;
            }

            Clear();
        }
#endif
    }
}

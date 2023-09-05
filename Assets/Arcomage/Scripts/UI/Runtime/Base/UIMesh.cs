// using NOAH.Criware;
using UnityEngine;
using UnityEngine.UI;

namespace NOAH.UI
{
    [ExecuteInEditMode]
    public class UIMesh : MaskableGraphic
    {
        //public bool m_useInstanceMaterial = false;
        private Mesh m_meshForRendering;

        private bool doUpdate = false;
        //private Material m_instanceMaterial;
        private MeshFilter m_filter;
        private Renderer m_renderer;
        private SkinnedMeshRenderer m_skinnedMeshRenderer;
        
        
        void Update()
        {
            if (!doUpdate) return;
            if (m_skinnedMeshRenderer)
            {
                m_skinnedMeshRenderer.BakeMesh(m_meshForRendering);
                m_meshForRendering.RecalculateBounds();
                canvasRenderer.SetMesh(m_meshForRendering);
            }
                    
        }

        protected override void Awake()
        {
           
            base.Awake();
            Init();
        }

        public override Texture mainTexture
        {
            get
            {
                return material.mainTexture ?? s_WhiteTexture;
            }
        }

        public void ResetMaterial()
        {
            m_renderer = GetComponent<Renderer>();
            if (m_renderer)
            {
                material = m_renderer.sharedMaterial;
            }
        }

        public Material Material
        {
            get => base.material;
        }

        public void Init()
        {
            doUpdate = false;
            m_filter = GetComponent<MeshFilter>();
            if(m_filter)
                m_meshForRendering = m_filter.sharedMesh;
            m_renderer = GetComponent<Renderer>();
            if (m_renderer)
            {
                material = m_renderer.sharedMaterial;
                m_renderer.enabled = false;
                if (m_renderer is SkinnedMeshRenderer)
                {
                    m_skinnedMeshRenderer = m_renderer as SkinnedMeshRenderer;
                    m_meshForRendering = new Mesh();
                    doUpdate = true;
                }
            }
        }

        //public override Material material
        //{
        //    get
        //    {
        //        if (m_useInstanceMaterial)
        //        {
        //            if (m_instanceMaterial == null) m_instanceMaterial = new Material(base.material);
        //            return m_instanceMaterial;
        //        }
        //        else
        //        {
        //            return base.material;
        //        }
        //    }
        //    set
        //    {
        //        if (base.material != value)
        //        {
        //            m_instanceMaterial = null;
        //            base.material = value;
        //        }
        //    }
        //}


        protected override void UpdateGeometry()
        {
            canvasRenderer.SetMesh(m_meshForRendering);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Init();
        }
#endif

        // public void UpdateMesh()
        // {
        //     var filter = GetComponent<MeshFilter>();
        //     if(filter)
        //         m_meshForRendering = filter.sharedMesh;
        //     
        //     var renderer = GetComponent<Renderer>();
        //     if (renderer)
        //     {
        //         material = renderer.sharedMaterial;
        //         renderer.enabled = false;
        //         
        //
        //         if (renderer is SkinnedMeshRenderer)
        //         {
        //             var sr = renderer as SkinnedMeshRenderer;
        //             // sr.BakeMesh(m_meshForRendering);
        //             m_meshForRendering = sr.sharedMesh;
        //         }
        //     }
        // }
    }
}

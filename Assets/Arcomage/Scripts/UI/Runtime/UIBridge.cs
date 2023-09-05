using System;
// using NOAH.Debug;
using UnityEngine;


namespace NOAH.UI
{
    [ExecuteInEditMode]
    public class UIBridge : MonoBehaviour
    {
        public string m_targetWindowName = "";
        public GameObject m_prefab;
        public float m_hudHeight;
        private RectTransform m_targetWindowTransform;
        private RectTransform m_instance;
        
        private Transform m_cachedTransform;
        private Camera m_cam;
        private Camera m_camUI;
        public Camera CameraWorld
        {
            get => m_cam;
            set { m_cam = value; }
        }

        public Camera CameraUI
        {
            get => m_camUI;
            set { m_camUI = value;  }
        }
        
        public bool fixedPosition = false;
        private bool m_manualUpdate = false;
        private float m_offHeight = 0;

        public GameObject GetInstance()
        {
            return m_instance?.gameObject;
        }

        public Component GetInstanceComponent(Type type)
        {
            return m_instance.GetComponent(type);
        }
        public void SetHudHeight(float height)
        {
            m_hudHeight = height;
        }

        public void ChangeTargetWindow(string windowName, string rootPath, bool fixedPosition)
        {
            if (m_instance == null)
            {
                return;
            }
            // var targetWindow = UIManager.Instance.FindWindow(windowName);
            // if (targetWindow != null)
            // {
            //     m_targetWindowName = windowName;
            //     var root = targetWindow.transform.Find(rootPath);
            //     if (root != null)
            //     {
            //         m_targetWindowTransform = root as RectTransform;
            //     }
            //     else
            //     {
            //         m_targetWindowTransform = targetWindow.transform as RectTransform;
            //     }
            //     m_instance.SetParent(m_targetWindowTransform);
            //     if (!fixedPosition)
            //     {
            //         var center = new Vector2(0.5f, 0.5f);
            //         m_instance.pivot = center;
            //         m_instance.anchorMax = center;
            //         m_instance.anchorMin = center;
            //         m_instance.anchoredPosition = Vector2.zero;
            //     }
            //     this.fixedPosition = fixedPosition;
            //     Update();
            // }
        }
        
        public void InitInstance(string uiPriority = null)
        {
            if (m_instance == null)
            {
                // var targetWindow = UIManager.Instance.FindWindow(m_targetWindowName);
                // if (targetWindow != null)
                // {
                //     var f = targetWindow.transform.Find("UIBridges");
                //     if (f != null)
                //     {
                //         m_targetWindowTransform = f as RectTransform;
                //     }
                //     else
                //     {
                //         m_targetWindowTransform = targetWindow.transform as RectTransform;
                //     }
                //     
                //     if (uiPriority != null)
                //     {
                //         var priority = m_targetWindowTransform.Find(uiPriority);
                //         if (priority == null)
                //         {
                //             LogTool.LogWarning("GamePlay", "没有这个优先级的节点：" + uiPriority);
                //             priority = m_targetWindowTransform;
                //         }
                //         m_instance = Instantiate(m_prefab, priority).GetComponent<RectTransform>();
                //     }
                //     else
                //     {
                //         m_instance = Instantiate(m_prefab, m_targetWindowTransform).GetComponent<RectTransform>();
                //     }
                //     Update();
                // }
            }
        }
        
        public void InitInstanceInEditor()
        {
            if (m_instance == null)
            {
                var floatingUI = GameObject.Find("FloatingUI");
                if (floatingUI != null)
                {
                    m_targetWindowTransform = floatingUI.transform as RectTransform;

                    m_instance = Instantiate(m_prefab, m_targetWindowTransform).GetComponent<RectTransform>();
                    Update();
                }
            }
        }

        public void SetManualUpdate(Transform trans, float off)
        {
            m_manualUpdate = true;
            m_cachedTransform = trans;
            m_offHeight = off;
        }

        public void Update()
        {
            if (m_manualUpdate)
                return;
            ManualUpdate();
        }

        public void ManualUpdate()
        {
            if (m_instance && !fixedPosition)
            {
                if (m_cachedTransform == null) m_cachedTransform = transform;
                if(!m_cam)
                    m_cam = Camera.main;
                var pos = m_cachedTransform.position + new Vector3(0, m_offHeight, 0);
                if (!Application.isPlaying)
                {
                    var newPos = UIUtility.WorldToUIPos(CameraWorld, CameraUI, pos, m_targetWindowTransform);
                    m_instance.anchoredPosition = newPos;
                }
                else
                {
                    var newPos = UIUtility.WorldToUIPos(m_cam, pos, m_targetWindowTransform);
                    m_instance.anchoredPosition = newPos;
                    //UnityEngine.Debug.LogWarning($"pos:{newPos}");
                }
            }
        }

        private void OnEnable()
        {
            if (m_instance != null) m_instance.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            if (m_instance != null) m_instance.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (m_instance != null)
            {
                m_instance.SetParent(null, false);
                DestroyImmediate(m_instance.gameObject);
                m_instance = null;
            }
        }
    } 
}

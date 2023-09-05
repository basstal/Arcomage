// using NOAH.Debug;
// using NOAH.Proto;
// using NOAH.SFX;
// using NOAH.Utility;
using System;
using UnityEngine;


namespace NOAH.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UIWindow : MonoBehaviour//NOAH.Utility.DelegatedBehaviour
    {
        private bool m_init = false;
        private UIWindowConfig m_config = null;
        private int m_referenceCount = 0;

        private Animator m_animator;

        [SerializeField]
        private Transform m_statusBarRoot = null;
        public Transform StatusBarRoot { get { return m_statusBarRoot != null ? m_statusBarRoot : transform; } }

        public UIWindowConfig Config
        {
            get
            {
                if (m_config == null)
                {
                    // m_config = NOAH.Proto.Xlsx.UIWindowConfig.Get(name);
                }

                // LogTool.Assert(m_config != null, "Missing window config for: " + name);
                if (m_config == null)
                {
                    m_config = new UIWindowConfig();
                }

                return m_config;
            }
        }

        public int ReferenceCount
        {
            get { return m_referenceCount; }
            set { m_referenceCount = value; }
        }

        private bool m_recycling = false;
        public bool Recycling
        {
            get { return m_recycling; }
            set { m_recycling = value; }
        }

        public void Setup()
        {
            SetupWindow();

            m_animator = GetComponent<Animator>();
        }

        protected virtual void SetupWindow()
        {

        }

        public void Init()
        {
            if (!m_init)
            {
                InitWindow();
                m_init = true;
            }
        }

        protected virtual void InitWindow()
        {

        }

        public void Uninit()
        {
            // CleanUp();

            UninitWindow();

            m_init = false;
        }

        protected virtual void UninitWindow()
        {

        }

        public void Focus(bool on)
        {
            if (on)
            {
                // SFXManager.Instance.PlayMusic(Config.MusicCueSheet, Config.Music, Config.MusicCueBlockName);
            }

            FocusWindow(on);
        }

        protected virtual void FocusWindow(bool on)
        {

        }

        public void Back()
        {
            BackWindowEvent();
        }

        protected virtual void BackWindowEvent()
        {

        }


        public void PlayAnimation(string animName, Action onComplete = null)
        {
            if (m_animator != null && m_animator.HasState(0, Animator.StringToHash(animName)))
            {
                m_animator.Play(animName);
                m_animator.Update(0);   //需立即Update后下面length的获取才正确
                // DelayInvokeInSeconds(m_animator.GetCurrentAnimatorStateInfo(0).length, () =>
                // {
                //     onComplete?.Invoke();
                // });
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public void OnAcquire()
        {
            ++m_referenceCount;
        }

        public void OnRecycle(bool changeLevel = false)
        {
            --m_referenceCount;
        }

        public virtual void SaveContext()
        {

        }

        public virtual void LoadContext()
        {

        }

        private void Update()
        {
            if (m_init && m_referenceCount > 0)
            {
                UpdateWindow(Time.deltaTime);
            }
        }

        protected virtual void UpdateWindow(float deltaTime)
        {

        }

        private void LateUpdate()
        {
            if (m_init && m_referenceCount > 0)
            {
                LateUpdateWindow(Time.deltaTime);
            }
        }

        protected virtual void LateUpdateWindow(float deltaTime)
        {

        }

        protected void OnDestroy()
        {
            // base.OnDestroy();

            // ** 尝试让luaEnv的dispose成功（清理c# call lua的delegate bridge强引用）
            foreach (var component in GetComponentsInChildren<UnityEngine.UI.Button>(true)) component.onClick = null;
            foreach (var component in GetComponentsInChildren<UnityEngine.UI.Toggle>(true)) component.onValueChanged = null;
            foreach (var component in GetComponentsInChildren<EventHandler>(true)) component.Callback = null;

            DestroyWindow();
        }

        protected virtual void DestroyWindow()
        {

        }
    }
}
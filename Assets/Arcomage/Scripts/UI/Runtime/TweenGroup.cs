using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TweenGroup : MonoBehaviour
{
    [Serializable]
    public class TweenConfig
    {
        public string id;
        public float startDelay;
        public float childDelay;

        public Action onFinished;
    }

    public enum Operation
    {
        Restart,
        Rewind,
        TogglePause,
        Complete,
        Pause,
        Play
    }

    public List<string> m_playOnStart = new List<string>();
    public List<string> m_playOnActive = new List<string>();
    public List<TweenConfig> m_groups = new List<TweenConfig>();
    private Dictionary<string, Sequence> m_sequenceMap = new Dictionary<string, Sequence>();

    bool m_isInited = false;

    void Awake()
    {
        if (!m_isInited)
            Init();
        for (int i = 0; i < m_playOnStart.Count; i++)
        {
            Sequence s = null;
            if (m_sequenceMap.TryGetValue(m_playOnStart[i], out s))
            {
                s.Play();
            }
        }

        for (int i = 0; i < m_playOnActive.Count; i++)
        {
            Sequence s = null;
            if (m_sequenceMap.TryGetValue(m_playOnActive[i], out s))
            {
                s.Restart();
            }
        }
    }

    void OnEnable()
    {
        //DOTweenAnimation在Start才初始化Tween，无法在OnEnable时就播放
        if (!m_isInited)
            return;
        for (int i = 0; i < m_playOnActive.Count; i++)
        {
            Sequence s = null;
            if (m_sequenceMap.TryGetValue(m_playOnActive[i], out s))
            {
                s.Restart();
            }
        }
    }

    void Init()
    {
        for (int i = 0; i < m_groups.Count; i++)
        {
            TweenConfig tc = m_groups[i];
            InitSequence(tc);
        }

        m_isInited = true;
    }

    void InitSequence(TweenConfig tc)
    {
        Sequence s = null;
        if (m_sequenceMap.TryGetValue(tc.id, out s))
        {
            m_sequenceMap.Remove(tc.id);
            s.Kill();
        }

        s = DOTween.Sequence();

        s.SetTarget(this);
        s.OnComplete(() => tc.onFinished?.Invoke());
        List<DOTweenAnimation> animations = new List<DOTweenAnimation>();
        GetComponentsInChildren(true, animations);
        for (var i = 0; i < animations.Count; i++)
        {
            var a = animations[i];
            if (a.id == tc.id)
            {
                if (a.tween == null)
                    a.CreateTween();
                // ** prevent from DOTween warning : Infinite loops aren't allowed inside a Sequence (only on the Sequence itself) and will be changed to int.MaxValue
                if (a.tween.Loops() == -1)
                {
                    a.tween.SetLoops(int.MaxValue);
                }

                s.Insert(tc.childDelay * i, a.tween);
            }
        }

        s.PrependInterval(tc.startDelay);
        s.SetAutoKill(false);
        s.SetUpdate(true);
        s.Pause();
        m_sequenceMap.Add(tc.id, s);
    }

    void DoBatch(Operation opr, string[] ids)
    {
        if (ids.Length == 0)
        {
            foreach (var item in m_sequenceMap)
            {
                Sequence s = item.Value;
                DoSequence(s, opr);
            }

            return;
        }

        for (int i = 0; i < ids.Length; i++)
        {
            Sequence s = null;
            if (m_sequenceMap.TryGetValue(ids[i], out s))
            {
                DoSequence(s, opr);
            }
            else
            {
                // NOAH.Debug.LogTool.LogWarning("TweenGroup", "Please do SetSequence() for id: " + ids[i] + " before do thhis!");
            }
        }
    }

    void DoSequence(Sequence s, Operation opr)
    {
        switch (opr)
        {
            case Operation.Restart:
                s.Restart();
                break;
            case Operation.Rewind:
                s.Rewind();
                break;
            case Operation.TogglePause:
                s.TogglePause();
                break;
            case Operation.Complete:
                s.Complete();
                break;
            case Operation.Pause:
                s.Pause();
                break;
            case Operation.Play:
                s.Play();
                break;
        }
    }

    public void Play(params string[] ids)
    {
        if (!m_isInited)
            Init();
        DoBatch(Operation.Restart, ids);
    }

    public void PlayContinue(params string[] ids)
    {
        if (!m_isInited)
            Init();
        DoBatch(Operation.Play, ids);
    }

    public void TogglePause(params string[] ids)
    {
        DoBatch(Operation.TogglePause, ids);
    }

    public void Pause(params string[] ids)
    {
        DoBatch(Operation.Pause, ids);
    }

    public void Complete(params string[] ids)
    {
        DoBatch(Operation.Complete, ids);
    }

    public void Rewind(params string[] ids)
    {
        DoBatch(Operation.Rewind, ids);
    }

    public void SetSequence(string id, float startDelay, float childDelay)
    {
        TweenConfig tweenConfig = m_groups.Find(tc => tc.id == id);
        if (tweenConfig == null)
        {
            tweenConfig = new TweenConfig();
            m_groups.Add(tweenConfig);
        }
        else
        {
            if ((tweenConfig.startDelay == startDelay) && (tweenConfig.childDelay == childDelay))
                return;
        }

        tweenConfig.id = id;
        tweenConfig.startDelay = startDelay;
        tweenConfig.childDelay = childDelay;
        InitSequence(tweenConfig);
    }

    public void PlayNew(string id, float startDelay, float childDelay)
    {
        SetSequence(id, startDelay, childDelay);
        Play(id);
    }

    public float GetGroupDelay(string id)
    {
        TweenConfig tweenConfig = m_groups.Find(tc => tc.id == id);
        if (tweenConfig == null)
        {
            return -1;
        }

        return tweenConfig.startDelay;
    }
    public void OnDestroy()
    {
        foreach (var item in m_sequenceMap)
        {
            item.Value.Kill();
        }
        m_sequenceMap.Clear();// 光执行这个还没用
    }

    public void SetOnCompleteAction(string id, TweenCallback onComplete)
    {
        if (m_sequenceMap.TryGetValue(id, out var tmpSequence))
        {
            tmpSequence.OnComplete(onComplete);
        }
    }
}

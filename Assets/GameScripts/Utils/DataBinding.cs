using System;
using System.Collections.Generic;

namespace GameScripts.Utils
{
    public delegate void DataBindingCallback(params object[] param);

    public class DataBinding
    {
        private static DataBinding m_instance;

        public static DataBinding Instance => m_instance ?? new DataBinding();

        public class DataBindingEntry
        {
            private WeakEvent<DataBindingCallback> m_slots = new WeakEvent<DataBindingCallback>();

            private object[] m_data = null;

            public object[] Data
            {
                get { return m_data; }
                set
                {
                    m_data = value;
                    m_slots.Invoke(m_data);
                }
            }

            public bool IsConnected()
            {
                return m_slots;
            }

            public void Connect(DataBindingCallback callback, bool autoTrigger = true)
            {
                if (callback != null)
                {
                    m_slots += callback;
                    if (autoTrigger)
                    {
                        try
                        {
                            callback(Data);
                        }
                        catch (Exception e)
                        {
                            Log.LogError("DataBinding", e.Message);
                        }
                    }
                }
            }

            public void Disconnect(DataBindingCallback callback)
            {
                if (callback != null)
                {
                    m_slots -= callback;
                }
            }
        }

        public Dictionary<string, DataBindingEntry> m_entries;

        public DataBinding()
        {
            m_entries = new Dictionary<string, DataBindingEntry>();
        }

        public bool IsBinded(string key)
        {
            return m_entries.ContainsKey(key) && m_entries[key].IsConnected();
        }

        public DataBindingContext Bind(string key, DataBindingCallback callback, bool autoTrigger = true)
        {
            if (!m_entries.ContainsKey(key))
            {
                m_entries.Add(key, new DataBindingEntry());
            }

            m_entries[key].Connect(callback, autoTrigger);

            return new DataBindingContext(key, callback);
        }

        public DataBindingContext Unbind(string key, DataBindingCallback callback)
        {
            if (m_entries.ContainsKey(key))
            {
                m_entries[key].Disconnect(callback);
            }

            return new DataBindingContext(key, callback);
        }

        public void SetData(string key, params object[] data)
        {
            if (!m_entries.ContainsKey(key))
            {
                m_entries.Add(key, new DataBindingEntry());
            }

            m_entries[key].Data = data;
        }

        public object GetData(string key)
        {
            object result = null;

            if (m_entries.ContainsKey(key))
            {
                result = m_entries[key].Data;
            }

            return result;
        }

        public void TriggerData(string key)
        {
            SetData(key, GetData(key));
        }

        public void Clear()
        {
            m_entries.Clear();
        }
    }

    public class DataBindingContext
    {
        public string Key { get; private set; }
        public DataBindingCallback Callback { get; private set; }

        public DataBindingContext(string key, DataBindingCallback callback)
        {
            Key = key;
            Callback = callback;
        }

        public override bool Equals(object obj)
        {
            var right = obj as DataBindingContext;
            return right != null && right.Key == Key && right.Callback == Callback;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode() ^ Callback.GetHashCode();
        }

        public void Delete()
        {
            DataBinding.Instance.Unbind(Key, Callback);
        }
    }

    public class WeakEvent<T> where T : class
    {
        List<WeakReference> m_list = new List<WeakReference>();

        List<WeakReference> m_backstageList;

        public static WeakEvent<T> operator +(WeakEvent<T> list, T element)
        {
            list.Add(element);
            return list;
        }

        public static WeakEvent<T> operator -(WeakEvent<T> list, T element)
        {
            list.Remove(element);
            return list;
        }

        public static implicit operator bool(WeakEvent<T> list)
        {
            return (list.m_backstageList != null ? list.m_backstageList : list.m_list).Count > 0;
        }

        public void Invoke(params object[] args)
        {
            SyncForward();
            for (int i = 0; i < m_list.Count;)
            {
                var iter = m_list[i];
                var @delegate = (iter.Target as T) as Delegate;
                if (@delegate != null)
                {
                    try
                    {
                        @delegate.DynamicInvoke(args);
                    }
                    catch (Exception e)
                    {
                        e = e.InnerException != null ? e.InnerException : e;
                        Log.LogError("WeakEvent", e);
                    }

                    i++;
                }
                else
                {
                    m_list.RemoveAt(i);
                }
            }
        }

        private void Add(T item)
        {
            SyncBackward();

            bool existed = false;
            foreach (var entry in m_backstageList)
            {
                if (entry.Target as T == item || item.Equals(entry.Target))
                {
                    existed = true;
                    Log.Assert(!existed, "Add Duplicated Item");
                    break;
                }
            }

            if (!existed)
                m_backstageList.Add(new WeakReference(item));
        }

        private void Remove(T item)
        {
            SyncBackward();
            for (int i = m_backstageList.Count - 1; i >= 0; i--)
            {
                var iter = m_backstageList[i];
                if (iter.Target == null || iter.Target as T == item || item.Equals(iter.Target))
                {
                    m_backstageList.RemoveAt(i);
                }
            }
        }

        private void SyncBackward()
        {
            if (m_backstageList == null)
            {
                m_backstageList = new List<WeakReference>();
                foreach (var entry in m_list)
                {
                    if (entry.Target != null)
                    {
                        m_backstageList.Add(entry);
                    }
                }
            }
        }

        private void SyncForward()
        {
            if (m_backstageList != null)
            {
                m_list = m_backstageList;
                m_backstageList = null;
            }
        }
    }
}
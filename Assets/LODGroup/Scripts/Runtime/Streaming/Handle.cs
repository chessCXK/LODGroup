using Chess.LODGroupIJob.Interface;
using Chess.LODGroupIJob.LoadAsset;
using System;
using UnityEngine;

namespace Chess.LODGroupIJob.Streaming
{
    public enum AsyncOperationStatus
    {
        None,
        Failed,
        Succeeded
    }
    [Serializable]
    public class Handle
    {
        public event Action<Handle> Completed;

        private LOD m_Controller;
        private string m_Address;
        private int m_Priority;
        private float m_Distance;
        private bool m_StartLoad = false;
        private AsyncOperationStatus m_Status;
        private uint m_Id;
        private GameObject m_Obj;
        #region AssetLoadManager使用
        public int Priority { get => m_Priority; }

        public float Distance { get => m_Distance; }

        public LOD Controller { get => m_Controller; }
        #endregion
        public AsyncOperationStatus Status
        {
            get
            {
                if (m_StartLoad == false)
                {
                    return AsyncOperationStatus.None;
                }
                return m_Status;
            }
        }

        public GameObject Result { get => m_Obj; set => m_Obj = value; }
        public uint Id { get => m_Id; set => m_Id = value; }
        public Handle(LOD controller, string address, int priority, float distance)
        {
            m_Controller = controller;
            m_Address = address;
            m_Priority = priority;
            m_Distance = distance;
        }

        //开始
        public bool Start()
        {
            var load = LoadAseetManager<ILoadAsset>.Instance.loadAsset;
            //Debug.Log(load);
            if (load == null)
                return false;
            m_StartLoad = true;
            Action<uint, GameObject> action = (uint id, GameObject obj) =>
            {
                m_Id = id;
                m_Obj = obj;
                m_Status = m_Obj == null ? AsyncOperationStatus.Failed : AsyncOperationStatus.Succeeded;
                Completed?.Invoke(this);
            };
            load.LoadAsync(m_Address, action);
            return true;
        }

        //结束
        public void UnloadAsset()
        {
            if (m_StartLoad == true)
            {
                var load = LoadAseetManager<ILoadAsset>.Instance.loadAsset;
                if (load == null)
                    return;
                load.UnloadAsset(Id);
            }
        }
    }
}

using Chess.LODGroupIJob.Utils;
using System.Collections.Generic;
namespace Chess.LODGroupIJob.Streaming
{
    public class AssetLoadManager
    {
        #region Singleton
        private static AssetLoadManager _Instance;

        public static AssetLoadManager Instance
        {
            get
            {
                
                if (_Instance == null)
                {
                    _Instance = new AssetLoadManager();
                }

                return _Instance;
            }
        }
        #endregion
        public AssetLoadManager()
        {
            m_Config = SystemConfig.Instance;
        }
        private SystemConfig m_Config;
        private int m_LoadCount = 0;
        private bool m_IsLoading = false;
        private LinkedList<Handle> m_loadQueue = new LinkedList<Handle>();

        //开始加载
        public Handle LoadAsset(LOD controller, string address, int priority, float distance)
        {
            Handle handle = new Handle(controller, address, priority, distance);
            return handle;
        }
        public void Start(Handle handle)
        {
            InsertHandle(handle);
        }
        //卸载
        public void UnloadAsset(Handle handle)
        {
            m_loadQueue.Remove(handle);
            handle.UnloadAsset();
        }

        //加载队列处理
        private void InsertHandle(Handle handle)
        {
            var node = m_loadQueue.First;
            while (node != null && node.Value.Priority < handle.Priority)
            {
                node = node.Next;
            }

            while (node != null && node.Value.Priority == handle.Priority && node.Value.Distance < handle.Distance)
            {
                node = node.Next;
            }

            if (node == null)
            {
                if (m_IsLoading == true)
                {
                    m_loadQueue.AddLast(handle);
                }
                else
                {
                    StartLoad(handle);
                }
            }
            else
            {
                m_loadQueue.AddBefore(node, handle);
            }
        }

        //正式加载
        private void StartLoad(Handle handle)
        {
            handle.Completed += handle1 =>
            {
                m_LoadCount--;
                m_IsLoading = false;
                if (m_loadQueue.Count > 0)
                {
                    Handle next = m_loadQueue.First.Value;
                    m_loadQueue.RemoveFirst();
                    StartLoad(next);
                }
            };

            bool result = handle.Start();

            if (result)
            {
                if (++m_LoadCount == m_Config.Config.asynLoadNum)
                {
                    m_IsLoading = true;
                    return;
                }
                    
            }

            //加载数量没达到设定继续加载
            if (m_loadQueue.Count == 0)
                return;
            Handle nextHandle = m_loadQueue.First.Value;
            m_loadQueue.RemoveFirst();
            StartLoad(nextHandle);
        }
    }
}
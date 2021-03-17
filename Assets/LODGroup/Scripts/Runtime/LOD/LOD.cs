using Chess.LODGroupIJob.Streaming;
using Chess.LODGroupIJob.Utils;
using System;
using UnityEngine;

namespace Chess.LODGroupIJob
{
    public enum State
    {
        None,
        UnLoaded,
        Loading,
        Loaded,
        Failed
    }

    //不使用继承原因抽象类无法序列化，而ScriptableObject会在拷贝的时候引用不变麻烦
    [Serializable]
    public sealed class LOD
    {
        //在屏幕上占比高度[0-1]
        [SerializeField]
        private float screenRelativeTransitionHeight;
        //当前管理的Renderer
        [SerializeField]
        public Renderer[] m_Renderers;
        //当前状态
        [SerializeField]
        private State m_CurrentState;
        //上一帧状态
        [SerializeField]
        private State m_LastState;
        #region 流式加载
        //是否流式

        [SerializeField]
        private bool m_Streaming;
        [SerializeField]
        private string address;
        [SerializeField]
        private int priority;
        [SerializeField]
        private Handle handle;
        #endregion
        public LOD(float screenRelative)
        {
            screenRelativeTransitionHeight = screenRelative;
            m_CurrentState = State.None;
        }
        public Renderer[] Renderers { get => m_Renderers; set => m_Renderers = value; }
        public State CurrentState { get => m_CurrentState; set => m_CurrentState = value; }
        public State LastState { get => m_LastState; set => m_LastState = value; }
        public bool Streaming { get => m_Streaming; set => m_Streaming = value; }
        public float ScreenRelativeTransitionHeight { get => screenRelativeTransitionHeight; set => screenRelativeTransitionHeight = value; }
        public string Address { get => address; set => address = value; }
        public int Priority { get => priority; set => priority = value; }
        public Handle Handle { get => handle; set => handle = value; }
        
        //返回true表示刚加载完成，否则返回false
        public void SetState(bool active, LODGroup lodGroup, float distance, CameraType type)
        {
            if(m_Streaming)
            {
#if UNITY_EDITOR
                //编辑器模式下启动了流式加载那么也生效
                if (!Application.isPlaying && (type != CameraType.Game || !SystemConfig.Instance.Config.editorStream))
                {
                    return;
                }
#endif
                StreamingLOD.SetState(active, this, lodGroup, distance, type);
            }
            else
            {
                NormalLOD.SetState(active, this, lodGroup);
            } 
        }
    }
}

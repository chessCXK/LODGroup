using Chess.LODGroupIJob.JobSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Chess.LODGroupIJob
{
    [System.Serializable]
    public struct Bounds
    {
        [SerializeField]
        public Vector3 center;
        [SerializeField]
        public float size;
        public Bounds(Vector3 center, float size)
        {
            this.center = center;
            this.size = size;
        }
        public Bounds(UnityEngine.Bounds b)
        {
            center = b.center;
            size = b.size.x;
        }
    }
    public class LODGroupBase : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        protected Bounds m_Bounds;
        [SerializeField, HideInInspector]
        protected LOD[] m_LODs;
        protected int m_CurrentLOD = -1;
        protected int m_LoadingLOD = -1;
        //LODGroup包围盒大小，包围盒永远都是正方体
        public float size { get => Mathf.Max(m_Bounds.size); }
        public Bounds Bounds { get => m_Bounds; set => m_Bounds = value; }
#if UNITY_EDITOR
        GUIStyle e_Style;
        protected GUIStyle Style
        {
            get
            {
                if (e_Style == null)
                {
                    e_Style = new GUIStyle();
                    e_Style.fontSize = 20;
                    e_Style.alignment = TextAnchor.UpperCenter;
                }
                return e_Style;
            }
        }
#endif
        public virtual void UpdataState(JobResult calResult, CameraType type) { }
        public virtual void SetLODs(LOD[] lods)
        {
            LODGroupManager.Instance.Dirty = true;
        }
        public virtual LOD[] GetLODs()
        {
            return null;
        }
        public virtual void RecalculateBounds() 
        {
            LODGroupManager.Instance.Dirty = true;
        }
        public virtual void OnEnable()
        {
            LODGroupManager.Instance.SetLODGroup(this);
        }
        public virtual void OnDisable()
        {
            LODGroupManager.Instance.RemoveLODGroup(this);
        }
    }
}
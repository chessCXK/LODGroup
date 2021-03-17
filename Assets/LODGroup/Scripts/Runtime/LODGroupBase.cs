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
        protected List<LOD> m_LODList;
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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Chess.LODGroupIJob
{
    [ExecuteAlways]
    public class LODGroup : LODGroupBase
    {
        //基于当前物体的坐标位置
        public Vector3 localReferencePoint { get => m_Bounds.center; set => m_Bounds.center = value; }
        //LOD数量
        public int lodCount { get => m_LODList == null ? 0 : m_LODList.Count; }
        //LODGroup包围盒大小，包围盒永远都是正方体
        public float size { get => Mathf.Max(m_Bounds.size); }

        public Bounds Bounds { get => m_Bounds; set => m_Bounds = value; }

        //当前屏幕占位置[0-1]
        private float m_ScreenRelative;
        //当前lod
        private int m_CurrentLOD = 0;

        //有lod加载完后调用，参数是加载完的那个LOD
        private UnityAction<LOD> m_LoadedAction;

        private void Awake()
        {
            /*
            if (m_LODList == null)
                return;
            foreach(var lod in m_LODList)
            {
                lod.CurrentState = State.None;
            }*/
        }
        //设置LOD组
        public void SetLODs(LOD[] lods)
        {
            if (lods != null && lods.Length > 0)
            {
                if (m_LODList == null)
                    m_LODList = new List<LOD>();
                m_LODList.Clear();
                m_LODList.AddRange(lods);
            }
            
        }
        //获得LOD组
        public LOD[] GetLODs()
        {
            return m_LODList.ToArray();
        }
     
        //状态改变
        public void UpdataState(Vector2 relativeAndDistance, CameraType type)
        {
            m_ScreenRelative = relativeAndDistance.x;
            float distance = relativeAndDistance.y;
            bool select = false;
            int i = -1;
            foreach (var lod in m_LODList)
            {
                i++;
                if (m_ScreenRelative > lod.ScreenRelativeTransitionHeight)
                {
                    m_CurrentLOD = i;
                    lod.SetState(true, this, distance, type);
                    select = true;
                    break;
                }
            }
            if (!select)
            {
                m_CurrentLOD = -1;
                OnDisableAllLOD(type);
            }
#if UNITY_EDITOR
            if (!Application.isPlaying && UnityEditor.Selection.gameObjects.Length != 0)
            {
                //被选中的物体不隐藏
                foreach (var obj in UnityEditor.Selection.gameObjects)
                {
                    foreach (var lod in m_LODList)
                    {
                        if(lod.Renderers != null)
                        foreach (var rd in lod.Renderers)
                        {
                            if (rd != null && obj == rd.gameObject)
                            {
                                rd.enabled = true;
                            }
                        }
                    }
                    
                }
            }
#endif
        }
        public void OnDisableAllLOD(CameraType type = CameraType.Game)
        {
            int i = -1;
            foreach (var lod in m_LODList)
            {
                i++;
                if (i == m_CurrentLOD)
                    continue;

                lod.SetState(false, this, 0, type);
            }
        }
        public void RecalculateBounds()
        {
            List<Renderer> all = new List<Renderer>();
            foreach (var lod in m_LODList)
            {
                if (lod.Renderers != null)
                {
                    all.AddRange(lod.Renderers);
                }
            }
            UnityEngine.Bounds bounds;
            if (all.Count <= 0)
            {
                bounds = new UnityEngine.Bounds(Vector3.zero, Vector3.one);
            }
            else
            {
                bounds = all[0].bounds;
                for (int i = 1; i < all.Count; i++)
                {
                    bounds.Encapsulate(all[i].bounds);
                }
                //相对于当前节点的位置
                bounds.center = bounds.center - transform.position;
                var maxSize = Mathf.Max(Mathf.Max(bounds.size.x, bounds.size.y), bounds.size.z);
                bounds.size = Vector3.one * maxSize; 
            }
            Bounds = new Bounds(bounds);
        }
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (Selection.activeObject != this.gameObject)
                return;

            Camera cam = Camera.current;
            if (cam.cameraType != CameraType.SceneView)
                return;
            var tempColor = GUI.backgroundColor;
            string show;
            if (m_CurrentLOD == -1)
            {
                show = Utils.LODUtils.kLODCulled;
                Gizmos.color = Utils.LODUtils.kDefaultLODColor;
            }
            else
            {
                show = string.Format("LOD{0}", m_CurrentLOD);
                Gizmos.color = Utils.LODUtils.kLODColors[m_CurrentLOD];
            }

            var pos = transform.position + localReferencePoint;
            var screenPos = cam.WorldToScreenPoint(pos);
            var screenX = cam.ScreenToWorldPoint(new Vector3(screenPos.normalized.x, screenPos.y, screenPos.z));
            var screenY = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.normalized.y, screenPos.z));
            screenX = pos - screenX;
            screenY = pos - screenY;
            screenX.Normalize();
            screenY.Normalize();

            float halfSize = 0.5f * size;
            //上
            Gizmos.DrawLine(pos - (screenX - screenY) * halfSize, pos + (screenX + screenY) * halfSize);
            //左
            Gizmos.DrawLine(pos - (screenX + screenY) * halfSize, pos - (screenX - screenY) * halfSize);
            //右
            Gizmos.DrawLine(pos + (screenX - screenY) * halfSize, pos + (screenX + screenY) * halfSize);
            //下
            Vector3 v1 = pos - (screenX + screenY) * halfSize;
            Vector3 v2 = pos + (screenX - screenY) * halfSize;
            Gizmos.DrawLine(v1, v2);

            pos = v1 + Vector3.Distance(v1, v2) * screenX / 2.1f;
            Handles.Label(pos, show, Style);
            Gizmos.color = tempColor;

        }
#endif
    }
}


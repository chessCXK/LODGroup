using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Chess.LODGroupIJob.Utils;
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
        public int lodCount { get => m_LODs == null ? 0 : m_LODs.Length; }

        //有流式的的lod
        private bool m_CoverStreamLOD;
        private void Awake()
        {
            if (m_LODs == null)
                return;
            foreach(var lod in m_LODs)
            {
                if(lod.Streaming)
                {
                    m_CoverStreamLOD = true;
                    lod.CurrentState = State.None;
                }
                else
                {
                    lod.SetState(false, this, 0);
                }
                
            }
        }
        //设置LOD组
        public override void SetLODs(LOD[] lods)
        {
            base.SetLODs(lods);
            if (lods != null && lods.Length > 0)
            {
                m_LODs = lods;
            }
        }
        //获得LOD组
        public override LOD[] GetLODs()
        {   
            return m_LODs;
        }
     
        //状态改变
        public override void UpdataState(JobSystem.JobResult calResult, CameraType type)
        {
            if (calResult.lodLevel == m_CurrentLOD && calResult.lodLevel == m_LoadingLOD)
                return;
#if UNITY_EDITOR
            //运行模式如果有流式lod那么scence相机不生效
            if(Application.isPlaying && m_CoverStreamLOD && type != CameraType.Game)
            {
                return;
            }
            //编辑器模式下启动了流式加载那么也生效
            if (!Application.isPlaying && (type != CameraType.Game || !SystemConfig.Instance.Config.editorStream))
            {
                if(calResult.lodLevel != -1)
                    if(m_LODs[calResult.lodLevel].Streaming)
                        return;
            }
#endif
            /*if (type != CameraType.Game)
                return;*/

            if (calResult.lodLevel == -1)
            {
                if(m_LoadingLOD != -1)
                {
                    m_LODs[m_LoadingLOD].SetState(false, this, calResult.distance);
                    m_LoadingLOD = -1;
                }
                if (m_CurrentLOD != -1)
                {
                    m_LODs[m_CurrentLOD].SetState(false, this, calResult.distance);
                    m_CurrentLOD = -1;
                }
                return;
            }
                
            var lod = m_LODs[calResult.lodLevel];
            bool result = false;
            result = lod.SetState(true, this, calResult.distance, calResult.lodLevel);

            if(m_LoadingLOD != -1 && m_LoadingLOD != calResult.lodLevel && m_LoadingLOD != m_CurrentLOD)
            {
                m_LODs[m_LoadingLOD].SetState(false, this, calResult.distance);
            }
            m_LoadingLOD = calResult.lodLevel;
        }
        public void OnDisableCurrentLOD(int willLOD = -1)
        {
            if (m_CurrentLOD != -1 && m_CurrentLOD != willLOD)
            {
                m_LODs[m_CurrentLOD].SetState(false, this, 0);
            }
            m_CurrentLOD = willLOD;
        }
        public override void RecalculateBounds()
        {
            base.RecalculateBounds();
            List<Renderer> all = new List<Renderer>();
            foreach (var lod in m_LODs)
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
        void OnDrawGizmosSelected()
        {
            Camera cam = Camera.current;
            if (cam.cameraType != CameraType.SceneView)
                return;
            var tempColor = GUI.backgroundColor;
            string show;
            if (m_LoadingLOD == -1)
            {
                show = Utils.LODUtils.kLODCulled;
                Gizmos.color = Utils.LODUtils.kDefaultLODColor;
            }
            else
            {
                show = string.Format("LOD{0}", m_LoadingLOD);
                Gizmos.color = Utils.LODUtils.kLODColors[m_LoadingLOD];
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


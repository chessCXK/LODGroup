using Chess.LODGroupIJob.Utils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
namespace Chess.LODGroupIJob.JobSystem
{
    public struct Float8
    {
        //最后两个参数是切换缓冲使用
        public float v0;
        public float v1;
        public float v2;
        public float v3;
        public float v4;
        public float v5;
        public float v6;
        public float v7;
    }
    public struct SwitchOffet
    {
        //切换lod缓冲
        public bool relativeOffet;
        public float relativePercent;
    }
    public struct JobResult
    {
        public float distance;
        public float relative;//当前屏占比位置
        public int lodLevel;//第几级lod
    }
    public struct JobValueMode
    {
        public NativeArray<Bounds> bounds;
        public NativeArray<Float8> lodRelative;
        public NativeArray<bool> openBuffer;

        public NativeArray<JobResult> result;

        public bool vaild;
    }
    public struct JobValueView
    {
        public void AfreshCalculate(ref JobValueMode mode, ref HashSet<LODGroupBase> lodGroups)
        {
            if(mode.bounds.Length > 0)
            {
                mode.bounds.Dispose();
                mode.lodRelative.Dispose();
                mode.openBuffer.Dispose();
                mode.result.Dispose();
            }
            mode.bounds = new NativeArray<Bounds>(lodGroups.Count, Allocator.Persistent);
            mode.lodRelative = new NativeArray<Float8>(lodGroups.Count, Allocator.Persistent);
            mode.openBuffer = new NativeArray<bool>(lodGroups.Count, Allocator.Persistent);
            mode.result = new NativeArray<JobResult>(lodGroups.Count, Allocator.Persistent);

            int j = 0;
            foreach (var item in lodGroups)
            {
                Bounds b = item.Bounds;
                b.center = item.transform.position + b.center;
                mode.bounds[j] = b;
#if UNITY_EDITOR

                if (Application.isPlaying)
                    mode.openBuffer[j] = true;
                else
                    mode.openBuffer[j] = false;
#else
                mode.openBuffer[j] = true;
#endif

                var lods = item.GetLODs();
                int count = lods.Length;
                Float8 f8 = new Float8();
                for (int i = 0; i < count; i++)
                {
                    var v = lods[i].ScreenRelativeTransitionHeight;
                    switch (i)
                    {
                        case 0:
                            f8.v0 = v;
                            break;
                        case 1:
                            f8.v1 = v;
                            break;
                        case 2:
                            f8.v2 = v;
                            break;
                        case 3:
                            f8.v3 = v;
                            break;
                        case 4:
                            f8.v4 = v;
                            break;
                        case 5:
                            f8.v5 = v;
                            break;
                        case 6:
                            f8.v6 = v;
                            break;
                        case 7:
                            f8.v7 = v;
                            break;
                    }
                }
                mode.lodRelative[j++] = f8;
            }
            
            mode.vaild = true;
        }
        public void OnDisable(ref JobValueMode mode)
        {
            if(mode.bounds.Length > 0)
            {
                mode.bounds.Dispose();
            }
            if (mode.lodRelative.Length > 0)
            {
                mode.lodRelative.Dispose();
            }
            if(mode.openBuffer.Length > 0)
            {
                mode.openBuffer.Dispose();
            }
            if (mode.result.Length > 0)
            {
                mode.result.Dispose();
            }
        }
    }
    public class LODGroupManager
    {
        static LODGroupManager _Instance;
        public static LODGroupManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new LODGroupManager();
                    _Instance.m_Config = SystemConfig.Instance;

                    Camera.onPreCull += _Instance.OnPreCull;
                }
                return _Instance;
            }
        }

        private Camera m_MainCamera;
        public SystemConfig m_Config;

        HashSet<LODGroupBase> m_AllLODGroup = new HashSet<LODGroupBase>();

        private JobValueMode m_JobValueMode;
        private JobValueView m_JobValueView;
        private bool m_Dirty = false;
        public Camera MainCamera
        {
            get
            {
                if(m_MainCamera == null)
                    m_MainCamera = Camera.main;
                return m_MainCamera;
            }
        }

        public bool Dirty { get => m_Dirty; set => m_Dirty = value; }

        private class CameraCullData
        {
            public float m_lastCullTime = -1;
            public Vector3 m_LastCameraPosition;
            public float m_LastFOV;
            public float m_LastLODBias;
        }

        private Dictionary<Camera, CameraCullData> m_CullData = new Dictionary<Camera, CameraCullData>();

        public void SetLODGroup(LODGroupBase lodGroup)
        {
            bool result = m_AllLODGroup.Add(lodGroup);
            if(result)
            {
                Dirty = true;
            }
        }
        public bool RemoveLODGroup(LODGroupBase lodGroup)
        {
           bool result = m_AllLODGroup.Remove(lodGroup);
            if(result)
            {
                Dirty = true;
            }
            if(m_AllLODGroup.Count == 0)
            {
                m_JobValueView.OnDisable(ref m_JobValueMode);
            }
            return result;
        }

        static Unity.Profiling.ProfilerMarker p = new Unity.Profiling.ProfilerMarker("LODGroupCalulate");
        private void OnPreCull(Camera camera)
        {
#if UNITY_EDITOR
            //只有场景相机和主相机有用，防止流式的时候所有相机都在改变状态
            if (camera.cameraType != CameraType.SceneView && camera != MainCamera)
                return;
#else
            if (camera != MainCamera)
                return;
#endif
            int count = m_AllLODGroup.Count;
            if (count == 0)
                return;

            bool dirty = false;
            CameraCullData data;
            if(!m_CullData.TryGetValue(camera, out data))
            {
                data = new CameraCullData();
                m_CullData.Add(camera, data);
            }
            if(data.m_lastCullTime == -1)
            {
                //第一次进来刷新一下
                dirty = true;
            }
            else
            {
                // 刷新间隔没到，不做任何处理
                if (Application.isPlaying && data.m_lastCullTime + m_Config.Config.cullInterval > Time.realtimeSinceStartup)
                {
                    return;
                }
                //判断摄像机参数是否有变化
                var cameraPosition = camera.transform.position;
                if (data.m_LastCameraPosition != cameraPosition)
                {
                    data.m_LastCameraPosition = cameraPosition;
                    dirty = true;
                }
                if (data.m_LastFOV != camera.fieldOfView)
                {
                    data.m_LastFOV = camera.fieldOfView;
                    dirty = true;
                }
                //判断LOD精度设置是否有变化
                if (data.m_LastLODBias != QualitySettings.lodBias)
                {
                    data.m_LastLODBias = QualitySettings.lodBias;
                    dirty = true;
                }
            }
            data.m_lastCullTime = Time.realtimeSinceStartup;

#if UNITY_EDITOR
            //没运行的时候实时刷新
            if (!Application.isPlaying)
            {
                dirty = true;
            }
#endif
            if (!dirty)
                return;
                
            
            if(Dirty)
            {
                Dirty = false;
                m_JobValueView.AfreshCalculate(ref m_JobValueMode, ref m_AllLODGroup);
                
            }
            if (!m_JobValueMode.vaild)
                return;

            var job = new LODCalculateJob()
            {
                orthographic = camera.orthographic,
                orthographicSize = camera.orthographicSize,
                fieldOfView = camera.fieldOfView,
                lodBias = QualitySettings.lodBias,
                camPosition = camera.transform.position,
                bounds = m_JobValueMode.bounds,
                lodRelatives = m_JobValueMode.lodRelative,
                openBuffer = m_JobValueMode.openBuffer,
                result = m_JobValueMode.result
            };
            JobHandle jobHandle = job.Schedule(count, 30);
            jobHandle.Complete();
            
            int i = 0;
            var result = m_JobValueMode.result;
            foreach (var item in m_AllLODGroup)
            {
                item.UpdataState(result[i++], camera.cameraType);
            }
        }
    }
}
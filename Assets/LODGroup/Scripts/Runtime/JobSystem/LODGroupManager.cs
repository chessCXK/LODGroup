using Chess.LODGroupIJob.Utils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
namespace Chess.LODGroupIJob.JobSystem
{
    public class LODGroupManager
    {
        static LODGroupManager _Instance;
        public NativeArray<Bounds> bounds;
        public NativeArray<Vector2> result;
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
        Dictionary<int, LODGroup> m_AllLODGroup = new Dictionary<int, LODGroup>();

        public Camera MainCamera
        {
            get
            {
                if(m_MainCamera == null)
                    m_MainCamera = Camera.main;
                return m_MainCamera;
            }
        }
        private class CameraCullData
        {
            public float m_lastCullTime = -1;
            public Vector3 m_LastCameraPosition;
            public float m_LastFOV;
            public float m_LastLODBias;
        }

        private Dictionary<Camera, CameraCullData> m_CullData = new Dictionary<Camera, CameraCullData>();

        public bool Contanis(int hashCodeId)
        {
            return m_AllLODGroup.ContainsKey(hashCodeId);
        }
        public void SetLODGroup(LODGroupBase lodGroup)
        {
            int hashCode = lodGroup.GetHashCode();
            if (!Contanis(hashCode))
            {
                m_AllLODGroup.Add(hashCode, lodGroup as LODGroup);
            }
        }
        public bool RemoveLODGroup(LODGroupBase lodGroup)
        {
            int hashCode = lodGroup.GetHashCode();
            return m_AllLODGroup.Remove(hashCode);
        }

        static Unity.Profiling.ProfilerMarker p = new Unity.Profiling.ProfilerMarker("LODGroupCalulate");
        private void OnPreCull(Camera camera)
        {
#if UNITY_EDITOR
            //运行的时候,场景视图流式加载不生效
            if(Application.isPlaying == true &&
                camera.cameraType == CameraType.SceneView )
            {
                return;
            }
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

            /*
             * //高通骁龙625测试，每一帧500个计算的消耗不到2ms，注释的代码可以降低代码的计算次数
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
                */

            int i = 0;
            var bounds = new NativeArray<Bounds>(count, Allocator.TempJob);
            var result = new NativeArray<Vector2>(count, Allocator.TempJob);
            
            foreach (var item in m_AllLODGroup)
            {
                Bounds b = item.Value.Bounds;
                b.center = item.Value.transform.position + b.center;
                bounds[i++] = b;
            }
            
            var job = new LODCalculateJob()
            {
                orthographic = camera.orthographic,
                orthographicSize = camera.orthographicSize,
                fieldOfView = camera.fieldOfView,
                lodBias = QualitySettings.lodBias,
                camPosition = camera.transform.position,
                bounds = bounds,
                result = result
            };
            JobHandle jobHandle = job.Schedule(count, 30);
            jobHandle.Complete();
            
            i = 0;
            foreach (var item in m_AllLODGroup)
            {
                item.Value.UpdataState(result[i++], camera.cameraType);
            }
            bounds.Dispose();
            result.Dispose();
        }
    }
}
using Chess.LODGroupIJob.SpaceManager;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Chess.LODGroupIJob.JobSystem
{
    [BurstCompile(CompileSynchronously = true)]
    public struct LODCalculateJob : IJobParallelFor
    {
        [ReadOnly]
        public bool orthographic;
        [ReadOnly]
        public float orthographicSize;
        [ReadOnly]
        public float fieldOfView;
        [ReadOnly]
        public float lodBias;
        //世界坐标
        [ReadOnly]
        public Vector3 camPosition;
        //center转成世界坐标
        [ReadOnly]
        public NativeArray<Bounds> bounds;
        //返回[x：相对屏占比，y：与距离相机]
        public NativeArray<Vector2> result;
        public void Execute(int index)
        {
            result[index] = QuadTreeSpaceManager.SettingCameraJob(orthographic, orthographicSize, fieldOfView, lodBias, bounds[index], camPosition);
        }
    }
}

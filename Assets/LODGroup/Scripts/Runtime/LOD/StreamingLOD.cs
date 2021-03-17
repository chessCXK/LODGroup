using UnityEngine;
using Chess.LODGroupIJob.Streaming;
namespace Chess.LODGroupIJob
{
    public static class StreamingLOD
    {
        //正常模式只有显示和隐藏
        public static void SetState(bool active, LOD lod, LODGroup lodGroup, float distance, CameraType type)
        {
            switch (lod.CurrentState)
            {
                case State.None:
                case State.UnLoaded:
                    if (active == true)
                    {
                        LoadAsset(lod, lodGroup, distance, type);
                    }
                    break;
                case State.Loading:
                    if (active == false)
                    {
                        UnLoaded(lod);
                    }
                    break;
                case State.Loaded:
                    if(active == false)
                    {
                        UnLoaded(lod);
                    }
                    else if(lod.LastState == State.Loading)
                    {
                        return;
                    }
                    break;
            }
            lod.LastState = lod.CurrentState;
        }
        private static void LoadAsset(LOD lod, LODGroup lodGroup, float distance, CameraType type)
        {
            var handle = AssetLoadManager.Instance.LoadAsset(lod, lod.Address, lod.Priority, distance);
            lod.Handle = handle;
            handle.Completed += h =>
            {
                if(lod.CurrentState != State.Loading)
                {
                    AssetLoadManager.Instance.UnloadAsset(h);
                    return;
                }
                if (h.Status == AsyncOperationStatus.Failed)
                {
                    Debug.LogError("Failed to load asset: " + lod.Address);
                    h.Controller.CurrentState = State.Failed;
                    return;
                }

              
                GameObject gameObject = GameObject.Instantiate(h.Result, lodGroup.transform, false);
                h.Result = gameObject;
                gameObject.transform.parent = lodGroup.transform;
                h.Controller.CurrentState = State.Loaded;
                lodGroup.OnDisableAllLOD(type);

            };
            lod.CurrentState = State.Loading;
        }

        public static void UnLoaded(LOD lod)
        {
#if UNITY_EDITOR
            if(!Application.isPlaying)
                GameObject.DestroyImmediate(lod.Handle.Result);
            else
                GameObject.Destroy(lod.Handle.Result);
#else
            GameObject.Destroy(lod.Handle.Result);
#endif

            AssetLoadManager.Instance.UnloadAsset(lod.Handle);
            lod.Handle = null;
            lod.CurrentState = State.UnLoaded;
        }
    }
}
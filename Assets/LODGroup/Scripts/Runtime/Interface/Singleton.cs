using Chess.LODGroupIJob.LoadAsset;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Chess.LODGroupIJob.Interface
{
    public class Singleton<T>
    {
        public Singleton()
        {
            LoadAseetManager<ILoadAsset>.Instance.loadAsset = this as ILoadAsset;
        }
        ~Singleton()
        {
            LoadAseetManager<ILoadAsset>.Instance.loadAsset = null;
        }
    }
}


using Chess.LODGroupIJob.Interface;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Chess.LODGroupIJob.LoadAsset
{
    [ExecuteAlways]
    public class ResourcesLoadAsset : MonoBehaviour
    {
        LoadAsset m_LoadAsset;
        private void Awake()
        {
            m_LoadAsset = new LoadAsset();
        }
    }

    public class LoadAsset : ILoadAsset
    {
        uint id = 0;

        HashSet<uint> m_AllObjs = new HashSet<uint>();
        public override uint LoadAsync(string address, int priority, float distance, Action<uint, GameObject> action)
        {
            return 0;
        }

        public override uint LoadAsync(string address, Action<uint, GameObject> action)
        {
            id++;
            //”√Resources≤‚ ‘
            address = address.Replace("Assets/LODGroup/Resources/", "");
            address = address.Replace(".prefab", "");
            var request = Resources.LoadAsync<GameObject>(address);
            request.completed += h =>
            {
                action?.Invoke(id, request.asset as GameObject);
            };
            m_AllObjs.Add(id);
            return id;
        }

        public override bool UnloadAsset(uint id)
        {
            return m_AllObjs.Remove(id);
        }
    }

}

namespace Chess.LODGroupIJob
{
    public static class NormalLOD
    {
        //正常模式只有显示和隐藏
        public static void SetState(bool active, LOD lod, LODGroup lodGroup, int willLOD = -1)
        {
            switch (lod.CurrentState)
            {
                case State.None:
                case State.UnLoaded:
                case State.UnLoading:
                    if (active == true)
                    {
                        ChangeRendererState(active, lod);
                        lodGroup.OnDisableCurrentLOD(willLOD);
                    }
                    break;
                case State.Loaded:
                    if (active == false)
                    {
                        ChangeRendererState(active, lod);
                    }
                    break;
            }
        }
        public static void ChangeRendererState(bool state, LOD lod)
        {
            if (lod.Renderers == null)
                return;
            var renderers = lod.Renderers;
            var count = renderers.Length;
            for (int i = 0; i < count;i++)
            {
                var rd = renderers[i];
                if (rd != null)
                    rd.enabled = state;
            }
            if (lod.Colliers != null)
            {
                var colliders = lod.Colliers;
                count = renderers.Length;
                for (int i = 0; i < count; i++)
                {
                    var c = colliders[i];
                    if (c != null)
                        c.enabled = state;
                }
            }  

            lod.CurrentState = state == true ? State.Loaded : State.UnLoaded;
        }
    }
}
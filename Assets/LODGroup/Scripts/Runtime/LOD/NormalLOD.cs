namespace Chess.LODGroupIJob
{
    public static class NormalLOD
    {
        //正常模式只有显示和隐藏
        public static void SetState(bool active, LOD lod, LODGroup lodGroup)
        {
            switch (lod.CurrentState)
            {
                case State.None:
                case State.UnLoaded:
                    if (active == true)
                    {
                        ChangeRendererState(active, lod);
                        lodGroup.OnDisableAllLOD();
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

            foreach(var rd in lod.Renderers)
            {
                if(rd != null)
                    rd.enabled = state;
            }
            lod.CurrentState = state == true ? State.Loaded : State.UnLoaded;
        }
    }
}
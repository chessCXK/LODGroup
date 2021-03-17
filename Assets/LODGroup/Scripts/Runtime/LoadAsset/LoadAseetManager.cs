namespace Chess.LODGroupIJob.LoadAsset
{
    public class LoadAseetManager<T>
    {
        static LoadAseetManager<T> _Instance;
        public static LoadAseetManager<T> Instance
        {
            get
            {
                if(_Instance == null)
                {
                    _Instance = new LoadAseetManager<T>();
                }
                return _Instance;
            }
        }
        public T loadAsset;
    }
}
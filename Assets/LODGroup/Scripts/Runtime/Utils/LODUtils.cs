using UnityEngine;
namespace Chess.LODGroupIJob.Utils
{
    public sealed class LODUtils
    {
        public static Color[] kLODColors =
        {
            new Color(0.4831376f, 0.6211768f, 0.0219608f, 1.0f),
            new Color(0.2792160f, 0.4078432f, 0.5835296f, 1.0f),
            new Color(0.2070592f, 0.5333336f, 0.6556864f, 1.0f),
            new Color(0.5333336f, 0.1600000f, 0.0282352f, 1.0f),
            new Color(0.3827448f, 0.2886272f, 0.5239216f, 1.0f),
            new Color(0.8000000f, 0.4423528f, 0.0000000f, 1.0f),
            new Color(0.4486272f, 0.4078432f, 0.0501960f, 1.0f),
            new Color(0.7749016f, 0.6368624f, 0.0250984f, 1.0f)
        };
        public static string[] kLODNames =
        {
            "LOD 0",
            "LOD 1",
            "LOD 2",
            "LOD 3",
            "LOD 4",
            "LOD 5",
            "LOD 6",
            "LOD 7"
        };
        public static readonly string kLODCulled = "Culled";
        public static readonly Color kDefaultLODColor = new Color(.4f, 0f, 0f, 1f);
    }
}

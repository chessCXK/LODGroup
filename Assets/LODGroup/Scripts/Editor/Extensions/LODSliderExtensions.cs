using UnityEngine;
using Chess.LODGroupIJob.Slider;
using System.Collections.Generic;
namespace Chess.LODGroupIJob.Extensions
{
    public static class LODSliderEditorExtensions
    {
        //添加LOD,userData = Vector2(被选中的lod下标，选中的位置)
        public static void InsertBeforeOnClick(this LODSlider lodSlider, object userData)
        {
            var lodGroupEditor = lodSlider.LODGroupEditor;
            var lodGroup = lodGroupEditor.LODGroup;
            var lods = new  List<LOD>(lodGroup.GetLODs());

            Vector2 data = (Vector2)userData;
            
            LOD lod = new LOD(data.y);
            if(data.x == -1)
            {
                lod.Priority = lods.Count - 1;
                lods.Insert(lods.Count, lod);
            }
            else
            {
                lod.Priority = (int)data.x;
                lods.Insert((int)data.x, lod);
                lodSlider.SelectedShowIndex += 1;
               
            }
            lodGroup.SetLODs(lods.ToArray());
            lodGroupEditor.RefreshLOD();




        }
        //删除LOD， userData = int：被选中的下标
        public static void DeleteOnClick(this LODSlider lodSlider, object userData)
        {
            var lodGroupEditor = lodSlider.LODGroupEditor;
            var lodGroup = lodGroupEditor.LODGroup;
            var lods = new List<LOD>(lodGroup.GetLODs());

            int index = (int)userData;
            lods.RemoveAt(index);
            lodGroup.SetLODs(lods.ToArray());
            lodGroupEditor.RefreshLOD();
            if(lodSlider.SelectedShowIndex != -1)
            {
                lodSlider.SelectedShowIndex = Mathf.Clamp(index - 1, 0, index - 1);
            }

        }
    }
}
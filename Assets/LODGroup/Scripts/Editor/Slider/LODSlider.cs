using Chess.LODGroupIJob.Extensions;
using Chess.LODGroupIJob.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Chess.LODGroupIJob.Slider
{

    #region 框背景
    public class LODSlider
    {
        public const int k_SliderBarHeight = 30;
        
        class GUIStyles
        {
            public readonly GUIStyle LODSliderBG = "LODSliderBG";
            

            public GUIStyles()
            {

            }
        }
        private static GUIStyles Styles
        {
            get
            {
                if (s_Styles == null)
                    s_Styles = new GUIStyles();
                return s_Styles;
            }
        }
        private static GUIStyles s_Styles;
        #endregion

        private int m_SliderID = typeof(LODSlider).GetHashCode();

        //要滑动哪个框
        private int m_SelectedIndex = -1;
        //culled框
        private LODSliderRange m_DefaultRange = null;
        //所有框
        private List<LODSliderRange> m_RangeList = new List<LODSliderRange>();

        LODGroupEditor m_LODGroupEditor;
        //相机滑动条
        SlideCursor m_SlideCursor;

        //选中了lod显示区域
        private int m_SelectedShowIndex = -1;
        public int SelectedShowIndex { get => m_SelectedShowIndex; set => m_SelectedShowIndex = value; }

        //是否需要刷新包围盒
        private bool m_RefreshBounds;

        //是否拖动了框框
        private bool m_RefreshDrag;
        //被调用将m_RefreshBounds变False;
        public bool RefreshBounds
        {
            get
            {
                bool result = m_RefreshBounds;
                m_RefreshBounds = false;
                return result;
            }
        }
        
        //被调用将m_RefreshBounds变False;
        public bool RefreshDrag
        {
            get
            {
                bool result = m_RefreshDrag;
                m_RefreshDrag = false;
                return result;
            }
        }
        public SlideCursor SlideCursor { get => m_SlideCursor; set => m_SlideCursor = value; }
        public LODGroupEditor LODGroupEditor { get => m_LODGroupEditor; set => m_LODGroupEditor = value; }

        public LODSlider(LODGroupEditor lodGroupEditor, bool useDefault = false, string name = "")
        {
            m_LODGroupEditor = lodGroupEditor;
            if (useDefault == true)
            {
                var defaultRange = new LODSliderRange();
                defaultRange.Name = name;
                m_DefaultRange = defaultRange;
            }
            m_SlideCursor = new SlideCursor();
        }

        public void InsertRange(string name, LOD lod)
        {
            var range = new LODSliderRange();
            range.Name = name;
            range.LOD = lod;

            int insertPosition = 0;

            for (; insertPosition < m_RangeList.Count; ++insertPosition)
            {
                if (m_RangeList[insertPosition].EndPosition < range.EndPosition)
                {
                    break;
                }
            }

            m_RangeList.Insert(insertPosition, range);
        }

        public void ClearRange()
        {
            m_RangeList.Clear();
        }

       //获得选择哪个框
        int CursorSelectSliderRange(Rect sliderBarPosition, List<LODSliderRange> rangeList)
        {
            Event evt = Event.current;
            for (int i = 0; i < rangeList.Count; ++i)
            {
                float startPosition = 1.0f;
                Rect rect = m_RangeList[i].GetRect(sliderBarPosition, startPosition);
                if (rect.Contains(evt.mousePosition) == true)
                {
                    return i;
                }
                startPosition = m_RangeList[i].EndPosition;
            }
            return -1;
        }
        public void Updata(Event curEvent)
        {
            m_SlideCursor.Updata(curEvent);
        }
        public void Draw()
        {
            GUILayout.Space(25);
            var sliderBarPosition = GUILayoutUtility.GetRect(0, k_SliderBarHeight, GUILayout.ExpandWidth(true));
            sliderBarPosition.width -= 5;   //< for margin
            Draw(sliderBarPosition);
            GUILayout.Space(15);
        }
        public void Draw(Rect sliderBarPosition)
        {
            Event evt = Event.current;
            int sliderId = GUIUtility.GetControlID(m_SliderID, FocusType.Passive);

            //处理滑动条范围内
            switch (evt.GetTypeForControl(sliderId))
            {
                case EventType.Repaint:
                {
                    Repaint(sliderBarPosition, sliderId);
                    break;
                }

                case EventType.MouseDown:
                {
                    MouseDown(sliderBarPosition, sliderId);
                    break;
                }
                case EventType.MouseDrag:
                {
                    MouseDrag(sliderBarPosition, sliderId);
                    break;
                }
                case EventType.MouseUp:
                {
                    if (GUIUtility.hotControl == sliderId)
                    {
                        evt.Use();
                        m_SelectedIndex = -1;
                        m_RefreshDrag = true;
                        GUIUtility.hotControl = 0;    
                    }       
                    break;
                }
                case EventType.ContextClick:
                    ContextClick(sliderBarPosition, sliderId);
                    break;
            }

            //处理超出滑动条范围事件
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    DragUpdatedAndPerform(sliderBarPosition);
                    break;
            }
            m_SlideCursor.Draw(sliderBarPosition);
        }
        
        //画选中的框
        void DrawSelectLODRect(Rect sliderBarPosition, int selectIndex)
        {
            if (selectIndex < 0)
                return;

            float startPosition = selectIndex == 0 ? 1.0f : m_RangeList[selectIndex - 1].EndPosition;
            Rect rect = m_RangeList[selectIndex].GetRect(sliderBarPosition, startPosition);
            EditorGUI.DrawRect(rect, new Color(1, 1, 1, 0.2f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 2.5f, rect.height), new Color(0.11764f,0.5647f, 1, 1));
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 2.5f), new Color(0.11764f, 0.5647f, 1, 1));
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - 2.5f, rect.width, 2.5f), new Color(0.11764f, 0.5647f, 1, 1));
            EditorGUI.DrawRect(new Rect(rect.x + rect.width - 2.5f, rect.y, 2.5f, rect.height), new Color(0.11764f, 0.5647f, 1, 1));
        }

        //不间断画lod框
        void Repaint(Rect sliderBarPosition, int sliderId)
        {
            Styles.LODSliderBG.Draw(sliderBarPosition, GUIContent.none, false, false, false, false);

            float startPosition = 1.0f;
            for (int i = 0; i < m_RangeList.Count; ++i)
            {
                m_RangeList[i].Draw(sliderBarPosition, LODUtils.kLODColors[i], startPosition);

                if (GUI.enabled == true)
                {
                    if (i != m_RangeList.Count - 1 || m_DefaultRange != null)
                        m_RangeList[i].DrawCursor(sliderBarPosition);
                }

                startPosition = m_RangeList[i].EndPosition;
            }

            if (m_DefaultRange != null)
            {
                m_DefaultRange.Draw(sliderBarPosition, LODUtils.kDefaultLODColor, startPosition);
            }

            if (SelectedShowIndex >= 0)
            {
                DrawSelectLODRect(sliderBarPosition, m_SelectedShowIndex);
            }
            GUI.changed = true;
        }

        //处理鼠左键按下
        void MouseDown(Rect sliderBarPosition, int sliderId)
        {
            Event evt = Event.current;
            int count = m_RangeList.Count;
            if (m_DefaultRange == null)
                count -= 1;

            //判断鼠标是否在滑动分割区域内
            for (int i = 0; i < count; ++i)
            {
                Rect resizeArea = m_RangeList[i].GetResizeArea(sliderBarPosition);
                if (resizeArea.Contains(evt.mousePosition) == true)
                {
                    evt.Use();
                    GUIUtility.hotControl = sliderId;
                    m_SelectedIndex = i;
                    return;
                }
            }
            
            //如果鼠标不在滑动范围点在slider上那么就选中被点中的LOD
            if (!sliderBarPosition.Contains(evt.mousePosition))
            {
                return;
            }
            int index = CursorSelectSliderRange(sliderBarPosition, m_RangeList);
            SelectedShowIndex = index;
            if (SelectedShowIndex != -1)
            {
                evt.Use();
            }
        }
        //拖拽框框
        void MouseDrag(Rect sliderBarPosition, int sliderId)
        {
            Event evt = Event.current;

            //更新每个lod占比
            if (GUIUtility.hotControl == sliderId && m_SelectedIndex >= 0)
            {
                evt.Use();

                var percentage =
                    1.0f - Mathf.Clamp((evt.mousePosition.x - sliderBarPosition.x) / sliderBarPosition.width,
                        0.01f, 1.0f);
                percentage = (percentage * percentage);

                if (m_RangeList[m_SelectedIndex].LOD != null)
                {
                    m_RangeList[m_SelectedIndex].LOD.ScreenRelativeTransitionHeight = percentage;
                }
                GUI.changed = true;
            }
            if (!m_SlideCursor.Slide)
                return;

            //滑动状态选择滑动到的lod,高度占了整个屏幕是因为鼠标高度可以在整个屏幕的高度移动
            Rect rect = new Rect(sliderBarPosition.x, 0, sliderBarPosition.width, 10000);
            int index = CursorSelectSliderRange(rect, m_RangeList);
            m_SelectedShowIndex = index >= 0 ? index : m_SelectedShowIndex;
        }

        //拖拽物体到框框里
        void DragUpdatedAndPerform(Rect sliderBarPosition)
        {
            Event evt = Event.current;

            if (!sliderBarPosition.Contains(evt.mousePosition))
            {
                //不在框内
                return;
            }
            
            int index = CursorSelectSliderRange(sliderBarPosition, m_RangeList);
            LOD lod = m_RangeList[index].LOD;
            if (lod.Streaming)
                return;

            SelectedShowIndex = index;//选中
            //松开鼠标，鼠标如果有物体就将物体加入当前LOD并选中
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                GameObject obj;
                if(DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0)
                {
                    obj = DragAndDrop.objectReferences[0] as GameObject;
                    //将新的rederer加入到lod
                    var renderers = new List<Renderer>();
                    if (lod.Renderers != null)
                        renderers.AddRange(lod.Renderers);
                    foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
                    {
                        if (!renderers.Contains(renderer))
                        {
                            renderer.enabled = false;
                            renderers.Add(renderer);
                        }
                        m_RefreshBounds = true;
                        //EditorUtility.SetDirty(lod as Object);
                        SceneView.lastActiveSceneView.Repaint();
                    }
                    lod.Renderers = renderers.ToArray();
                    List<Collider> l = new List<Collider>();
                    foreach (var r in renderers)
                    {
                        var c = r.GetComponent<Collider>();
                        if (c)
                            l.Add(c);
                    }
                    lod.Colliers = l.ToArray();
                }
                
            }
            evt.Use();
        }

        //处理右键按下框框，然后添加或删除lod操作
        void ContextClick(Rect sliderBarPosition, int sliderId)
        {
            Event evt = Event.current;
            if (!sliderBarPosition.Contains(evt.mousePosition))
            {
                //不在框内
                return;
            }
            int index = CursorSelectSliderRange(sliderBarPosition, m_RangeList);
            float relative = 0;
            if (index == -1)
            {
                relative = m_DefaultRange.GetRelativeHeight(sliderBarPosition, evt.mousePosition.x);
            }
            else
            {
                relative = m_RangeList[index].GetRelativeHeight(sliderBarPosition, evt.mousePosition.x);
            }

            GenericMenu menu = new GenericMenu();
            if(Utils.LODUtils.kLODColors.Length == m_RangeList.Count)
            {
                //满了
                menu.AddItem(new GUIContent("Insert Before"), false, null, null);
            }
            else
            {
                menu.AddItem(new GUIContent("Insert Before"), false, this.InsertBeforeOnClick, new Vector2(index, relative));
            }
           
            menu.AddSeparator("");
            if(index == -1 || m_RangeList.Count == 1)
            {
                //Culled不可删除
                menu.AddItem(new GUIContent("Delete"), false, null, null);
            }
            else
            {
                menu.AddItem(new GUIContent("Delete"), false, this.DeleteOnClick, index);
            }
            menu.ShowAsContext();

            //设置该事件被使用
            Event.current.Use();
        }
    }
}
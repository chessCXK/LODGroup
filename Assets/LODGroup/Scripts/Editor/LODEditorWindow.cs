using UnityEditor;
using UnityEngine;

namespace Chess.LODGroupIJob
{
    public class LODEditorWindow : Editor
    {
        protected Rect DrawHeader(string title, ref string searchString, float searchWidth, bool center)
        {
            return DrawHeader(new GUIContent(title), ref searchString, searchWidth, center);
        }
        protected Rect DrawHeader(GUIContent title, ref string searchString, float searchWidth, bool center)
        {
            Rect headerRect = EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUIStyle headerStyle = null;
            headerStyle = new GUIStyle(EditorStyles.toolbar);
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = center ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
            GUIStyle searchBoxStyle = GUI.skin.FindStyle("ToolbarSeachTextField");
            GUIStyle searchCancelStyle = GUI.skin.FindStyle("ToolbarSeachCancelButton");
            if (center)
            {
                GUILayout.FlexibleSpace();
            }
            GUILayout.Label(title, headerStyle);
            if (searchString != null)
            {
                string controlName = "ValueFld" + GUIUtility.GetControlID(FocusType.Keyboard);
                GUI.SetNextControlName(controlName);
                searchString = EditorGUILayout.TextField(searchString, searchBoxStyle, GUILayout.Width(searchWidth));
                if (GUILayout.Button("", searchCancelStyle))
                {
                    searchString = "";
                    GUI.FocusControl(null);
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            return headerRect;
        }
    }
}
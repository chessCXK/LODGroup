using Chess.LODGroupIJob.Slider;
using Chess.LODGroupIJob.SpaceManager;
using UnityEditor;
using UnityEngine;
namespace Chess.LODGroupIJob
{
    public class SceneCameraManager
    {
        LODGroup m_LODGroup;
        SlideCursor m_SlideCursor;
        float m_PreRelative;

        public SceneCameraManager(LODGroup LODGroup, SlideCursor slideCursor)
        {
            m_LODGroup = LODGroup;
            m_SlideCursor = slideCursor;
        }

        public SceneView SceneView { get => SceneView.lastActiveSceneView;}

        public void OnInspectorGUI()
        {
            if (SceneView.camera == null)
                return;
            if (m_SlideCursor.Slide)
            {
                //在拖动滑动条上的相机
                Camera camera = SceneView.camera;
    
                QuadTreeSpaceManager.SettingReCamera(SceneView.camera, out m_PreRelative);
                float distance = QuadTreeSpaceManager.GetReDistance(m_LODGroup.Bounds, m_PreRelative, m_SlideCursor.RelativeHeight);

                var worldPos = m_LODGroup.transform.position + m_LODGroup.localReferencePoint;
                worldPos = worldPos - camera.transform.forward * distance;
                SceneView.lastActiveSceneView.LookAt(worldPos, camera.transform.rotation, 0.0001f, SceneView.orthographic, true);
                SceneView.lastActiveSceneView.Repaint();
            }
            else
            {
             
                QuadTreeSpaceManager.SettingCamera(m_LODGroup.transform, SceneView.camera, out m_PreRelative);
                m_SlideCursor.RelativeHeight = QuadTreeSpaceManager.GetRelativeHeight(m_LODGroup.Bounds, m_LODGroup.transform.position, m_PreRelative, SceneView.camera.transform.position);
            }
        }
    }
}


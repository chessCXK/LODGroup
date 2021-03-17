using UnityEngine;

namespace Chess.LODGroupIJob.SpaceManager
{
    public static class QuadTreeSpaceManager
    {
        //bounds的值已经是世界坐标的了，camPosition也是世界坐标
        public static Vector2 SettingCameraJob
                        (bool orthographic,
                        float orthographicSize, 
                        float fieldOfView,
                        float lodBias,
                        Bounds bounds,
                        Vector3 camPosition
                        )
        {
            Vector2 result;
            float preRelative;
            if (orthographic)
            {
                preRelative = 0.5f / orthographicSize;
            }
            else
            {
                float halfAngle = Mathf.Tan(Mathf.Deg2Rad * fieldOfView * 0.5F);
                preRelative = 0.5f / halfAngle;
            }
            preRelative = preRelative * lodBias;
            result.y = GetDistance(bounds.center, camPosition);
            result.x = bounds.size * preRelative / result.y;
            return result;
        }
        public static void SettingCamera(Transform lodTransform, Camera cam, out float preRelative)
        {
            if (cam.orthographic)
            {
                preRelative = 0.5f / cam.orthographicSize;
            }
            else
            {
                float halfAngle = Mathf.Tan(Mathf.Deg2Rad * cam.fieldOfView * 0.5F);
                preRelative = 0.5f / halfAngle;
            }
            preRelative = preRelative * QualitySettings.lodBias;
        }

        public static float GetRelativeHeight(Bounds bounds, Vector3 lodGroupPos, float preRelative, Vector3 camPosition)
        {
            float distance = GetDistance(lodGroupPos + bounds.center, camPosition);
            float relativeHeight = bounds.size * preRelative / distance;
            return relativeHeight;
        }

        private static float GetDistance(Vector3 boundsPos, Vector3 camPos)
        {
            return (boundsPos - camPos).magnitude;
        }

        //上面的逆向
        public static void SettingReCamera(Camera cam, out float preRelative)
        {
            if (cam.orthographic)
            {
                preRelative = 0.5f * cam.orthographicSize;
            }
            else
            {
                float halfAngle = Mathf.Tan(Mathf.Deg2Rad * (90 - cam.fieldOfView * 0.5F));
                preRelative = 0.5f * halfAngle;
            }
            preRelative = preRelative * QualitySettings.lodBias;
        }
        public static float GetReDistance(Bounds bounds, float preRelative, float relativeHeight)
        {
            float distance = bounds.size * preRelative / relativeHeight;
            return distance;
        }
    }

}
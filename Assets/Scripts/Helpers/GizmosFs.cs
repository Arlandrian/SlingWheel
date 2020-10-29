using UnityEngine;

namespace SlingWheel.Utils
{
    public static class GizmosFs
    {
        public static void DrawArrow(Vector3 from, Vector3 to, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawLine(from, to);
            var direction = to - from;
            if (direction == Vector3.zero)
                return;

            var right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            var left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawLine(to, to + right * arrowHeadLength);
            Gizmos.DrawLine(to, to + left * arrowHeadLength);
        }

        public static void DrawArrowDir(Vector3 from, Vector3 dir, float length = 2, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            DrawArrow(from, from + (dir * length), arrowHeadLength, arrowHeadAngle);
        }
    }
    
}

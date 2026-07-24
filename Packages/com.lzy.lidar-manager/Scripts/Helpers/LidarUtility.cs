// https://github.com/wangyangwang/hokuyo-unity

using UnityEngine;
using UnityEngine.UI;

namespace LZY.Lidar
{
    public static class LidarUtility
    {
        public static Vector2 MapRectPoint(Vector2 point, Vector2 originRect, Vector2 targetRect)
        {
            return MapRectPoint(point, originRect.x, originRect.y, targetRect.x, targetRect.y);
        }
        
        public static Vector2 MapRectPoint(Vector2 point, float originWidth, float originHeight, float targetWidth, float targetHeight)
        {
            var scaleX = targetWidth / originWidth;
            var scaleY = targetHeight / originHeight;

            var newX = point.x * scaleX;
            var newY = point.y * scaleY;

            return new Vector2(newX, newY);
        }
        
        public static void DrawWireSquare(VertexHelper vh, Vector2 point, float size, ref int i, float thickness = 1f, Color color = default, Vector2 offset = default)
        {
            DrawWireRectangle(vh, point, new Vector2(size, size), ref i, thickness, color, offset);
        }
        
        public static void DrawWireRectangle(VertexHelper vh, Vector2 point, Vector2 rectSize, ref int i, float thickness = 1f, Color color = default, Vector2 offset = default)
        {
            var halfWidth = rectSize.x / 2f;
            var halfHeight = rectSize.y / 2f;
            var points = new Vector2[]
            {
                point + new Vector2(-halfWidth, halfHeight),
                point + new Vector2(halfWidth, halfHeight),
                point + new Vector2(halfWidth, -halfHeight),
                point + new Vector2(-halfWidth, -halfHeight)
            };

            for (int j = 0; j < points.Length; j++)
                CreateLine(vh, points[j], points[(j + 1) % points.Length], i + j, thickness, color, offset);
            i += points.Length;
        }

        public static void CreateLine(VertexHelper vh, Vector2 startPoint, Vector2 endPoint, int i = 0, float thickness = 1f, Color color = default, Vector2 offset = default)
        {
            // Create a line segment between the next two points
            CreateLineSegment(startPoint, endPoint, vh, thickness, color, offset);

            int index = i * 5;

            // Add the line segment to the triangles array
            vh.AddTriangle(index, index+1, index+3);
            vh.AddTriangle(index+3, index+2, index);

            // These two triangles create the beveled edges
            // between line segments using the end point of
            // the last line segment and the start points of this one
            // if (i != 0)
            // {
            //     vh.AddTriangle(index, index-1, index-3);
            //     vh.AddTriangle(index+1, index-1, index-2);
            // }
        }

        /// <summary>
        /// Creates a rect from two points that acts as a line segment
        /// </summary>
        /// <param name="point1">The starting point of the segment</param>
        /// <param name="point2">The endint point of the segment</param>
        /// <param name="vh">The vertex helper that the segment is added to</param>
        public static void CreateLineSegment(Vector3 point1, Vector3 point2, VertexHelper vh, float thickness = 1f, Color color = default, Vector2 offset = default)
        {
            // Create vertex template
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            // Create the start of the segment
            Quaternion point1Rotation = Quaternion.Euler(0, 0, RotatePointTowards(point1, point2) + 90);
            vertex.position = point1Rotation * new Vector3(-thickness / 2, 0);
            vertex.position += (point1 - (Vector3)offset);
            vh.AddVert(vertex);
            vertex.position = point1Rotation * new Vector3(thickness / 2, 0);
            vertex.position += point1 - (Vector3)offset;
            vh.AddVert(vertex);

            // Create the end of the segment
            Quaternion point2Rotation = Quaternion.Euler(0, 0, RotatePointTowards(point2, point1) - 90);
            vertex.position = point2Rotation * new Vector3(-thickness / 2, 0);
            vertex.position += point2 - (Vector3)offset;
            vh.AddVert(vertex);
            vertex.position = point2Rotation * new Vector3(thickness / 2, 0);
            vertex.position += point2 - (Vector3)offset;
            vh.AddVert(vertex);

            // Also add the end point
            vertex.position = point2 - (Vector3)offset;
            vh.AddVert(vertex);
        }

        /// <summary>
        /// Gets the angle that a vertex needs to rotate to face target vertex
        /// </summary>
        /// <param name="vertex">The vertex being rotated</param>
        /// <param name="target">The vertex to rotate towards</param>
        /// <returns>The angle required to rotate vertex towards target</returns>
        public static float RotatePointTowards(Vector2 vertex, Vector2 target)
        {
            return (float)(Mathf.Atan2(target.y - vertex.y, target.x - vertex.x) * (180 / Mathf.PI));
        }

        public static Vector2 CalculatePivotOffset(RectTransform rectTransform)
        {
            return new Vector2(rectTransform.sizeDelta.x * rectTransform.pivot.x, rectTransform.sizeDelta.y * rectTransform.pivot.y);
        }
    }
}

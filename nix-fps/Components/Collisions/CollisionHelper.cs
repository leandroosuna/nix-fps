using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nixfps.NixFPS;

namespace nixfps.Components.Collisions
{
    public static class CollisionHelper
    {
        public static bool IsTriangleIntersectingAABB(CollisionTriangle triangle, BoundingBox aabb)
        {
            // Step 1: Check if the triangle’s points are inside the AABB
            if (IsPointInAABB(triangle.v[0], aabb) ||
                IsPointInAABB(triangle.v[1], aabb) ||
                IsPointInAABB(triangle.v[2], aabb))
            {
                return true;
            }

            // Step 2: Check for overlap between triangle and AABB on all axes
            Vector3[] aabbAxes = { Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ };

            // Test overlap on AABB's X, Y, Z axes
            foreach (var axis in aabbAxes)
            {
                if (!OverlapsOnAxis(triangle, aabb, axis))
                    return false; // Separation found
            }

            // Step 3: Check triangle's normal as a separating axis
            Vector3 triangleNormal = triangle.GetNormal();
            if (!OverlapsOnAxis(triangle, aabb, triangleNormal))
                return false; // Separation found

            // Step 4: Check cross products of triangle edges and AABB axes
            Vector3[] triangleEdges = { triangle.v[1] - triangle.v[0], triangle.v[2] - triangle.v[1], triangle.v[0] - triangle.v[2] };

            foreach (var edge in triangleEdges)
            {
                foreach (var axis in aabbAxes)
                {
                    Vector3 crossAxis = Vector3.Cross(edge, axis);
                    if (!OverlapsOnAxis(triangle, aabb, crossAxis))
                        return false; // Separation found
                }
            }

            // If no separation axis is found, the triangle and AABB are intersecting
            return true;
        }

        private static bool IsPointInAABB(Vector3 point, BoundingBox aabb)
        {
            return (point.X >= aabb.Min.X && point.X <= aabb.Max.X &&
                    point.Y >= aabb.Min.Y && point.Y <= aabb.Max.Y &&
                    point.Z >= aabb.Min.Z && point.Z <= aabb.Max.Z);
        }

        private static bool OverlapsOnAxis(CollisionTriangle triangle, BoundingBox aabb, Vector3 axis)
        {
            // Project the AABB onto the axis
            (float minAABB, float maxAABB) = ProjectAABBOnAxis(aabb, axis);

            // Project the triangle onto the same axis
            (float minTriangle, float maxTriangle) = ProjectTriangleOnAxis(triangle, axis);

            // Check if the projections overlap
            return !(minTriangle > maxAABB || maxTriangle < minAABB);
        }

        private static (float, float) ProjectAABBOnAxis(BoundingBox aabb, Vector3 axis)
        {
            // Project all 8 corners of the AABB onto the axis and get min/max values
            Vector3[] corners = aabb.GetCorners();
            float min = Vector3.Dot(corners[0], axis);
            float max = min;

            for (int i = 1; i < corners.Length; i++)
            {
                float projection = Vector3.Dot(corners[i], axis);
                min = Math.Min(min, projection);
                max = Math.Max(max, projection);
            }

            return (min, max);
        }

        private static (float, float) ProjectTriangleOnAxis(CollisionTriangle triangle, Vector3 axis)
        {
            // Project each vertex of the triangle onto the axis
            float p1 = Vector3.Dot(triangle.v[0], axis);
            float p2 = Vector3.Dot(triangle.v[1], axis);
            float p3 = Vector3.Dot(triangle.v[2], axis);

            float min = Math.Min(p1, Math.Min(p2, p3));
            float max = Math.Max(p1, Math.Max(p2, p3));

            return (min, max);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace nixfps.Components.Collisions
{
    /// <summary>
    ///     Class that extends BoundingVolumes classes
    /// </summary>
    public static class BoundingVolumesExtensions
    {
        /// <summary>
        ///     Get an extents vector that contains the half-size on each axis of the box.
        /// </summary>
        /// <param name="box">A <see cref="BoundingBox"/> to calculate its extents</param>
        /// <returns>The extents on each axis</returns>
        public static Vector3 GetExtents(BoundingBox box)
        {
            var max = box.Max;
            var min = box.Min;

            return (max - min) * 0.5f;            
        }

        /// <summary>
        ///     Gets the total volume of the box in units.
        /// </summary>
        /// <param name="box">A <see cref="BoundingBox"/> to calculate its volume</param>
        /// <returns>The volume of the box in units</returns>
        public static float GetVolume(BoundingBox box)
        {
            var difference = box.Max - box.Min;
            return difference.X * difference.Y * difference.Z;
        }

        /// <summary>
        ///     Gets the center position of the box.
        /// </summary>
        /// <param name="box">A <see cref="BoundingBox"/> to calculate its center</param>
        /// <returns>The position of the center of the box.</returns>
        public static Vector3 GetCenter(BoundingBox box)
        {
            return (box.Max + box.Min) * 0.5f;
        }


        /// <summary>
        ///     Scales the box by a given scalar.
        /// </summary>
        /// <param name="scale">The scale for every axis</param>
        /// <param name="box">The <see cref="BoundingBox"/> to scale</param>
        /// <returns>A new box with its extents scaled</returns>
        public static BoundingBox Scale(BoundingBox box, float scale)
        {
            var center = GetCenter(box);
            var extents = GetExtents(box);
            var scaledExtents = extents * scale;

            return new BoundingBox(center - scaledExtents, center + scaledExtents);
        }

        /// <summary>
        ///     Scales the box by a given scalar per axis.
        /// </summary>
        /// <param name="box">The <see cref="BoundingBox"/> to calculate its scale</param>
        /// <param name="scale">The scale for each axis</param>
        /// <returns>A new <see cref="BoundingBox">BoundingBox</see> with its extents scaled</returns>
        public static BoundingBox Scale(BoundingBox box, Vector3 scale)
        {
            var center = GetCenter(box);
            var extents = GetExtents(box);
            var scaledExtents = extents * scale;

            return new BoundingBox(center - scaledExtents, center + scaledExtents);
        }

        /// <summary>
        ///     Gets the closest point to the box.
        /// </summary>
        /// <param name="box">A <see cref="BoundingBox"/> to calculate the closest point</param>
        /// <param name="point">The point to find the closest point from</param>
        /// <returns>The position inside the box that is closer to the given point</returns>
        public static Vector3 ClosestPoint(BoundingBox box, Vector3 point)
        {
            var min = box.Min;
            var max = box.Max;
            point.X = MathHelper.Clamp(point.X, min.X, max.X);
            point.Y = MathHelper.Clamp(point.Y, min.Y, max.Y);
            point.Z = MathHelper.Clamp(point.Z, min.Z, max.Z);
            return point;
        }

        /// <summary>
        ///     Gets the normal vector from a point in the box surface.
        /// </summary>
        /// <param name="box">A <see cref="BoundingBox"/> to calculate the normal</param>
        /// <param name="point">The point in the surface of the box</param>
        /// <returns>The normal vector of the surface in which the point is in</returns>
        public static Vector3 GetNormalFromPoint(BoundingBox box, Vector3 point)
        {
            var normal = Vector3.Zero;
            var min = float.MaxValue;

            point -= GetCenter(box);
            var extents = GetExtents(box);

            var distance = MathF.Abs(extents.X - Math.Abs(point.X));
            if (distance < min)
            {
                min = distance;
                normal = Math.Sign(point.X) * Vector3.UnitX;
                // Cardinal axis for X            
            }
            distance = Math.Abs(extents.Y - Math.Abs(point.Y));
            if (distance < min)
            {
                min = distance;
                normal = Math.Sign(point.Y) * Vector3.UnitY;
                // Cardinal axis for Y            
            }
            distance = Math.Abs(extents.Z - Math.Abs(point.Z));
            if (distance < min)
            {
                normal = Math.Sign(point.Z) * Vector3.UnitZ;
                // Cardinal axis for Z            
            }
            return normal;
        }

        /// <summary>
        ///     Creates a <see cref="BoundingBox">BoundingBox</see> from a Matrix.
        /// </summary>
        /// <param name="matrix">The Matrix that describes a transformation to apply to each point of a box</param>
        /// <returns>The <see cref="BoundingBox">BoundingBox</see> created from the Matrix</returns>
        public static BoundingBox FromMatrix(Matrix matrix)
        {
            return new BoundingBox(Vector3.Transform(-Vector3.One * 0.5f, matrix), Vector3.Transform(Vector3.One * 0.5f, matrix));
        }

        /// <summary>
        ///     Creates a <see cref="BoundingBox">BoundingBox</see> from a Model, using all its sub-meshes.
        /// </summary>
        /// <param name="model">The model to create the box</param>
        /// <returns>The <see cref="BoundingBox">BoundingBox</see> that encloses the vertices from the model</returns>
        public static BoundingBox CreateAABBFrom(Model model)
        {
            var minPoint = Vector3.One * float.MaxValue;
            var maxPoint = Vector3.One * float.MinValue;

            var transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            var meshes = model.Meshes;
            for (int index = 0; index < meshes.Count; index++)
            {
                var meshParts = meshes[index].MeshParts;
                for (int subIndex = 0; subIndex < meshParts.Count; subIndex++)
                {
                    var vertexBuffer = meshParts[subIndex].VertexBuffer;
                    var declaration = vertexBuffer.VertexDeclaration;
                    var vertexSize = declaration.VertexStride / sizeof(float);

                    var rawVertexBuffer = new float[vertexBuffer.VertexCount * vertexSize];
                    vertexBuffer.GetData(rawVertexBuffer);

                    for (var vertexIndex = 0; vertexIndex < rawVertexBuffer.Length; vertexIndex += vertexSize)
                    {
                        var transform = transforms[meshes[index].ParentBone.Index];
                        var vertex = new Vector3(rawVertexBuffer[vertexIndex], rawVertexBuffer[vertexIndex + 1], rawVertexBuffer[vertexIndex + 2]);
                        vertex = Vector3.Transform(vertex, transform);
                        minPoint = Vector3.Min(minPoint, vertex);
                        maxPoint = Vector3.Max(maxPoint, vertex);
                    }
                }
            }
            return new BoundingBox(minPoint, maxPoint);
        }

        /// <summary>
        ///     Creates a <see cref="BoundingSphere">BoundingSphere</see> from a Model, using all its sub-meshes.
        /// </summary>
        /// <param name="model">The model to create the sphere</param>
        /// <returns>The <see cref="BoundingSphere">BoundingSphere</see> which radius encloses the vertices from the model</returns>
        public static BoundingSphere CreateSphereFrom(Model model)
        {
            var minPoint = Vector3.One * float.MaxValue;
            var maxPoint = Vector3.One * float.MinValue;

            var transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            var meshes = model.Meshes;
            for (var index = 0; index < meshes.Count; index++)
            {
                var meshParts = meshes[index].MeshParts;
                for (var subIndex = 0; subIndex < meshParts.Count; subIndex++)
                {
                    var vertexBuffer = meshParts[subIndex].VertexBuffer;
                    var declaration = vertexBuffer.VertexDeclaration;
                    int vertexSize = declaration.VertexStride / sizeof(float);

                    var rawVertexBuffer = new float[vertexBuffer.VertexCount * vertexSize];
                    vertexBuffer.GetData(rawVertexBuffer);

                    for (var vertexIndex = 0; vertexIndex < rawVertexBuffer.Length; vertexIndex += vertexSize)
                    {
                        var transform = transforms[meshes[index].ParentBone.Index];
                        var vertex = new Vector3(rawVertexBuffer[vertexIndex], rawVertexBuffer[vertexIndex + 1], rawVertexBuffer[vertexIndex + 2]);
                        vertex = Vector3.Transform(vertex, transform);
                        minPoint = Vector3.Min(minPoint, vertex);
                        maxPoint = Vector3.Max(maxPoint, vertex);
                    }
                }
            }
            var difference = (maxPoint - minPoint) * 0.5f;
            return new BoundingSphere(difference, difference.Length());
        }
        public static bool IntersectSegmentTriangle(Vector3 p, Vector3 q, Vector3 a, Vector3 b, Vector3 c,
            out Vector3 uvw, out float t, out Vector3 col)
        {
            float u;
            float v;
            float w;
            uvw = Vector3.Zero;
            col = Vector3.Zero;
            t = -1;

            var ab = b - a;
            var ac = c - a;
            var qp = p - q;

            // Compute triangle normal. Can be precalculated or cached if intersecting multiple segments against the same triangle
            var n = Vector3.Cross(ab, ac);

            // Compute denominator d. If d <= 0, segment is parallel to or points away from triangle, so exit early
            var d = Vector3.Dot(qp, n);
            if (d <= 0.0f) return false;

            // Compute intersection t value of pq with plane of triangle. A ray intersects iff 0 <= t.
            // Segment intersects iff 0 <= t <= 1. Delay dividing by d until intersection has been found to pierce triangle
            var ap = p - a;
            t = Vector3.Dot(ap, n);
            if (t < 0.0f) return false;
            if (t > d) return false; // For segment; exclude this code line for a ray test

            // Compute barycentric coordinate components and test if within bounds
            var e = Vector3.Cross(qp, ap);
            v = Vector3.Dot(ac, e);
            if (v < 0.0f || v > d) return false;
            w = -Vector3.Dot(ab, e);
            if (w < 0.0f || v + w > d) return false;

            // Segment/ray intersects triangle. Perform delayed division and compute the last barycentric coordinate component
            var ood = 1.0f / d;
            t *= ood;
            v *= ood;
            w *= ood;
            u = 1.0f - v - w;

            uvw.X = u;
            uvw.Y = v;
            uvw.Z = w;
            col = p + t * (p - q);
            return true;
        }
        public static bool IntersectRayTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Ray ray)
        {
            // Vectors from p1 to p2/p3 (edges)
            Vector3 e1, e2;

            Vector3 p, q, t;
            float det, invDet, u, v;


            //Find vectors for two edges sharing vertex/point p1
            e1 = p2 - p1;
            e2 = p3 - p1;

            // calculating determinant
            p = Vector3.Cross(ray.Direction, e2);

            //Calculate determinat
            det = Vector3.Dot(e1, p);

            //if determinant is near zero, ray lies in plane of triangle otherwise not
            if (det > -0.0001f && det < 0.0001f) { return false; }
            invDet = 1.0f / det;

            //calculate distance from p1 to ray origin
            t = ray.Position - p1;

            //Calculate u parameter
            u = Vector3.Dot(t, p) * invDet;

            //Check for ray hit
            if (u < 0 || u > 1) { return false; }

            //Prepare to test v parameter
            q = Vector3.Cross(t, e1);

            //Calculate v parameter
            v = Vector3.Dot(ray.Direction, q) * invDet;

            //Check for ray hit
            if (v < 0 || u + v > 1) { return false; }

            if ((Vector3.Dot(e2, q) * invDet) > 0.0001f)
            {
                //ray does intersect
                return true;
            }

            // No hit at all
            return false;
        }
    }
}

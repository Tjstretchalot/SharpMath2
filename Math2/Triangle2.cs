using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace SharpMath2
{
    /// <summary>
    /// Describes a triangle, which is a collection of three points. This is
    /// used for the implementation of the Polygon2.
    /// </summary>
    public class Triangle2
    {
        /// <summary>
        /// The 3 vertices of this triangle.
        /// </summary>
        public Vector2[] Vertices;

        /// <summary>
        /// This is used to determine if points are inside the triangle.
        /// This has 4 values where the first 2 correspond to row 1 and
        /// the second 2 to row 2 of a 2x2 matrix. When that matrix is
        /// matrix-multiplied by a point, if the result has a sum less
        /// than 1 and each component is positive, the point is in the
        /// triangle.
        /// </summary>
        private float[] InvContainsBasis;

        /// <summary>
        /// The centroid of the triangle
        /// </summary>
        public readonly Vector2 Center;

        /// <summary>
        /// The edges of the triangle, where the first edge is from 
        /// Vertices[0] to Vertices[1], etc.
        /// </summary>
        public readonly Line2[] Edges;

        /// <summary>
        /// The area of the triangle.
        /// </summary>
        public readonly float Area;

        /// <summary>
        /// Constructs a triangle with the given vertices, assuming that
        /// the vertices define a triangle (i.e., are not collinear)
        /// </summary>
        /// <param name="vertices">The vertices of the triangle</param>
        public Triangle2(Vector2[] vertices)
        {
            Vertices = vertices;

            Vector2 vertSum = Vector2.Zero;
            for(int i = 0; i < 3; i++)
            {
                vertSum += vertices[i];
            }

            Center = vertSum / 3.0f;
            float a = vertices[1].X - vertices[0].X;
            float b = vertices[2].X - vertices[0].X;
            float c = vertices[1].Y - vertices[0].Y;
            float d = vertices[2].Y - vertices[0].Y;

            float det = a * d - b * c;
            Area = 0.5f * det;

            float invDet = 1 / det;
            InvContainsBasis = new float[4]
            {
                invDet * d, -invDet * b, 
                -invDet * c, invDet * a
            };

            Edges = new Line2[]
            {
                new Line2(Vertices[0], Vertices[1]),
                new Line2(Vertices[1], Vertices[2]),
                new Line2(Vertices[2], Vertices[0])
            };
        }

        /// <summary>
        /// Checks if this triangle contains the given point. This is
        /// never strict.
        /// </summary>
        /// <param name="tri">The triangle</param>
        /// <param name="pos">The position of the triangle</param>
        /// <param name="pt">The point to check</param>
        /// <returns>true if this triangle contains the point or the point
        /// is along an edge of this polygon</returns>
        public static bool Contains(Triangle2 tri, Vector2 pos, Vector2 pt)
        {
            Vector2 relPt = pt - pos - tri.Vertices[0];
            float r = tri.InvContainsBasis[0] * relPt.X + tri.InvContainsBasis[1] * relPt.Y;
            if (r < -Math2.DEFAULT_EPSILON)
                return false;

            float t = tri.InvContainsBasis[2] * relPt.X + tri.InvContainsBasis[3] * relPt.Y;
            if (t < -Math2.DEFAULT_EPSILON)
                return false;

            return (r + t) < 1 + Math2.DEFAULT_EPSILON;
        }
    }
}

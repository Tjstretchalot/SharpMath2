using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SharpMath2
{
    /// <summary>
    /// Describes a simple polygon based on it's vertices. Does not
    /// have position - most functions require specifying the origin of the
    /// polygon. Polygons are meant to be reused.
    /// </summary>
    public class Polygon2 : Shape2
    {
        /// <summary>
        /// The vertices of this polygon, in order
        /// </summary>
        public readonly Vector2[] Vertices;

        /// <summary>
        /// The three normal vectors of this polygon, normalized
        /// </summary>
        public readonly List<Vector2> Normals;

        /// <summary>
        /// Initializes a polygon with the specified vertices
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <exception cref="ArgumentNullException">If vertices is null</exception>
        public Polygon2(Vector2[] vertices)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            Vertices = vertices;

            Normals = new List<Vector2>();
            Vector2 tmp;
            for(int i = 1; i < vertices.Length; i++)
            {
                tmp = Vector2.Normalize(Vector2.Perpendicular(vertices[i] - vertices[i - 1]));

                if (!Normals.Contains(tmp))
                    Normals.Add(tmp);
            }

            tmp = Vector2.Normalize(Vector2.Perpendicular(vertices[0] - vertices[vertices.Length - 1]));
            if (!Normals.Contains(tmp))
                Normals.Add(tmp);
        }
        
        /// <summary>
        /// Determines if the first polygon intersects the second polygon when polygon one
        /// is at position 1 and polygon two is at position two.
        /// </summary>
        /// <param name="poly1">polygon one</param>
        /// <param name="poly2">polygon two</param>
        /// <param name="pos1">Position one</param>
        /// <param name="pos2">Position two</param>
        /// <param name="strict">If overlapping is required for intersection</param>
        /// <returns>If poly1 at pos1 intersects poly2 at pos2</returns>
        public static bool Intersects(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2, bool strict)
        {
            foreach(var axis in poly1.Normals.Union(poly2.Normals))
            {
                if (!IntersectsAlongAxis(poly1, poly2, pos1, pos2, strict, axis))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines the mtv to move pos1 by to prevent poly1 at pos1 from intersecting poly2 at pos2.
        /// Returns null if poly1 and poly2 do not intersect.
        /// </summary>
        /// <param name="poly1">polygon 1</param>
        /// <param name="poly2">polygon 2</param>
        /// <param name="pos1">polygon 1 origin</param>
        /// <param name="pos2">polygon 2 origin</param>
        /// <returns>MTV for polygon 1</returns>
        public static Vector2? IntersectMTV(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2)
        {
            Vector2? bestAxis = null;
            float? bestMagn = null;

            foreach(var axis in poly1.Normals.Union(poly2.Normals))
            {
                var mtv = IntersectMTVAlongAxis(poly1, poly2, pos1, pos2, axis);
                if (!mtv.HasValue)
                    return null;
                else if (!bestAxis.HasValue || bestMagn.Value > mtv.Value)
                {
                    bestAxis = axis;
                    bestMagn = mtv;
                }
            }

            return Vector2.ScalarMult(bestAxis.Value, bestMagn.Value);
        }

        /// <summary>
        /// Determines if polygon 1 and polygon 2 at position 1 and position 2, respectively, intersect along axis.
        /// </summary>
        /// <param name="poly1">polygon 1</param>
        /// <param name="poly2">polygon 2</param>
        /// <param name="pos1">Origin of polygon 1</param>
        /// <param name="pos2">Origin of polygon 2</param>
        /// <param name="strict">If overlapping is required for intersection</param>
        /// <param name="axis">The axis to check</param>
        /// <returns>If poly1 at pos1 intersects poly2 at pos2 along axis</returns>
        public static bool IntersectsAlongAxis(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2, bool strict, Vector2 axis)
        {
            var proj1 = ProjectAlongAxis(poly1, pos1, axis);
            var proj2 = ProjectAlongAxis(poly2, pos2, axis);

            return AxisAlignedLine2.Intersects(proj1, proj2, strict);
        }

        /// <summary>
        /// Determines the distance along axis, if any, that polygon 1 should be shifted by
        /// to prevent intersection with polygon 2. Null if no intersection along axis.
        /// </summary>
        /// <param name="poly1">polygon 1</param>
        /// <param name="poly2">polygon 2</param>
        /// <param name="pos1">polygon 1 origin</param>
        /// <param name="pos2">polygon 2 origin</param>
        /// <param name="axis">Axis to check</param>
        /// <returns>a number to shift pos1 along axis by to prevent poly1 at pos1 from intersecting poly2 at pos2, or null if no int. along axis</returns>
        public static float? IntersectMTVAlongAxis(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2, Vector2 axis)
        {
            var proj1 = ProjectAlongAxis(poly1, pos1, axis);
            var proj2 = ProjectAlongAxis(poly2, pos2, axis);

            return AxisAlignedLine2.IntersectMTV(proj1, proj2);
        }
        /// <summary>
        /// Projects the polygon at position onto the specified axis.
        /// </summary>
        /// <param name="poly">The polygon</param>
        /// <param name="pos">The polygons origin</param>
        /// <param name="axis">The axis to project onto</param>
        /// <returns>poly at pos projected along axis</returns>
        public static AxisAlignedLine2 ProjectAlongAxis(Polygon2 poly, Vector2 pos, Vector2 axis)
        {
            return ProjectAlongAxis(axis, pos, poly.Vertices);
        }
    }
}

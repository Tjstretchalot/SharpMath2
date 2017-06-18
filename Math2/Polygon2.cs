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
        /// The vertices of this polygon, such that any two adjacent vertices
        /// create a line of the polygon
        /// </summary>
        public readonly Vector2[] Vertices;

        /// <summary>
        /// The lines of this polygon, such that any two adjacent (wrapping)
        /// lines share a vertex
        /// </summary>
        public readonly Line2[] Lines;

        /// <summary>
        /// The center of this polyogn
        /// </summary>
        public readonly Vector2 Center;

        /// <summary>
        /// The three normal vectors of this polygon, normalized
        /// </summary>
        public readonly List<Vector2> Normals;

        /// <summary>
        /// The bounding box.
        /// </summary>
        public readonly Rect2 AABB;

        /// <summary>
        /// The longest line that can be created inside this polygon. 
        /// <example>
        /// var poly = ShapeUtils.CreateRectangle(2, 3);
        /// 
        /// Console.WriteLine($"corner-to-corner = longest axis = Math.Sqrt(2 * 2 + 3 * 3) = {Math.Sqrt(2 * 2 + 3 * 3)} = {poly.LongestAxisLength}");
        /// </example>
        /// </summary>
        public readonly float LongestAxisLength;

        /// <summary>
        /// The area of this polygon
        /// </summary>
        public readonly float Area;

        /// <summary>
        /// If this polygon is defined clockwise
        /// </summary>
        public readonly bool Clockwise;

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
            for (int i = 1; i < vertices.Length; i++)
            {
                tmp = Math2.MakeStandardNormal(Vector2.Normalize(Math2.Perpendicular(vertices[i] - vertices[i - 1])));
                if (!Normals.Contains(tmp))
                    Normals.Add(tmp);
            }

            tmp = Math2.MakeStandardNormal(Vector2.Normalize(Math2.Perpendicular(vertices[0] - vertices[vertices.Length - 1])));
            if (!Normals.Contains(tmp))
                Normals.Add(tmp);

            var min = new Vector2(vertices[0].X, vertices[0].Y);
            var max = new Vector2(min.X, min.Y);
            for (int i = 1; i < vertices.Length; i++)
            {
                min.X = Math.Min(min.X, vertices[i].X);
                min.Y = Math.Min(min.Y, vertices[i].Y);
                max.X = Math.Max(max.X, vertices[i].X);
                max.Y = Math.Max(max.Y, vertices[i].Y);
            }
            AABB = new Rect2(min, max);

            Center = new Vector2(0, 0);
            foreach (var vert in Vertices)
            {
                Center += vert;
            }
            Center *= (1.0f / Vertices.Length);
            
            // Find longest axis
            float longestAxisLenSq = -1;
            for (int i = 1; i < vertices.Length; i++)
            {
                var vec = vertices[i] - vertices[i - 1];
                longestAxisLenSq = Math.Max(longestAxisLenSq, vec.LengthSquared());
            }
            longestAxisLenSq = Math.Max(longestAxisLenSq, (vertices[0] - vertices[vertices.Length - 1]).LengthSquared());
            LongestAxisLength = (float)Math.Sqrt(longestAxisLenSq);

            // Area and lines
            float area = 0;
            Lines = new Line2[Vertices.Length];
            var last = Vertices[Vertices.Length - 1];
            for(int i = 0; i < Vertices.Length; i++)
            {
                var next = Vertices[i];
                Lines[i] = new Line2(last, next);
                area += Math2.AreaOfTriangle(last, next, Center);
                last = next;
            }
            Area = area;

            last = Vertices[Vertices.Length - 1];
            var centToLast = (last - Center);
            var angLast = Math.Atan2(centToLast.Y, centToLast.X);
            var cwCounter = 0;
            var ccwCounter = 0;
            var foundDefinitiveResult = false;
            for (int i = 0; i < Vertices.Length; i++)
            {
                var curr = Vertices[i];
                var centToCurr = (curr - Center);
                var angCurr = Math.Atan2(centToCurr.Y, centToCurr.X);

                var clockwise = angCurr < angLast;
                if (clockwise)
                    cwCounter++;
                else
                    ccwCounter++;
                
                Clockwise = clockwise;
                if(Math.Abs(angLast - angCurr) > Math2.DEFAULT_EPSILON)
                {
                    foundDefinitiveResult = true;
                    break;
                }

                last = curr;
                centToLast = centToCurr;
                angLast = angCurr;
            }
            if (!foundDefinitiveResult)
                Clockwise = cwCounter > ccwCounter;
        }

        /// <summary>
        /// Determines if the specified polygon at the specified position and rotation contains the specified point
        /// </summary>
        /// <param name="poly">The polygon</param>
        /// <param name="pos">Origin of the polygon</param>
        /// <param name="rot">Rotation of the polygon</param>
        /// <param name="point">Point to check</param>
        /// <param name="strict">True if the edges do not count as inside</param>
        /// <returns>If the polygon at pos with rotation rot about its center contains point</returns>
        public static bool Contains(Polygon2 poly, Vector2 pos, Rotation2 rot, Vector2 point, bool strict)
        {
            if (!Rect2.Contains(poly.AABB, pos, point, strict))
                return false;

            // Calculate the area of the triangles constructed by the lines of the polygon. If it
            // matches the area of the polygon, we're inside the polygon. 
            float myArea = 0;

            var center = poly.Center + pos;
            var last = Math2.Rotate(poly.Vertices[poly.Vertices.Length - 1], poly.Center, rot) + pos;
            for(int i = 0; i < poly.Vertices.Length; i++)
            {
                var curr = Math2.Rotate(poly.Vertices[i], poly.Center, rot) + pos;

                myArea += Math2.AreaOfTriangle(center, last, curr);

                last = curr;
            }

            return Math2.Approximately(myArea, poly.Area, poly.Area / 1000);
        }

        /// <summary>
        /// Determines if the first polygon intersects the second polygon when they are at
        /// the respective positions and rotations.
        /// </summary>
        /// <param name="poly1">First polygon</param>
        /// <param name="poly2">Second polygon</param>
        /// <param name="pos1">Position of the first polygon</param>
        /// <param name="pos2">Position of the second polygon</param>
        /// <param name="rot1">Rotation of the first polygon</param>
        /// <param name="rot2">Rotation fo the second polyogn</param>
        /// <param name="strict">If overlapping is required for intersection</param>
        /// <returns>If poly1 at pos1 with rotation rot1 intersects poly2 at pos2with rotation rot2</returns>
        public static bool Intersects(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2, Rotation2 rot1, Rotation2 rot2, bool strict)
        {
            foreach (var norm in poly1.Normals.Select((v) => Tuple.Create(v, rot1)).Union(poly2.Normals.Select((v) => Tuple.Create(v, rot2))))
            {
                var axis = Math2.Rotate(norm.Item1, Vector2.Zero, norm.Item2);
                if (!IntersectsAlongAxis(poly1, poly2, pos1, pos2, rot1, rot2, strict, axis))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines the mtv to move pos1 by to prevent poly1 at pos1 from intersecting poly2 at pos2.
        /// Returns null if poly1 and poly2 do not intersect.
        /// </summary>
        /// <param name="poly1">First polygon</param>
        /// <param name="poly2">Second polygon</param>
        /// <param name="pos1">Position of the first polygon</param>
        /// <param name="pos2">Position of the second polygon</param>
        /// <param name="rot1">Rotation of the first polyogn</param>
        /// <param name="rot2">Rotation of the second polygon</param>
        /// <returns>MTV to move poly1 to prevent intersection with poly2</returns>
        public static Tuple<Vector2, float> IntersectMTV(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2, Rotation2 rot1, Rotation2 rot2)
        {
            Vector2 bestAxis = Vector2.Zero;
            float bestMagn = float.MaxValue;

            foreach (var norm in poly1.Normals.Select((v) => Tuple.Create(v, rot1)).Union(poly2.Normals.Select((v) => Tuple.Create(v, rot2))))
            {
                var axis = Math2.Rotate(norm.Item1, Vector2.Zero, norm.Item2);
                var mtv = IntersectMTVAlongAxis(poly1, poly2, pos1, pos2, rot1, rot2, axis);
                if (!mtv.HasValue)
                    return null;
                else if(Math.Abs(mtv.Value) < Math.Abs(bestMagn))
                {
                    bestAxis = axis;
                    bestMagn = mtv.Value;
                }
            }

            return Tuple.Create(bestAxis, bestMagn);
        }

        /// <summary>
        /// Determines if polygon 1 and polygon 2 at position 1 and position 2, respectively, intersect along axis.
        /// </summary>
        /// <param name="poly1">polygon 1</param>
        /// <param name="poly2">polygon 2</param>
        /// <param name="pos1">Origin of polygon 1</param>
        /// <param name="pos2">Origin of polygon 2</param>
        /// <param name="rot1">Rotation of the first polygon</param>
        /// <param name="rot2">Rotation of the second polygon</param>
        /// <param name="strict">If overlapping is required for intersection</param>
        /// <param name="axis">The axis to check</param>
        /// <returns>If poly1 at pos1 intersects poly2 at pos2 along axis</returns>
        public static bool IntersectsAlongAxis(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2, Rotation2 rot1, Rotation2 rot2, bool strict, Vector2 axis)
        {
            var proj1 = ProjectAlongAxis(poly1, pos1, rot1, axis);
            var proj2 = ProjectAlongAxis(poly2, pos2, rot2, axis);

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
        /// <param name="rot1">polygon 1 rotation</param>
        /// <param name="rot2">polygon 2 rotation</param>
        /// <param name="axis">Axis to check</param>
        /// <returns>a number to shift pos1 along axis by to prevent poly1 at pos1 from intersecting poly2 at pos2, or null if no int. along axis</returns>
        public static float? IntersectMTVAlongAxis(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2, Rotation2 rot1, Rotation2 rot2, Vector2 axis)
        {
            var proj1 = ProjectAlongAxis(poly1, pos1, rot1, axis);
            var proj2 = ProjectAlongAxis(poly2, pos2, rot2, axis);

            return AxisAlignedLine2.IntersectMTV(proj1, proj2);
        }
        /// <summary>
        /// Projects the polygon at position onto the specified axis.
        /// </summary>
        /// <param name="poly">The polygon</param>
        /// <param name="pos">The polygons origin</param>
        /// <param name="axis">The axis to project onto</param>
        /// <returns>poly at pos projected along axis</returns>
        public static AxisAlignedLine2 ProjectAlongAxis(Polygon2 poly, Vector2 pos, Rotation2 rot, Vector2 axis)
        {
            return ProjectAlongAxis(axis, pos, rot, poly.Center, poly.Vertices);
        }

        /// <summary>
        /// Calculates the shortest distance from the specified polygon to the specified point,
        /// and the axis from polygon to pos.
        /// 
        /// Returns null if pt is contained in the polygon (not strictly).
        /// </summary>
        /// <returns>The distance form poly to pt.</returns>
        /// <param name="poly">The polygon</param>
        /// <param name="pos">Origin of the polygon</param>
        /// <param name="rot">Rotation of the polygon</param>
        /// <param name="pt">Point to check.</param>
        public static Tuple<Vector2, float> MinDistance(Polygon2 poly, Vector2 pos, Rotation2 rot, Vector2 pt)
        {
            /*
             * Definitions
             * 
             * For each line in the polygon, find the normal of the line in the direction of outside the polygon.
             * Call the side of the original line that contains none of the polygon "above the line". The other side is "below the line".
             * 
             * If the point falls above the line:
             *   Imagine two additional lines that are normal to the line and fall on the start and end, respectively.
             *   For each of those two lines, call the side of the line that contains the original line "below the line". The other side is "above the line"
             *   
             *   If the point is above the line containing the start:
             *     The shortest vector is from the start to the point
             *   
             *   If the point is above the line containing the end:
             *     The shortest vector is from the end to the point
             *     
             *   Otherwise
             *     The shortest vector is from the line to the point
             * 
             * If this is not true for ANY of the lines, the polygon does not contain the point.
             */

            var last = Math2.Rotate(poly.Vertices[poly.Vertices.Length - 1], poly.Center, rot) + pos;
            for(var i = 0; i < poly.Vertices.Length; i++)
            {
                var curr = Math2.Rotate(poly.Vertices[i], poly.Center, rot) + pos;
                var axis = curr - last;
                Vector2 norm;
                if (poly.Clockwise)
                    norm = new Vector2(-axis.Y, axis.X);
                else
                    norm = new Vector2(axis.Y, -axis.X);
                norm = Vector2.Normalize(norm);
                axis = Vector2.Normalize(axis);

                var lineProjOnNorm = Vector2.Dot(norm, last);
                var ptProjOnNorm = Vector2.Dot(norm, pt);
                
                if(ptProjOnNorm > lineProjOnNorm)
                {
                    var ptProjOnAxis = Vector2.Dot(axis, pt);
                    var stProjOnAxis = Vector2.Dot(axis, last);
                    
                    if(ptProjOnAxis < stProjOnAxis)
                    {
                        var res = pt - last;
                        return Tuple.Create(Vector2.Normalize(res), res.Length());
                    }

                    var enProjOnAxis = Vector2.Dot(axis, curr);

                    if(ptProjOnAxis > enProjOnAxis)
                    {
                        var res = pt - curr;
                        return Tuple.Create(Vector2.Normalize(res), res.Length());
                    }


                    var distOnNorm = ptProjOnNorm - lineProjOnNorm;
                    return Tuple.Create(norm, distOnNorm);
                }

                last = curr;
            }

            return null;
        }

        /// <summary>
        /// Calculates the shortest distance and direction to go from poly1 at pos1 to poly2 at pos2. Returns null
        /// if the polygons intersect.
        /// </summary>
        /// <returns>The distance.</returns>
        /// <param name="poly1">First polygon</param>
        /// <param name="poly2">Second polygon</param>
        /// <param name="pos1">Origin of first polygon</param>
        /// <param name="pos2">Origin of second polygon</param>
        /// <param name="rot1">Rotation of first polygon</param>
        /// <param name="rot2">Rotation of second polygon</param>
        public static Tuple<Vector2, float> MinDistance(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2, Rotation2 rot1, Rotation2 rot2)
        {
            if (Intersects(poly1, poly2, pos1, pos2, rot1, rot2, false))
                return null;

            float? res = null;
            Vector2 axis = Vector2.Zero;

            var polyPolyNorms = new HashSet<Vector2>();
            foreach(var vert in poly1.Vertices)
            {
                var vertRot = Math2.Rotate(vert, poly1.Center, rot1) + pos1;

                foreach(var vert2 in poly2.Vertices)
                {
                    var vert2Rot = Math2.Rotate(vert2, poly2.Center, rot2) + pos2;

                    polyPolyNorms.Add(Math2.MakeStandardNormal(Vector2.Normalize(vert2Rot - vertRot)));
                }
            }

            foreach (var norm in polyPolyNorms)
            {
                var proj1 = ProjectAlongAxis(poly1, pos1, rot1, norm);
                var proj2 = ProjectAlongAxis(poly2, pos2, rot2, norm);

                var distTo = AxisAlignedLine2.MinDistance(proj1, proj2);
                if (!distTo.HasValue)
                    continue;

                if (!res.HasValue || distTo.Value > res.Value)
                {
                    res = distTo;
                    axis = norm;
                }
            }

            return Tuple.Create(axis, res.Value);
        }

        #region NoRotation
        /// <summary>
        /// Determines if the specified polygons intersect when at the specified positions and not rotated.
        /// </summary>
        /// <param name="poly1">First polygon</param>
        /// <param name="poly2">Second polygon</param>
        /// <param name="pos1">Origin of first polygon</param>
        /// <param name="pos2">Origin of second polygon</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns>If poly1 at pos1 not rotated and poly2 at pos2 not rotated intersect</returns>
        public static bool Intersects(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2, bool strict)
        {
            return Intersects(poly1, poly2, pos1, pos2, Rotation2.Zero, Rotation2.Zero, strict);
        }

        /// <summary>
        /// Determines if the first polygon at position 1 intersects the second polygon at position 2, where
        /// neither polygon is rotated.
        /// </summary>
        /// <param name="poly1">First polygon</param>
        /// <param name="poly2">Second polygon</param>
        /// <param name="pos1">Origin of first polygon</param>
        /// <param name="pos2">Origin of second polygon</param>
        /// <returns>If poly1 at pos1 not rotated intersects poly2 at pos2 not rotated</returns>
        public static Tuple<Vector2, float> IntersectMTV(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2)
        {
            return IntersectMTV(poly1, poly2, pos1, pos2, Rotation2.Zero, Rotation2.Zero);
        }

        /// <summary>
        /// Determines the shortest way for the specified polygon at the specified position with
        /// no rotation to get to the specified point, if point is not (non-strictly) intersected
        /// the polygon when it's at the specified position with no rotation.
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <param name="pos">Position of the polygon</param>
        /// <param name="pt">Point to check</param>
        /// <returns>axis to go in, distance to go if pos is not in poly, otherwise null</returns>
        public static Tuple<Vector2, float> MinDistance(Polygon2 poly, Vector2 pos, Vector2 pt)
        {
            return MinDistance(poly, pos, Rotation2.Zero, pt);
        }

        /// <summary>
        /// Determines the shortest way for the first polygon at position 1 to touch the second polygon at
        /// position 2, assuming the polygons do not intersect (not strictly) and are not rotated.
        /// </summary>
        /// <param name="poly1">First polygon</param>
        /// <param name="poly2">Second polygon</param>
        /// <param name="pos1">Position of first polygon</param>
        /// <param name="pos2">Position of second polygon</param>
        /// <returns>axis to go in, distance to go if poly1 does not intersect poly2, otherwise null</returns>
        public static Tuple<Vector2, float> MinDistance(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2)
        {
            return MinDistance(poly1, poly2, pos1, pos2, Rotation2.Zero, Rotation2.Zero);
        }
        #endregion
    }
}

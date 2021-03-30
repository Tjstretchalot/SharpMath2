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
        /// This convex polygon partitioned into triangles, sorted by the area
        /// of the triangles in descending order
        /// </summary>
        public readonly Triangle2[] TrianglePartition;

        /// <summary>
        /// The three normal vectors of this polygon, normalized
        /// </summary>
        public readonly List<Vector2> Normals;

        /// <summary>
        /// The bounding box.
        /// </summary>
        public readonly Rect2 AABB;

        private float _LongestAxisLength;

        /// <summary>
        /// The longest line that can be created inside this polygon.
        /// <example>
        /// var poly = ShapeUtils.CreateRectangle(2, 3);
        ///
        /// Console.WriteLine($"corner-to-corner = longest axis = Math.Sqrt(2 * 2 + 3 * 3) = {Math.Sqrt(2 * 2 + 3 * 3)} = {poly.LongestAxisLength}");
        /// </example>
        /// </summary>
        public float LongestAxisLength
        {
            get
            {
                if(_LongestAxisLength < 0)
                {
                    Vector2[] verts = Vertices;
                    float longestAxisLenSq = -1;
                    for (int i = 1, len = verts.Length; i < len; i++)
                    {
                        for (int j = 0; j < i; j++)
                        {
                            var vec = verts[i] - verts[j];
                            var vecLenSq = vec.LengthSquared();
                            if (vecLenSq > longestAxisLenSq)
                                longestAxisLenSq = vecLenSq;
                        }
                    }
                    _LongestAxisLength = (float)Math.Sqrt(longestAxisLenSq);
                }

                return _LongestAxisLength;
            }
        }

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

            _LongestAxisLength = -1;

            // Center, area, and lines
            TrianglePartition = new Triangle2[Vertices.Length - 2];
            float[] triangleSortKeys = new float[TrianglePartition.Length];
            float area = 0;
            Lines = new Line2[Vertices.Length];
            Lines[0] = new Line2(Vertices[Vertices.Length - 1], Vertices[0]);
            var last = Vertices[0];
            Center = new Vector2(0, 0);
            for (int i = 1; i < Vertices.Length - 1; i++)
            {
                var next = Vertices[i];
                var next2 = Vertices[i + 1];
                Lines[i] = new Line2(last, next);
                var tri = new Triangle2(new Vector2[] { Vertices[0], next, next2 });
                TrianglePartition[i - 1] = tri;
                triangleSortKeys[i - 1] = -tri.Area;
                area += tri.Area;
                Center += tri.Center * tri.Area;
                last = next;
            }
            Lines[Vertices.Length - 1] = new Line2(Vertices[Vertices.Length - 2], Vertices[Vertices.Length - 1]);

            Array.Sort(triangleSortKeys, TrianglePartition);

            Area = area;
            Center /= area;

            last = Vertices[Vertices.Length - 1];
            var centToLast = (last - Center);
            var angLast = Rotation2.Standardize((float)Math.Atan2(centToLast.Y, centToLast.X));
            var cwCounter = 0;
            var ccwCounter = 0;
            var foundDefinitiveResult = false;
            for (int i = 0; i < Vertices.Length; i++)
            {
                var curr = Vertices[i];
                var centToCurr = (curr - Center);
                var angCurr = Rotation2.Standardize((float)Math.Atan2(centToCurr.Y, centToCurr.X));


                var clockwise = (angCurr < angLast && (angCurr - angLast) < Math.PI) || (angCurr - angLast) > Math.PI;
                if (clockwise)
                    cwCounter++;
                else
                    ccwCounter++;

                Clockwise = clockwise;
                if (Math.Abs(angLast - angCurr) > Math2.DEFAULT_EPSILON)
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
        /// Determines the actual location of the vertices of the given polygon
        /// when at the given offset and rotation.
        /// </summary>
        /// <param name="polygon">The polygon</param>
        /// <param name="offset">The polygons offset</param>
        /// <param name="rotation">The polygons rotation</param>
        /// <returns>The actualized polygon</returns>
        public static Vector2[] ActualizePolygon(Polygon2 polygon, Vector2 offset, Rotation2 rotation)
        {
            int len = polygon.Vertices.Length;
            Vector2[] result = new Vector2[len];

            if (rotation != Rotation2.Zero)
            {
                for (int i = 0; i < len; i++)
                {
                    result[i] = Math2.Rotate(polygon.Vertices[i], polygon.Center, rotation) + offset;
                }
            } else
            {
                // performance sensitive section
                int i = 0;
                for (; i + 3 < len; i += 4)
                {
                    result[i] = new Vector2(
                        polygon.Vertices[i].X + offset.X,
                        polygon.Vertices[i].Y + offset.Y
                    );
                    result[i + 1] = new Vector2(
                        polygon.Vertices[i + 1].X + offset.X,
                        polygon.Vertices[i + 1].Y + offset.Y
                    );
                    result[i + 2] = new Vector2(
                        polygon.Vertices[i + 2].X + offset.X,
                        polygon.Vertices[i + 2].Y + offset.Y
                    );
                    result[i + 3] = new Vector2(
                        polygon.Vertices[i + 3].X + offset.X,
                        polygon.Vertices[i + 3].Y + offset.Y
                    );
                }

                for (; i < len; i++)
                {
                    result[i] = new Vector2(
                        polygon.Vertices[i].X + offset.X,
                        polygon.Vertices[i].Y + offset.Y
                    );
                }
            }

            return result;
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
            // The point is contained in the polygon iff it is contained in one of the triangles
            // which partition this polygon. Due to how we constructed the triangles, it will
            // be on the edge of the polygon if its on the first 2 edges of the triangle.

            for (int i = 0, len = poly.TrianglePartition.Length; i < len; i++)
            {
                var tri = poly.TrianglePartition[i];

                if (Triangle2.Contains(tri, pos, point))
                {
                    if (strict && (Line2.Contains(tri.Edges[0], pos, point) || Line2.Contains(tri.Edges[1], pos, point)))
                        return false;
                    return true;
                }
            }
            return false;
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
            return IntersectsSAT(poly1, poly2, pos1, pos2, rot1, rot2, strict);
        }

        /// <summary>
        /// Determines if the two polygons intersect using the Separating Axis Theorem.
        /// The performance of this function depends on the number of unique normals
        /// between the two polygons.
        /// </summary>
        /// <param name="poly1">First polygon</param>
        /// <param name="poly2">Second polygon</param>
        /// <param name="pos1">Offset for the vertices of the first polygon</param>
        /// <param name="pos2">Offset for the vertices of the second polygon</param>
        /// <param name="rot1">Rotation of the first polygon</param>
        /// <param name="rot2">Rotation of the second polygon</param>
        /// <param name="strict">
        /// True if the two polygons must overlap a non-zero area for intersection,
        /// false if they must overlap on at least one point for intersection.
        /// </param>
        /// <returns>True if the polygons overlap, false if they do not</returns>
        public static bool IntersectsSAT(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2, Rotation2 rot1, Rotation2 rot2, bool strict)
        {
            if (rot1 == Rotation2.Zero && rot2 == Rotation2.Zero)
            {
                // This was a serious performance bottleneck so we speed up the fast case
                HashSet<Vector2> seen = new HashSet<Vector2>();
                Vector2[] poly1Verts = poly1.Vertices;
                Vector2[] poly2Verts = poly2.Vertices;
                for (int i = 0, len = poly1.Normals.Count; i < len; i++)
                {
                    var axis = poly1.Normals[i];
                    var proj1 = ProjectAlongAxis(axis, pos1, poly1Verts);
                    var proj2 = ProjectAlongAxis(axis, pos2, poly2Verts);
                    if (!AxisAlignedLine2.Intersects(proj1, proj2, strict))
                        return false;
                    seen.Add(axis);
                }
                for (int i = 0, len = poly2.Normals.Count; i < len; i++)
                {
                    var axis = poly2.Normals[i];
                    if (seen.Contains(axis))
                        continue;

                    var proj1 = ProjectAlongAxis(axis, pos1, poly1Verts);
                    var proj2 = ProjectAlongAxis(axis, pos2, poly2Verts);
                    if (!AxisAlignedLine2.Intersects(proj1, proj2, strict))
                        return false;
                }
                return true;
            }

            foreach (var norm in poly1.Normals.Select((v) => Tuple.Create(v, rot1)).Union(poly2.Normals.Select((v) => Tuple.Create(v, rot2))))
            {
                var axis = Math2.Rotate(norm.Item1, Vector2.Zero, norm.Item2);
                if (!IntersectsAlongAxis(poly1, poly2, pos1, pos2, rot1, rot2, strict, axis))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the two polygons intersect, inspired by the GJK algorithm. The
        /// performance of this algorithm generally depends on how separated the
        /// two polygons are.
        ///
        /// This essentially acts as a directed search of the triangles in the
        /// minkowski difference to check if any of them contain the origin.
        ///
        /// The minkowski difference polygon has up to M*N possible vertices, where M is the
        /// number of vertices in the first polygon and N is the number of vertices
        /// in the second polygon.
        /// </summary>
        /// <param name="poly1">First polygon</param>
        /// <param name="poly2">Second polygon</param>
        /// <param name="pos1">Offset for the vertices of the first polygon</param>
        /// <param name="pos2">Offset for the vertices of the second polygon</param>
        /// <param name="rot1">Rotation of the first polygon</param>
        /// <param name="rot2">Rotation of the second polygon</param>
        /// <param name="strict">
        /// True if the two polygons must overlap a non-zero area for intersection,
        /// false if they must overlap on at least one point for intersection.
        /// </param>
        /// <returns>True if the polygons overlap, false if they do not</returns>
        public static unsafe bool IntersectsGJK(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2, Rotation2 rot1, Rotation2 rot2, bool strict)
        {
            Vector2[] verts1 = ActualizePolygon(poly1, pos1, rot1);
            Vector2[] verts2 = ActualizePolygon(poly2, pos2, rot2);

            Vector2 desiredAxis = new Vector2(
                poly1.Center.X + pos1.X - poly2.Center.X - pos2.X,
                poly2.Center.Y + pos1.Y - poly2.Center.Y - pos2.Y
            );

            if (Math2.Approximately(desiredAxis, Vector2.Zero))
                desiredAxis = Vector2.UnitX;
            else
                desiredAxis.Normalize(); // cleanup rounding issues

            var simplex = stackalloc Vector2[3];
            int simplexIndex = -1;
            bool simplexProper = true;

            while (true)
            {
                if (simplexIndex < 2) {
                    simplex[++simplexIndex] = CalculateSupport(verts1, verts2, desiredAxis);

                    float progressFromOriginTowardDesiredAxis = Math2.Dot(simplex[simplexIndex], desiredAxis);
                    if (progressFromOriginTowardDesiredAxis < -Math2.DEFAULT_EPSILON)
                    {
                        return false; // no hope
                    }

                    if (progressFromOriginTowardDesiredAxis < Math2.DEFAULT_EPSILON)
                    {
                        if (Math2.Approximately(simplex[simplexIndex], Vector2.Zero))
                        {
                            // We've determined that the origin is a point on the
                            // edge of the minkowski difference. In fact, it's even
                            // a vertex. This means that the two polygons are just
                            // touching.
                            return !strict;
                        }
                        // When we go to check the simplex, we can't assume that
                        // we know the origin will be in either AC or AB, as that
                        // assumption relies on this progress being strictly positive.
                        simplexProper = false;
                    }

                    if (simplexIndex == 0)
                    {
                        desiredAxis = -simplex[0];
                        desiredAxis.Normalize(); // resolve rounding issues
                        continue;
                    }

                    if (simplexIndex == 1)
                    {
                        // We only have 2 points; we need to select the third.
                        desiredAxis = Math2.TripleCross(simplex[1] - simplex[0], -simplex[1]);

                        if (Math2.Approximately(desiredAxis, Vector2.Zero))
                        {
                            // This means that the origin lies along the infinite
                            // line which goes through simplex[0] and simplex[1].
                            // We will choose a point perpendicular for now, but we
                            // will have to do extra work later to handle the fact that
                            // the origin won't be in regions AB or AC.
                            simplexProper = false;
                            desiredAxis = Math2.Perpendicular(simplex[1] - simplex[0]);
                        }

                        desiredAxis.Normalize(); // resolve rounding issues
                        continue;
                    }
                }

                Vector2 ac = simplex[0] - simplex[2];
                Vector2 ab = simplex[1] - simplex[2];
                Vector2 ao = -simplex[2];

                Vector2 acPerp = Math2.TripleCross(ac, ab);
                acPerp.Normalize(); // resolve rounding issues
                float amountTowardsOriginAC = Math2.Dot(acPerp, ao);

                if (amountTowardsOriginAC < -Math2.DEFAULT_EPSILON)
                {
                    // We detected that the origin is in the AC region
                    desiredAxis = -acPerp;
                    simplexProper = true;
                }
                else
                {
                    if (amountTowardsOriginAC < Math2.DEFAULT_EPSILON)
                    {
                        simplexProper = false;
                    }

                    // Could still be within the triangle.
                    Vector2 abPerp = Math2.TripleCross(ab, ac);
                    abPerp.Normalize(); // resolve rounding issues

                    float amountTowardsOriginAB = Math2.Dot(abPerp, ao);
                    if (amountTowardsOriginAB < -Math2.DEFAULT_EPSILON)
                    {
                        // We detected that the origin is in the AB region
                        simplex[0] = simplex[1];
                        desiredAxis = -abPerp;
                        simplexProper = true;
                    }
                    else
                    {
                        if (amountTowardsOriginAB < Math2.DEFAULT_EPSILON)
                        {
                            simplexProper = false;
                        }

                        if (simplexProper)
                        {
                            return true;
                        }

                        // We've eliminated the standard cases for the simplex, i.e.,
                        // regions AB and AC. If the previous steps succeeded, this
                        // means we've definitively shown that the origin is within
                        // the triangle. However, if the simplex is improper, then
                        // we need to check the edges before we can be confident.

                        // We'll check edges first.
                        bool isOnABEdge = false;

                        if (Math2.IsBetweenLine(simplex[0], simplex[2], Vector2.Zero))
                        {
                            // we've determined the origin is on the edge AC.
                            // we'll swap B and C so that we're now on the edge
                            // AB, and handle like that case. abPerp and acPerp also swap,
                            // but we don't care about acPerp anymore
                            Vector2 tmp = simplex[0];
                            simplex[0] = simplex[1];
                            simplex[1] = tmp;
                            abPerp = acPerp;
                            isOnABEdge = true;
                        }
                        else if (Math2.IsBetweenLine(simplex[0], simplex[1], Vector2.Zero))
                        {
                            // we've determined the origin is on edge BC.
                            // we'll swap A and C so that we're now on the
                            // edge AB, and handle like that case. we'll need to
                            // recalculate abPerp
                            Vector2 tmp = simplex[2];
                            simplex[2] = simplex[0];
                            simplex[0] = tmp;
                            ab = simplex[1] - simplex[2];
                            ac = simplex[0] - simplex[2];
                            abPerp = Math2.TripleCross(ab, ac);
                            abPerp.Normalize();
                            isOnABEdge = true;
                        }

                        if (isOnABEdge || Math2.IsBetweenLine(simplex[1], simplex[2], Vector2.Zero))
                        {
                            // The origin is along the line AB. This means we'll either
                            // have another choice for A that wouldn't have done this,
                            // or the line AB is actually on the edge of the minkowski
                            // difference, and hence we are just touching.

                            // There is a case where this trick isn't going to work, in
                            // particular, if when you triangularize the polygon, the
                            // origin falls on an inner edge.

                            // In our case, at this point, we are going to have 4 points,
                            // which form a quadrilateral which contains the origin, but
                            // for which there is no way to draw a triangle out of the
                            // vertices that does not have the origin on the edge.

                            // I think though that the only way this happens would imply
                            // the origin is on simplex[1] <-> ogSimplex2 (we know this
                            // as that is what this if statement is for) and on
                            // simplex[0], (new) simplex[2], and I think it guarrantees
                            // we're in that case.


                            desiredAxis = -abPerp;
                            Vector2 ogSimplex2 = simplex[2];

                            simplex[2] = CalculateSupport(verts1, verts2, desiredAxis);

                            if (
                                Math2.Approximately(simplex[1], simplex[2]) ||
                                Math2.Approximately(ogSimplex2, simplex[2]) ||
                                Math2.Approximately(simplex[2], Vector2.Zero)
                            )
                            {
                                // we've shown that this is a true edge
                                return !strict;
                            }

                            if (Math2.Dot(simplex[2], desiredAxis) <= 0)
                            {
                                // we didn't find a useful point!
                                return !strict;
                            }

                            if (Math2.IsBetweenLine(simplex[0], simplex[2], Vector2.Zero))
                            {
                                // We've proven that we're contained in a quadrilateral
                                // Example of how we get here: C B A ogSimplex2
                                // (-1, -1), (-1, 0), (5, 5), (5, 0)
                                return true;
                            }

                            if (Math2.IsBetweenLine(simplex[1], simplex[2], Vector2.Zero))
                            {
                                // We've shown that we on the edge
                                // Example of how we get here: C B A ogSimplex2
                                // (-32.66077,4.318787), (1.25, 0), (-25.41077, -0.006134033), (-32.66077, -0.006134033
                                return !strict;
                            }

                            simplexProper = true;
                            continue;
                        }

                        // we can trust our results now as we know the point is
                        // not on an edge. we'll need to be confident in our
                        // progress check as well, so we'll skip the top of the
                        // loop

                        if (amountTowardsOriginAB < 0)
                        {
                            // in the AB region
                            simplex[0] = simplex[1];
                            desiredAxis = -abPerp;
                        }
                        else if (amountTowardsOriginAC < 0)
                        {
                            // in the AC region
                            desiredAxis = -acPerp;
                        }
                        else
                        {
                            // now we're sure the point is in the triangle
                            return true;
                        }

                        simplex[1] = simplex[2];
                        simplex[2] = CalculateSupport(verts1, verts2, desiredAxis);
                        if (Math2.Dot(simplex[simplexIndex], desiredAxis) < 0)
                        {
                            return false;
                        }

                        simplexProper = true;
                        continue;
                    }
                }

                simplex[1] = simplex[2];
                simplexIndex--;
            }
        }


        /// <summary>
        /// Calculates the support vector along +axis+ for the two polygons. This
        /// is the point furthest in the direction of +axis+ within the minkowski
        /// difference of poly1 (-) poly2.
        /// </summary>
        /// <param name="verts1">First polygon vertices</param>
        /// <param name="verts2">Second polygon vertices</param>
        /// <param name="axis">The axis for the support</param>
        /// <returns>Support along axis for the minkowski difference</returns>
        public static Vector2 CalculateSupport(Vector2[] verts1, Vector2[] verts2, Vector2 axis)
        {
            // We calculate the two supports individually, and the difference will
            // still satisfy the necessary property.
            int index1 = IndexOfFurthestPoint(verts1, axis);
            int index2 = IndexOfFurthestPoint(verts2, -axis);

            return verts1[index1] - verts2[index2];
        }

        /// <summary>
        /// Calculates the index of the vector within verts which is most along the
        /// given axis.
        /// </summary>
        /// <param name="verts">The array of vertices</param>
        /// <param name="axis">The axis</param>
        /// <returns>The index within verts furthest along axis</returns>
        public static int IndexOfFurthestPoint(Vector2[] verts, Vector2 axis)
        {
            // performance sensitive section
            // force inlining of dots
            float max = verts[0].X * axis.X + verts[0].Y * axis.Y;
            int index = 0;
            for (int i = 1, len = verts.Length; i < len; i++)
            {
                float dot = verts[i].X * axis.X + verts[i].Y * axis.Y;
                if (dot > max)
                {
                    max = dot;
                    index = i;
                }
            }
            return index;
        }

        public static void DumpInfo(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2, bool strict)
        {
            Console.WriteLine("Polygon2 poly1 = new Polygon2(new Vector2[]");
            Console.WriteLine("{");
            foreach (Vector2 v in poly1.Vertices) {
                Console.WriteLine($"  new Vector2({v.X}f, {v.Y}f),");
            }
            Console.WriteLine("});");
            Console.WriteLine("Polygon2 poly2 = new Polygon2(new Vector2[]");
            Console.WriteLine("{");
            foreach (Vector2 v in poly2.Vertices)
            {
                Console.WriteLine($"  new Vector2({v.X}f, {v.Y}f),");
            }
            Console.WriteLine("});");

            Console.WriteLine($"Vector2 pos1 = new Vector2({pos1.X}f, {pos1.Y}f);");
            Console.WriteLine($"Vector2 pos2 = new Vector2({pos2.X}f, {pos2.Y}f);");
            Console.WriteLine($"bool strict = {strict.ToString().ToLower()};");
        }

        public static void DesmosReady(Vector2[] verts)
        {
            foreach (Vector2 v in verts)
            {
                Console.Write($"({v.X},{v.Y}),");
            }
            Console.WriteLine();
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
                else if (Math.Abs(mtv.Value) < Math.Abs(bestMagn))
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
        /// <param name="rot">the rotation of the polygon</param>
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
            for (var i = 0; i < poly.Vertices.Length; i++)
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

                if (ptProjOnNorm > lineProjOnNorm)
                {
                    var ptProjOnAxis = Vector2.Dot(axis, pt);
                    var stProjOnAxis = Vector2.Dot(axis, last);

                    if (ptProjOnAxis < stProjOnAxis)
                    {
                        var res = pt - last;
                        return Tuple.Create(Vector2.Normalize(res), res.Length());
                    }

                    var enProjOnAxis = Vector2.Dot(axis, curr);

                    if (ptProjOnAxis > enProjOnAxis)
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

        private static IEnumerable<Vector2> GetExtraMinDistanceVecsPolyPoly(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2)
        {
            foreach (var vert in poly1.Vertices)
            {
                foreach (var vert2 in poly2.Vertices)
                {
                    var roughAxis = ((vert2 + pos2) - (vert + pos1));
                    roughAxis.Normalize();
                    yield return Math2.MakeStandardNormal(roughAxis);
                }
            }
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
            if (rot1.Theta != 0 || rot2.Theta != 0)
            {
                throw new NotSupportedException("Finding the minimum distance between polygons requires calculating the rotated polygons. This operation is expensive and should be cached. " +
                                                "Create the rotated polygons with Polygon2#GetRotated and call this function with Rotation2.Zero for both rotations.");
            }

            var axises = poly1.Normals.Union(poly2.Normals).Union(GetExtraMinDistanceVecsPolyPoly(poly1, poly2, pos1, pos2));
            Vector2? bestAxis = null; // note this is the one with the longest distance
            float bestDist = 0;
            foreach (var norm in axises)
            {
                var proj1 = ProjectAlongAxis(poly1, pos1, rot1, norm);
                var proj2 = ProjectAlongAxis(poly2, pos2, rot2, norm);

                var dist = AxisAlignedLine2.MinDistance(proj1, proj2);
                if (dist.HasValue && (bestAxis == null || dist.Value > bestDist))
                {
                    bestDist = dist.Value;
                    if (proj2.Min < proj1.Min && dist > 0)
                        bestAxis = -norm;
                    else
                        bestAxis = norm;
                }
            }

            if (!bestAxis.HasValue || Math2.Approximately(bestDist, 0))
                return null; // they intersect

            return Tuple.Create(bestAxis.Value, bestDist);
        }

        /// <summary>
        /// Returns a polygon that is created by rotated the original polygon
        /// about its center by the specified amount. Returns the original polygon if
        /// rot.Theta == 0.
        /// </summary>
        /// <returns>The rotated polygon.</returns>
        /// <param name="original">Original.</param>
        /// <param name="rot">Rot.</param>
        public static Polygon2 GetRotated(Polygon2 original, Rotation2 rot)
        {
            if (rot.Theta == 0)
                return original;

            var rotatedVerts = new Vector2[original.Vertices.Length];
            for (var i = 0; i < original.Vertices.Length; i++)
            {
                rotatedVerts[i] = Math2.Rotate(original.Vertices[i], original.Center, rot);
            }

            return new Polygon2(rotatedVerts);
        }


        /// <summary>
        /// Creates the ray trace polygons from the given polygon moving from start to end. The returned set of polygons
        /// may not be the smallest possible set of polygons which perform this job.
        ///
        /// In order to determine if polygon A intersects polygon B during a move from position S to E, you can check if
        /// B intersects any of the polygons in CreateRaytraceAblesFromPolygon(A, E - S) when they are placed at S.
        /// </summary>
        /// <example>
        /// <code>
        /// Polygon2 a = ShapeUtils.CreateCircle(10, 0, 0, 5);
        /// Polygon2 b = ShapeUtils.CreateCircle(15, 0, 0, 7);
        ///
        /// Vector2 from = new Vector2(3, 3);
        /// Vector2 to = new Vector2(15, 3);
        /// Vector2 bloc = new Vector2(6, 3);
        ///
        /// List&lt;Polygon2&gt; traces = Polygon2.CreateRaytraceAbles(a, to - from);
        /// foreach (var trace in traces)
        /// {
        ///     if (Polygon2.Intersects(trace, b, from, bloc, true))
        ///     {
        ///         Console.WriteLine("Intersects!");
        ///         break;
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <param name="poly">The polygon that you want to move</param>
        /// <param name="offset">The direction and magnitude that the polygon moves</param>
        /// <returns>A set of polygons which completely contain the area that the polygon will intersect during a move
        /// from the origin to offset.</returns>
        public static List<Polygon2> CreateRaytraceAbles(Polygon2 poly, Vector2 offset)
        {
            var ourLinesAsRects = new List<Polygon2>();
            if (Math2.Approximately(offset, Vector2.Zero))
            {
                ourLinesAsRects.Add(poly);
                return ourLinesAsRects;
            }

            for (int lineIndex = 0, nLines = poly.Lines.Length; lineIndex < nLines; lineIndex++)
            {
                var line = poly.Lines[lineIndex];
                if (!Math2.IsOnLine(line.Start, line.End, line.Start + offset))
                {
                    ourLinesAsRects.Add(new Polygon2(new Vector2[]
                    {
                    line.Start,
                    line.End,
                    line.End + offset,
                    line.Start + offset
                    }));
                }
            }

            return ourLinesAsRects;
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

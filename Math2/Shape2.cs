using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SharpMath2
{
    /// <summary>
    /// Parent class for shapes - contains functions for comparing different shapes.
    /// </summary>
    public class Shape2
    {
        /// <summary>
        /// Determines if polygon at position 1 intersects the rectangle at position 2. Polygon may
        /// be rotated, but the rectangle cannot (use a polygon if you want to rotate it).
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <param name="rect">Rectangle</param>
        /// <param name="pos1">Origin of polygon</param>
        /// <param name="pos2">Origin of rectangle</param>
        /// <param name="rot1">Rotation of the polygon.</param>
        /// <param name="strict">If overlapping is required for intersection</param>
        /// <returns>if poly at pos1 intersects rect at pos2</returns>
        public static bool Intersects(Polygon2 poly, Rect2 rect, Vector2 pos1, Vector2 pos2, Rotation2 rot1, bool strict)
        {
            bool checkedX = false, checkedY = false;
            for (int i = 0; i < poly.Normals.Count; i++)
            {
                var norm = Math2.Rotate(poly.Normals[i], Vector2.Zero, rot1);
                if (!IntersectsAlongAxis(poly, rect, pos1, pos2, rot1, strict, norm))
                    return false;

                if (norm.X == 0)
                    checkedY = true;
                if (norm.Y == 0)
                    checkedX = true;
            }

            if (!checkedX && !IntersectsAlongAxis(poly, rect, pos1, pos2, rot1, strict, Vector2.UnitX))
                return false;
            if (!checkedY && !IntersectsAlongAxis(poly, rect, pos1, pos2, rot1, strict, Vector2.UnitY))
                return false;

            return true;
        }

        /// <summary>
        /// Determines the vector, if any, to move poly at pos1 rotated rot1 to prevent intersection of rect
        /// at pos2.
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <param name="rect">Rectangle</param>
        /// <param name="pos1">Origin of polygon</param>
        /// <param name="pos2">Origin of rectangle</param>
        /// <param name="rot1">Rotation of the polygon.</param>
        /// <returns>The vector to move pos1 by or null</returns>
        public static Tuple<Vector2, float> IntersectMTV(Polygon2 poly, Rect2 rect, Vector2 pos1, Vector2 pos2, Rotation2 rot1)
        {
            bool checkedX = false, checkedY = false;

            Vector2 bestAxis = Vector2.Zero;
            float bestMagn = float.MaxValue;

            for (int i = 0; i < poly.Normals.Count; i++)
            {
                var norm = Math2.Rotate(poly.Normals[i], Vector2.Zero, rot1);
                var mtv = IntersectMTVAlongAxis(poly, rect, pos1, pos2, rot1, norm);
                if (!mtv.HasValue)
                    return null;

                if (Math.Abs(mtv.Value) < Math.Abs(bestMagn))
                {
                    bestAxis = norm;
                    bestMagn = mtv.Value;
                }

                if (norm.X == 0)
                    checkedY = true;
                if (norm.Y == 0)
                    checkedX = true;
            }

            if (!checkedX)
            {
                var mtv = IntersectMTVAlongAxis(poly, rect, pos1, pos2, rot1, Vector2.UnitX);
                if (!mtv.HasValue)
                    return null;

                if (Math.Abs(mtv.Value) < Math.Abs(bestMagn))
                {
                    bestAxis = Vector2.UnitX;
                    bestMagn = mtv.Value;
                }
            }

            if (!checkedY)
            {
                var mtv = IntersectMTVAlongAxis(poly, rect, pos1, pos2, rot1, Vector2.UnitY);
                if (!mtv.HasValue)
                    return null;

                if (Math.Abs(mtv.Value) < Math.Abs(bestMagn))
                {
                    bestAxis = Vector2.UnitY;
                    bestMagn = mtv.Value;
                }
            }

            return Tuple.Create(bestAxis, bestMagn);
        }

        /// <summary>
        /// Determines the vector to move pos1 to get rect not to intersect poly at pos2 rotated
        /// by rot2 radians.
        /// </summary>
        /// <param name="rect">The rectangle</param>
        /// <param name="poly">The polygon</param>
        /// <param name="pos1">Origin of rectangle</param>
        /// <param name="pos2">Origin of </param>
        /// <param name="rot2">Rotation of the polygon</param>
        /// <returns>Offset of pos1 to get rect not to intersect poly</returns>
        public static Tuple<Vector2, float> IntersectMTV(Rect2 rect, Polygon2 poly, Vector2 pos1, Vector2 pos2, Rotation2 rot2)
        {
            var res = IntersectMTV(poly, rect, pos2, pos1, rot2);
            return res != null ? Tuple.Create(-res.Item1, res.Item2) : res;
        }

        /// <summary>
        /// Determines if the rectangle at pos1 intersects the polygon at pos2.
        /// </summary>
        /// <param name="rect">The rectangle</param>
        /// <param name="poly">The polygon</param>
        /// <param name="pos1">Origin of retangle</param>
        /// <param name="pos2">Origin of polygon</param>
        /// <param name="rot2">Rotation of the polygon.</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns>If rect at pos1 intersects poly at pos2</returns>
        public static bool Intersects(Rect2 rect, Polygon2 poly, Vector2 pos1, Vector2 pos2, Rotation2 rot2, bool strict)
        {
            return Intersects(poly, rect, pos2, pos1, rot2, strict);
        }


        /// <summary>
        /// Determines if the specified polygon and rectangle where poly is at pos1 and rect is at pos2 intersect
        /// along the specified axis.
        /// </summary>
        /// <param name="poly">polygon</param>
        /// <param name="rect">Rectangle</param>
        /// <param name="pos1">Origin of polygon</param>
        /// <param name="pos2">Origin of rectangle</param>
        /// <param name="rot1">Rotation of the polygon.</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <param name="axis">Axis to check</param>
        /// <returns>If poly at pos1 intersects rect at pos2 along axis</returns>
        public static bool IntersectsAlongAxis(Polygon2 poly, Rect2 rect, Vector2 pos1, Vector2 pos2, Rotation2 rot1, bool strict, Vector2 axis)
        {
            var proj1 = Polygon2.ProjectAlongAxis(poly, pos1, rot1, axis);
            var proj2 = Rect2.ProjectAlongAxis(rect, pos2, axis);

            return AxisAlignedLine2.Intersects(proj1, proj2, strict);
        }

        /// <summary>
        /// Determines if the specified rectangle and polygon where rect is at pos1 and poly is at pos2 intersect
        /// along the specified axis.
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <param name="poly">Polygon</param>
        /// <param name="pos1">Origin of rectangle</param>
        /// <param name="pos2">Origin of polygon</param>
        /// <param name="rot2">Rotation of polygon</param>
        /// <param name="strict"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static bool IntersectsAlongAxis(Rect2 rect, Polygon2 poly, Vector2 pos1, Vector2 pos2, Rotation2 rot2, bool strict, Vector2 axis)
        {
            return IntersectsAlongAxis(poly, rect, pos2, pos1, rot2, strict, axis);
        }

        /// <summary>
        /// Determines the mtv along axis to move poly at pos1 to prevent intersection with rect at pos2.
        /// </summary>
        /// <param name="poly">polygon</param>
        /// <param name="rect">Rectangle</param>
        /// <param name="pos1">Origin of polygon</param>
        /// <param name="pos2">Origin of rectangle</param>
        /// <param name="rot1">Rotation of polygon in radians</param>
        /// <param name="axis">Axis to check</param>
        /// <returns>Number if poly intersects rect along axis, null otherwise</returns>
        public static float? IntersectMTVAlongAxis(Polygon2 poly, Rect2 rect, Vector2 pos1, Vector2 pos2, Rotation2 rot1, Vector2 axis)
        {
            var proj1 = Polygon2.ProjectAlongAxis(poly, pos1, rot1, axis);
            var proj2 = Rect2.ProjectAlongAxis(rect, pos2, axis);

            return AxisAlignedLine2.IntersectMTV(proj1, proj2);
        }

        /// <summary>
        /// Determines the mtv along axis to move rect at pos1 to prevent intersection with poly at pos2
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <param name="poly">polygon</param>
        /// <param name="pos1">Origin of rectangle</param>
        /// <param name="pos2">Origin of polygon</param>
        /// <param name="rot2">Rotation of the polygon in radians</param>
        /// <param name="axis">Axis to check</param>
        /// <returns>Number if rect intersects poly along axis, null otherwise</returns>
        public static float? IntersectMTVAlongAxis(Rect2 rect, Polygon2 poly, Vector2 pos1, Vector2 pos2, Rotation2 rot2, Vector2 axis)
        {
            var proj1 = Rect2.ProjectAlongAxis(rect, pos1, axis);
            var proj2 = Polygon2.ProjectAlongAxis(poly, pos2, rot2, axis);

            return AxisAlignedLine2.IntersectMTV(proj1, proj2);
        }

        /// <summary>
        /// Determines if the specified polygon at the specified position and rotation
        /// intersects the specified circle at it's respective position.
        /// </summary>
        /// <param name="poly">The polygon</param>
        /// <param name="circle">The circle</param>
        /// <param name="pos1">The origin for the polygon</param>
        /// <param name="pos2">The top-left of the circles bounding box</param>
        /// <param name="rot1">The rotation of the polygon</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns>If poly at pos1 with rotation rot1 intersects the circle at pos2</returns>
        public static bool Intersects(Polygon2 poly, Circle2 circle, Vector2 pos1, Vector2 pos2, Rotation2 rot1, bool strict)
        {
            // look at pictures of https://stackoverflow.com/questions/401847/circle-rectangle-collision-detection-intersection if you don't
            // believe this is true
            return poly.Lines.Any((l) => CircleIntersectsLine(circle, l, pos2, pos1, rot1, poly.Center, strict)) || Polygon2.Contains(poly, pos1, rot1, new Vector2(pos2.X + circle.Radius, pos2.Y + circle.Radius), strict);
        }

        /// <summary>
        /// Determines the minimum translation that must be applied the specified polygon (at the given position
        /// and rotation) to prevent intersection with the circle (at its given rotation). If the two are not overlapping,
        /// returns null.
        ///
        /// Returns a tuple of the axis to move the polygon in (unit vector) and the distance to move the polygon.
        /// </summary>
        /// <param name="poly">The polygon</param>
        /// <param name="circle">The circle</param>
        /// <param name="pos1">The origin of the polygon</param>
        /// <param name="pos2">The top-left of the circles bounding box</param>
        /// <param name="rot1">The rotation of the polygon</param>
        /// <returns></returns>
        public static Tuple<Vector2, float> IntersectMTV(Polygon2 poly, Circle2 circle, Vector2 pos1, Vector2 pos2, Rotation2 rot1)
        {
            // We have two situations, either the circle is not strictly intersecting the polygon, or
            // there exists at least one shortest line that you could push the polygon to prevent
            // intersection with the circle.

            // That line will either go from a vertix of the polygon to a point on the edge of the circle,
            // or it will go from a point on a line of the polygon to the edge of the circle.

            // If the line comes from a vertix of the polygon, the MTV will be along the line produced
            // by going from the center of the circle to the vertix, and the distance can be found by
            // projecting the cirle on that axis and the polygon on that axis and doing 1D overlap.

            // If the line comes from a point on the edge of the polygon, the MTV will be along the
            // normal of that line, and the distance can be found by projecting the circle on that axis
            // and the polygon on that axis and doing 1D overlap.

            // As with all SAT, if we find any axis that the circle and polygon do not overlap, we've
            // proven they do not intersect.

            // The worst case performance is related to 2x the number of vertices of the polygon, the same speed
            // as for 2 polygons of equal number of vertices.

            HashSet<Vector2> checkedAxis = new HashSet<Vector2>();

            Vector2 bestAxis = Vector2.Zero;
            float shortestOverlap = float.MaxValue;

            Func<Vector2, bool> checkAxis = (axis) =>
            {
                var standard = Math2.MakeStandardNormal(axis);
                if (!checkedAxis.Contains(standard))
                {
                    checkedAxis.Add(standard);
                    var polyProj = Polygon2.ProjectAlongAxis(poly, pos1, rot1, axis);
                    var circleProj = Circle2.ProjectAlongAxis(circle, pos2, axis);

                    var mtv = AxisAlignedLine2.IntersectMTV(polyProj, circleProj);
                    if (!mtv.HasValue)
                        return false;

                    if (Math.Abs(mtv.Value) < Math.Abs(shortestOverlap))
                    {
                        bestAxis = axis;
                        shortestOverlap = mtv.Value;
                    }
                }
                return true;
            };

            var circleCenter = new Vector2(pos2.X + circle.Radius, pos2.Y + circle.Radius);
            int last = poly.Vertices.Length - 1;
            var lastVec = Math2.Rotate(poly.Vertices[last], poly.Center, rot1) + pos1;
            for(int curr = 0; curr < poly.Vertices.Length; curr++)
            {
                var currVec = Math2.Rotate(poly.Vertices[curr], poly.Center, rot1) + pos1;

                // Test along circle center -> vector
                if (!checkAxis(Vector2.Normalize(currVec - circleCenter)))
                    return null;

                // Test along line normal
                if (!checkAxis(Vector2.Normalize(Math2.Perpendicular(currVec - lastVec))))
                    return null;

                last = curr;
                lastVec = currVec;
            }

            return Tuple.Create(bestAxis, shortestOverlap);
        }

        /// <summary>
        /// Determines if the specified circle, at the given position, intersects the specified polygon,
        /// at the given position and rotation.
        /// </summary>
        /// <param name="circle">The circle</param>
        /// <param name="poly">The polygon</param>
        /// <param name="pos1">The top-left of the circles bounding box</param>
        /// <param name="pos2">The origin of the polygon</param>
        /// <param name="rot2">The rotation of the polygon</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns>If circle at pos1 intersects poly at pos2 with rotation rot2</returns>
        public static bool Intersects(Circle2 circle, Polygon2 poly, Vector2 pos1, Vector2 pos2, Rotation2 rot2, bool strict)
        {
            return Intersects(poly, circle, pos2, pos1, rot2, strict);
        }

        /// <summary>
        /// Determines the minimum translation vector that must be applied to the circle at the given position to
        /// prevent overlap with the polygon at the given position and rotation. If the circle and the polygon do
        /// not overlap, returns null. Otherwise, returns a tuple of the unit axis to move the circle in, and the
        /// distance to move the circle.
        /// </summary>
        /// <param name="circle">The circle</param>
        /// <param name="poly">The polygon</param>
        /// <param name="pos1">The top-left of the circles bounding box</param>
        /// <param name="pos2">The origin of the polygon</param>
        /// <param name="rot2">The rotation of the polygon</param>
        /// <returns>The mtv to move the circle at pos1 to prevent overlap with the poly at pos2 with rotation rot2</returns>
        public static Tuple<Vector2, float> IntersectMTV(Circle2 circle, Polygon2 poly, Vector2 pos1, Vector2 pos2, Rotation2 rot2)
        {
            var res = IntersectMTV(poly, circle, pos2, pos1, rot2);
            if (res != null)
                return Tuple.Create(-res.Item1, res.Item2);
            return null;
        }

        /// <summary>
        /// Determines if the specified circle an rectangle intersect at their given positions.
        /// </summary>
        /// <param name="circle">The circle</param>
        /// <param name="rect">The rectangle</param>
        /// <param name="pos1">The top-left of the circles bounding box</param>
        /// <param name="pos2">The origin of the rectangle</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns>If circle at pos1 intersects rect at pos2</returns>
        public static bool Intersects(Circle2 circle, Rect2 rect, Vector2 pos1, Vector2 pos2, bool strict)
        {
            var circleCenter = new Vector2(pos1.X + circle.Radius, pos1.Y + circle.Radius);
            return CircleIntersectsHorizontalLine(circle, new Line2(rect.Min + pos2, rect.UpperRight + pos2), circleCenter, strict)
                || CircleIntersectsHorizontalLine(circle, new Line2(rect.LowerLeft + pos2, rect.Max + pos2), circleCenter, strict)
                || CircleIntersectsVerticalLine(circle, new Line2(rect.Min + pos2, rect.LowerLeft + pos2), circleCenter, strict)
                || CircleIntersectsVerticalLine(circle, new Line2(rect.UpperRight + pos2, rect.Max + pos2), circleCenter, strict)
                || Rect2.Contains(rect, pos2, new Vector2(pos1.X + circle.Radius, pos1.Y + circle.Radius), strict);
        }

        /// <summary>
        /// Determines if the specified rectangle and circle intersect at their given positions.
        /// </summary>
        /// <param name="rect">The rectangle</param>
        /// <param name="circle">The circle</param>
        /// <param name="pos1">The origin of the rectangle</param>
        /// <param name="pos2">The top-left of the circles bounding box</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns></returns>
        public static bool Intersects(Rect2 rect, Circle2 circle, Vector2 pos1, Vector2 pos2, bool strict)
        {
            return Intersects(circle, rect, pos2, pos1, strict);
        }

        /// <summary>
        /// Determines the minimum translation vector to be applied to the circle to
        /// prevent overlap with the rectangle, when they are at their given positions.
        /// </summary>
        /// <param name="circle">The circle</param>
        /// <param name="rect">The rectangle</param>
        /// <param name="pos1">The top-left of the circles bounding box</param>
        /// <param name="pos2">The rectangles origin</param>
        /// <returns>MTV for circle at pos1 to prevent overlap with rect at pos2</returns>
        public static Tuple<Vector2, float> IntersectMTV(Circle2 circle, Rect2 rect, Vector2 pos1, Vector2 pos2)
        {
            // Same as polygon rect, just converted to rects points
            HashSet<Vector2> checkedAxis = new HashSet<Vector2>();

            Vector2 bestAxis = Vector2.Zero;
            float shortestOverlap = float.MaxValue;

            Func<Vector2, bool> checkAxis = (axis) =>
            {
                var standard = Math2.MakeStandardNormal(axis);
                if (!checkedAxis.Contains(standard))
                {
                    checkedAxis.Add(standard);
                    var circleProj = Circle2.ProjectAlongAxis(circle, pos1, axis);
                    var rectProj = Rect2.ProjectAlongAxis(rect, pos2, axis);

                    var mtv = AxisAlignedLine2.IntersectMTV(circleProj, rectProj);
                    if (!mtv.HasValue)
                        return false;

                    if (Math.Abs(mtv.Value) < Math.Abs(shortestOverlap))
                    {
                        bestAxis = axis;
                        shortestOverlap = mtv.Value;
                    }
                }
                return true;
            };

            var circleCenter = new Vector2(pos1.X + circle.Radius, pos1.Y + circle.Radius);
            int last = 4;
            var lastVec = rect.UpperRight + pos2;
            for (int curr = 0; curr < 4; curr++)
            {
                Vector2 currVec = Vector2.Zero;
                switch(curr)
                {
                    case 0:
                        currVec = rect.Min + pos2;
                        break;
                    case 1:
                        currVec = rect.LowerLeft + pos2;
                        break;
                    case 2:
                        currVec = rect.Max + pos2;
                        break;
                    case 3:
                        currVec = rect.UpperRight + pos2;
                        break;
                }

                // Test along circle center -> vector
                if (!checkAxis(Vector2.Normalize(currVec - circleCenter)))
                    return null;

                // Test along line normal
                if (!checkAxis(Vector2.Normalize(Math2.Perpendicular(currVec - lastVec))))
                    return null;

                last = curr;
                lastVec = currVec;
            }

            return Tuple.Create(bestAxis, shortestOverlap);
        }

        /// <summary>
        /// Determines the minimum translation vector to be applied to the rectangle to
        /// prevent overlap with the circle, when they are at their given positions.
        /// </summary>
        /// <param name="rect">The rectangle</param>
        /// <param name="circle">The circle</param>
        /// <param name="pos1">The origin of the rectangle</param>
        /// <param name="pos2">The top-left of the circles bounding box</param>
        /// <returns>MTV for rect at pos1 to prevent overlap with circle at pos2</returns>
        public static Tuple<Vector2, float> IntersectMTV(Rect2 rect, Circle2 circle, Vector2 pos1, Vector2 pos2)
        {
            var res = IntersectMTV(circle, rect, pos2, pos1);
            if (res != null)
                return Tuple.Create(-res.Item1, res.Item2);
            return null;
        }

        /// <summary>
        /// Projects the polygon from the given points with origin pos along the specified axis.
        /// </summary>
        /// <param name="axis">Axis to project onto</param>
        /// <param name="pos">Origin of polygon</param>
        /// <param name="rot">Rotation of the polygon in radians</param>
        /// <param name="center">Center of the polygon</param>
        /// <param name="points">Points of polygon</param>
        /// <returns>Projection of polygon of points at pos along axis</returns>
        protected static AxisAlignedLine2 ProjectAlongAxis(Vector2 axis, Vector2 pos, Rotation2 rot, Vector2 center, params Vector2[] points)
        {
            if (rot == Rotation2.Zero)
                return ProjectAlongAxis(axis, pos, points);

            float min = 0;
            float max = 0;

            for (int i = 0; i < points.Length; i++)
            {
                var polyPt = Math2.Rotate(points[i], center, rot);
                var tmp = Math2.Dot(polyPt.X + pos.X, polyPt.Y + pos.Y, axis.X, axis.Y);

                if (i == 0)
                {
                    min = max = tmp;
                }
                else
                {
                    min = Math.Min(min, tmp);
                    max = Math.Max(max, tmp);
                }
            }

            return new AxisAlignedLine2(axis, min, max);
        }

        /// <summary>
        /// A faster variant of ProjectAlongAxis that assumes no rotation.
        /// </summary>
        /// <param name="axis">The axis that the points are being projected along</param>
        /// <param name="pos">The offset for the points</param>
        /// <param name="points">The points in the convex polygon</param>
        /// <returns>The projectino of the polygon comprised of points at pos along axis</returns>
        protected unsafe static AxisAlignedLine2 ProjectAlongAxis(Vector2 axis, Vector2 pos, Vector2[] points)
        {
            int len = points.Length;
            if (len == 0)
                return new AxisAlignedLine2(axis, 0, 0);

            float min;
            float max;
            fixed(Vector2* pt = points)
            {
                min = axis.X * (pt[0].X + pos.X) + axis.Y * (pt[0].Y + pos.Y);
                max = min;
                for (int i = 1; i < len; i++)
                {
                    float tmp = axis.X * (pt[i].X + pos.X) + axis.Y * (pt[i].Y + pos.Y);

                    if (tmp < min)
                        min = tmp;
                    if (tmp > max)
                        max = tmp;
                }
            }

            return new AxisAlignedLine2(axis, min, max);
        }

        /// <summary>
        /// Determines if the circle whose bounding boxs top left is at the first postion intersects the line
        /// at the second position who is rotated the specified amount about the specified point.
        /// </summary>
        /// <param name="circle">The circle</param>
        /// <param name="line">The line</param>
        /// <param name="pos1">The top-left of the circles bounding box</param>
        /// <param name="pos2">The origin of the line</param>
        /// <param name="rot2">What rotation the line is under</param>
        /// <param name="about2">What the line is rotated about</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns>If the circle at pos1 intersects the line at pos2 rotated rot2 about about2</returns>
        protected static bool CircleIntersectsLine(Circle2 circle, Line2 line, Vector2 pos1, Vector2 pos2, Rotation2 rot2, Vector2 about2, bool strict)
        {
            // Make more math friendly
            var actualLine = new Line2(Math2.Rotate(line.Start, about2, rot2) + pos2, Math2.Rotate(line.End, about2, rot2) + pos2);
            var circleCenter = new Vector2(pos1.X + circle.Radius, pos1.Y + circle.Radius);

            // Check weird situations
            if (actualLine.Horizontal)
                return CircleIntersectsHorizontalLine(circle, actualLine, circleCenter, strict);
            if (actualLine.Vertical)
                return CircleIntersectsVerticalLine(circle, actualLine, circleCenter, strict);

            // Goal:
            // 1. Find closest distance, closestDistance, on the line to the circle (assuming the line was infinite)
            //   1a Determine if closestPoint is intersects the circle according to strict
            //    - If it does not, we've shown there is no intersection.
            // 2. Find closest point, closestPoint, on the line to the circle (assuming the line was infinite)
            // 3. Determine if closestPoint is on the line (including edges)
            //   - If it is, we've shown there is intersection.
            // 4. Determine which edge, edgeClosest, is closest to closestPoint
            // 5. Determine if edgeClosest intersects the circle according to strict
            //   - If it does, we've shown there is intersection
            //   - If it does not, we've shown there is no intersection

            // Step 1
            // We're trying to find closestDistance

            // Recall that the shortest line from a line to a point will be normal to the line
            // Thus, the shortest distance from a line to a point can be found by projecting
            // the line onto it's own normal vector and projecting the point onto the lines
            // normal vector; the distance between those points is the shortest distance from
            // the two points.

            // The projection of a line onto its normal will be a single point, and will be same
            // for any point on that line. So we pick a point that's convienent (the start or end).
            var lineProjectedOntoItsNormal = Vector2.Dot(actualLine.Start, actualLine.Normal);
            var centerOfCircleProjectedOntoNormalOfLine = Vector2.Dot(circleCenter, actualLine.Normal);
            var closestDistance = Math.Abs(centerOfCircleProjectedOntoNormalOfLine - lineProjectedOntoItsNormal);

            // Step 1a
            if(strict)
            {
                if (closestDistance >= circle.Radius)
                    return false;
            }else
            {
                if (closestDistance > circle.Radius)
                    return false;
            }

            // Step 2
            // We're trying to find closestPoint

            // We can just walk the vector from the center to the closest point, which we know is on
            // the normal axis and the distance closestDistance. However it's helpful to get the signed
            // version End - Start to walk.
            var signedDistanceCircleCenterToLine = lineProjectedOntoItsNormal - centerOfCircleProjectedOntoNormalOfLine;
            var closestPoint = circleCenter - actualLine.Normal * signedDistanceCircleCenterToLine;

            // Step 3
            // Determine if closestPoint is on the line (including edges)

            // We're going to accomplish this by projecting the line onto it's own axis and the closestPoint onto the lines
            // axis. Then we have a 1D comparison.
            var lineStartProjectedOntoLineAxis = Vector2.Dot(actualLine.Start, actualLine.Axis);
            var lineEndProjectedOntoLineAxis = Vector2.Dot(actualLine.End, actualLine.Axis);

            var closestPointProjectedOntoLineAxis = Vector2.Dot(closestPoint, actualLine.Axis);

            if (AxisAlignedLine2.Contains(lineStartProjectedOntoLineAxis, lineEndProjectedOntoLineAxis, closestPointProjectedOntoLineAxis, false, true))
            {
                return true;
            }

            // Step 4
            // We're trying to find edgeClosest.
            //
            // We're going to reuse those projections from step 3.
            //
            // (for each "point" in the next paragraph I mean "point projected on the lines axis" but that's wordy)
            //
            // We know that the start is closest iff EITHER the start is less than the end and the
            // closest point is less than the start, OR the start is greater than the end and
            // closest point is greater than the end.

            var closestEdge = Vector2.Zero;
            if (lineStartProjectedOntoLineAxis < lineEndProjectedOntoLineAxis)
                closestEdge = (closestPointProjectedOntoLineAxis <= lineStartProjectedOntoLineAxis) ? actualLine.Start : actualLine.End;
            else
                closestEdge = (closestPointProjectedOntoLineAxis >= lineEndProjectedOntoLineAxis) ? actualLine.Start : actualLine.End;

            // Step 5
            // Circle->Point intersection for closestEdge

            var distToCircleFromClosestEdgeSq = (circleCenter - closestEdge).LengthSquared();
            if (strict)
                return distToCircleFromClosestEdgeSq < (circle.Radius * circle.Radius);
            else
                return distToCircleFromClosestEdgeSq <= (circle.Radius * circle.Radius);

            // If you had trouble following, see the horizontal and vertical cases which are the same process but the projections
            // are simpler
        }

        /// <summary>
        /// Determines if the circle at the specified position intersects the line,
        /// which is at its true position and rotation, when the line is assumed to be horizontal.
        /// </summary>
        /// <param name="circle">The circle</param>
        /// <param name="line">The line</param>
        /// <param name="circleCenter">The center of the circle</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns>If the circle with center circleCenter intersects the horizontal line</returns>
        protected static bool CircleIntersectsHorizontalLine(Circle2 circle, Line2 line, Vector2 circleCenter, bool strict)
        {
            // This is exactly the same process as CircleIntersectsLine, except the projetions are easier
            var lineY = line.Start.Y;

            // Step 1 - Find closest distance
            var vecCircleCenterToLine1D = lineY - circleCenter.Y;
            var closestDistance = Math.Abs(vecCircleCenterToLine1D);

            // Step 1a
            if(strict)
            {
                if (closestDistance >= circle.Radius)
                    return false;
            }else
            {
                if (closestDistance > circle.Radius)
                    return false;
            }

            // Step 2 - Find closest point
            var closestPointX = circleCenter.X;

            // Step 3 - Is closest point on line
            if (AxisAlignedLine2.Contains(line.Start.X, line.End.X, closestPointX, false, true))
                return true;

            // Step 4 - Find edgeClosest
            float edgeClosestX;
            if (line.Start.X < line.End.X)
                edgeClosestX = (closestPointX <= line.Start.X) ? line.Start.X : line.End.X;
            else
                edgeClosestX = (closestPointX >= line.Start.X) ? line.Start.X : line.End.X;

            // Step 5 - Circle-point intersection on closest point
            var distClosestEdgeToCircleSq = new Vector2(circleCenter.X - edgeClosestX, circleCenter.Y - lineY).LengthSquared();

            if (strict)
                return distClosestEdgeToCircleSq < circle.Radius * circle.Radius;
            else
                return distClosestEdgeToCircleSq <= circle.Radius * circle.Radius;
        }

        /// <summary>
        /// Determines if the circle at the specified position intersects the line, which
        /// is at its true position and rotation, when the line is assumed to be vertical
        /// </summary>
        /// <param name="circle">The circle</param>
        /// <param name="line">The line</param>
        /// <param name="circleCenter">The center of the circle</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns>If the circle with center circleCenter intersects the line</returns>
        protected static bool CircleIntersectsVerticalLine(Circle2 circle, Line2 line, Vector2 circleCenter, bool strict)
        {
            // Same process as horizontal, but axis flipped
            var lineX = line.Start.X;
            // Step 1 - Find closest distance
            var vecCircleCenterToLine1D = lineX - circleCenter.X;
            var closestDistance = Math.Abs(vecCircleCenterToLine1D);

            // Step 1a
            if (strict)
            {
                if (closestDistance >= circle.Radius)
                    return false;
            }
            else
            {
                if (closestDistance > circle.Radius)
                    return false;
            }

            // Step 2 - Find closest point
            var closestPointY = circleCenter.Y;

            // Step 3 - Is closest point on line
            if (AxisAlignedLine2.Contains(line.Start.Y, line.End.Y, closestPointY, false, true))
                return true;

            // Step 4 - Find edgeClosest
            float edgeClosestY;
            if (line.Start.Y < line.End.Y)
                edgeClosestY = (closestPointY <= line.Start.Y) ? line.Start.Y : line.End.Y;
            else
                edgeClosestY = (closestPointY >= line.Start.Y) ? line.Start.Y : line.End.Y;

            // Step 5 - Circle-point intersection on closest point
            var distClosestEdgeToCircleSq = new Vector2(circleCenter.X - lineX, circleCenter.Y - edgeClosestY).LengthSquared();

            if (strict)
                return distClosestEdgeToCircleSq < circle.Radius * circle.Radius;
            else
                return distClosestEdgeToCircleSq <= circle.Radius * circle.Radius;
        }
        #region NoRotation
        /// <summary>
        /// Determines if the specified polygon at pos1 with no rotation and rectangle at pos2 intersect
        /// </summary>
        /// <param name="poly">Polygon to check</param>
        /// <param name="rect">Rectangle to check</param>
        /// <param name="pos1">Origin of polygon</param>
        /// <param name="pos2">Origin of rect</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns>If poly at pos1 intersects rect at pos2</returns>
        public static bool Intersects(Polygon2 poly, Rect2 rect, Vector2 pos1, Vector2 pos2, bool strict)
        {
            return Intersects(poly, rect, pos1, pos2, Rotation2.Zero, strict);
        }

        /// <summary>
        /// Determines if the specified rectangle at pos1 intersects the specified polygon at pos2 with
        /// no rotation.
        /// </summary>
        /// <param name="rect">The rectangle</param>
        /// <param name="poly">The polygon</param>
        /// <param name="pos1">Origin of rectangle</param>
        /// <param name="pos2">Origin of polygon</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns>If rect at pos1 no rotation intersects poly at pos2</returns>
        public static bool Intersects(Rect2 rect, Polygon2 poly, Vector2 pos1, Vector2 pos2, bool strict)
        {
            return Intersects(rect, poly, pos1, pos2, Rotation2.Zero, strict);
        }

        /// <summary>
        /// Determines if the specified polygon at pos1 with no rotation intersects the specified
        ///
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="rect"></param>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        public static Tuple<Vector2, float> IntersectMTV(Polygon2 poly, Rect2 rect, Vector2 pos1, Vector2 pos2)
        {
            return IntersectMTV(poly, rect, pos1, pos2, Rotation2.Zero);
        }

        /// <summary>
        /// Determines the minimum translation vector to be applied to the rect to prevent
        /// intersection with the specified polygon, when they are at the given positions.
        /// </summary>
        /// <param name="rect">The rect</param>
        /// <param name="poly">The polygon</param>
        /// <param name="pos1">The origin of the rect</param>
        /// <param name="pos2">The origin of the polygon</param>
        /// <returns>MTV to move rect at pos1 to prevent overlap with poly at pos2</returns>
        public static Tuple<Vector2, float> IntersectMTV(Rect2 rect, Polygon2 poly, Vector2 pos1, Vector2 pos2)
        {
            return IntersectMTV(rect, poly, pos1, pos2, Rotation2.Zero);
        }

        /// <summary>
        /// Determines if the polygon and circle intersect when at the given positions.
        /// </summary>
        /// <param name="poly">The polygon</param>
        /// <param name="circle">The circle</param>
        /// <param name="pos1">The origin of the polygon</param>
        /// <param name="pos2">The top-left of the circles bounding box</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns>If poly at pos1 intersects circle at pos2</returns>
        public static bool Intersects(Polygon2 poly, Circle2 circle, Vector2 pos1, Vector2 pos2, bool strict)
        {
            return Intersects(poly, circle, pos1, pos2, Rotation2.Zero, strict);
        }

        /// <summary>
        /// Determines if the circle and polygon intersect when at the given positions.
        /// </summary>
        /// <param name="circle">The circle</param>
        /// <param name="poly">The polygon</param>
        /// <param name="pos1">The top-left of the circles bounding box</param>
        /// <param name="pos2">The origin of the polygon</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns>If circle at pos1 intersects poly at pos2</returns>
        public static bool Intersects(Circle2 circle, Polygon2 poly, Vector2 pos1, Vector2 pos2, bool strict)
        {
            return Intersects(circle, poly, pos1, pos2, Rotation2.Zero, strict);
        }

        /// <summary>
        /// Determines the minimum translation vector the be applied to the polygon to prevent
        /// intersection with the specified circle, when they are at the given positions.
        /// </summary>
        /// <param name="poly">The polygon</param>
        /// <param name="circle">The circle</param>
        /// <param name="pos1">The position of the polygon</param>
        /// <param name="pos2">The top-left of the circles bounding box</param>
        /// <returns>MTV to move poly at pos1 to prevent overlap with circle at pos2</returns>
        public static Tuple<Vector2, float> IntersectMTV(Polygon2 poly, Circle2 circle, Vector2 pos1, Vector2 pos2)
        {
            return IntersectMTV(poly, circle, pos1, pos2, Rotation2.Zero);
        }

        /// <summary>
        /// Determines the minimum translation vector to be applied to the circle to prevent
        /// intersection with the specified polyogn, when they are at the given positions.
        /// </summary>
        /// <param name="circle">The circle</param>
        /// <param name="poly">The polygon</param>
        /// <param name="pos1">The top-left of the circles bounding box</param>
        /// <param name="pos2">The origin of the polygon</param>
        /// <returns></returns>
        public static Tuple<Vector2, float> IntersectMTV(Circle2 circle, Polygon2 poly, Vector2 pos1, Vector2 pos2)
        {
            return IntersectMTV(circle, poly, pos1, pos2, Rotation2.Zero);
        }
        #endregion
    }
}

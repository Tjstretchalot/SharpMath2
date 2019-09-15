using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SharpMath2
{
    public enum LineInterType
    {
        /// <summary>
        /// Two segments with different slopes which do not intersect
        /// </summary>
        NonParallelNone,
        /// <summary>
        /// Two segments with different slopes which intersect at a 
        /// single point.
        /// </summary>
        NonParallelPoint,
        /// <summary>
        /// Two parallel but not coincident segments. These never intersect
        /// </summary>
        ParallelNone,
        /// <summary>
        /// Two coincident segments which do not intersect
        /// </summary>
        CoincidentNone,
        /// <summary>
        /// Two coincident segments which intersect at a point
        /// </summary>
        CoincidentPoint,
        /// <summary>
        /// Two coincident segments which intersect on infinitely many points
        /// </summary>
        CoincidentLine
    }

    /// <summary>
    /// Describes a line. Does not have position and is meant to be reused.
    /// </summary>
    public class Line2
    {
        /// <summary>
        /// Where the line begins
        /// </summary>
        public readonly Vector2 Start;

        /// <summary>
        /// Where the line ends
        /// </summary>
        public readonly Vector2 End;

        /// <summary>
        /// End - Start
        /// </summary>
        public readonly Vector2 Delta;

        /// <summary>
        /// Normalized Delta
        /// </summary>
        public readonly Vector2 Axis;

        /// <summary>
        /// The normalized normal of axis.
        /// </summary>
        public readonly Vector2 Normal;

        /// <summary>
        /// Square of the magnitude of this line
        /// </summary>
        public readonly float MagnitudeSquared;

        /// <summary>
        /// Magnitude of this line
        /// </summary>
        public readonly float Magnitude;

        /// <summary>
        /// Min x
        /// </summary>
        public readonly float MinX;
        /// <summary>
        /// Min y
        /// </summary>
        public readonly float MinY;

        /// <summary>
        /// Max x
        /// </summary>
        public readonly float MaxX;

        /// <summary>
        /// Max y
        /// </summary>
        public readonly float MaxY;

        /// <summary>
        /// Slope of this line
        /// </summary>
        public readonly float Slope;

        /// <summary>
        /// Where this line would hit the y intercept. NaN if vertical line.
        /// </summary>
        public readonly float YIntercept;

        /// <summary>
        /// If this line is horizontal
        /// </summary>
        public readonly bool Horizontal;

        /// <summary>
        /// If this line is vertical
        /// </summary>
        public readonly bool Vertical;

        /// <summary>
        /// Creates a line from start to end
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="end">End</param>
        public Line2(Vector2 start, Vector2 end)
        {
            if (Math2.Approximately(start, end))
                throw new ArgumentException($"start is approximately end - that's a point, not a line. start={start}, end={end}");

            Start = start;
            End = end;


            Delta = End - Start;
            Axis = Vector2.Normalize(Delta);
            Normal = Vector2.Normalize(Math2.Perpendicular(Delta));
            MagnitudeSquared = Delta.LengthSquared();
            Magnitude = (float)Math.Sqrt(MagnitudeSquared);

            MinX = Math.Min(Start.X, End.X);
            MinY = Math.Min(Start.Y, End.Y);
            MaxX = Math.Max(Start.X, End.X);
            MaxY = Math.Max(Start.Y, End.Y);
            Horizontal = Math.Abs(End.Y - Start.Y) <= Math2.DEFAULT_EPSILON;
            Vertical = Math.Abs(End.X - Start.X) <= Math2.DEFAULT_EPSILON;

            if (Vertical)
                Slope = float.PositiveInfinity;
            else
                Slope = (End.Y - Start.Y) / (End.X - Start.X);

            if (Vertical)
                YIntercept = float.NaN;
            else
            {
                // y = mx + b
                // Start.Y = Slope * Start.X + b
                // b = Start.Y - Slope * Start.X
                YIntercept = Start.Y - Slope * Start.X;
            }
        }

        /// <summary>
        /// Determines if the two lines are parallel. Shifting lines will not
        /// effect the result.
        /// </summary>
        /// <param name="line1">The first line</param>
        /// <param name="line2">The second line</param>
        /// <returns>True if the lines are parallel, false otherwise</returns>
        public static bool Parallel(Line2 line1, Line2 line2)
        {
            return (
                Math2.Approximately(line1.Axis, line2.Axis)
                || Math2.Approximately(line1.Axis, -line2.Axis)
                );
        }

        /// <summary>
        /// Determines if the given point is along the infinite line described
        /// by the given line shifted the given amount.
        /// </summary>
        /// <param name="line">The line</param>
        /// <param name="pos">The shift for the line</param>
        /// <param name="pt">The point</param>
        /// <returns>True if pt is on the infinite line extension of the segment</returns>
        public static bool AlongInfiniteLine(Line2 line, Vector2 pos, Vector2 pt)
        {
            float normalPart = Vector2.Dot(pt - pos - line.Start, line.Normal);
            return Math2.Approximately(normalPart, 0);
        }

        /// <summary>
        /// Determines if the given line contains the given point.
        /// </summary>
        /// <param name="line">The line to check</param>
        /// <param name="pos">The offset for the line</param>
        /// <param name="pt">The point to check</param>
        /// <returns>True if pt is on the line, false otherwise</returns>
        public static bool Contains(Line2 line, Vector2 pos, Vector2 pt)
        {
            // The horizontal/vertical checks are not required but are
            // very fast to calculate and short-circuit the common case
            // (false) very quickly
            if(line.Horizontal)
            {
                return Math2.Approximately(line.Start.Y + pos.Y, pt.Y)
                    && AxisAlignedLine2.Contains(line.MinX, line.MaxX, pt.X - pos.X, false, false);
            }
            if(line.Vertical)
            {
                return Math2.Approximately(line.Start.X + pos.X, pt.X)
                    && AxisAlignedLine2.Contains(line.MinY, line.MaxY, pt.Y - pos.Y, false, false);
            }

            // Our line is not necessarily a linear space, but if we shift
            // our line to the origin and adjust the point correspondingly
            // then we have a linear space and the problem remains the same.

            // Our line at the origin is just the infinite line with slope
            // Axis. We can form an orthonormal basis of R2 as (Axis, Normal).
            // Hence we can write pt = line_part * Axis + normal_part * Normal. 
            // where line_part and normal_part are floats. If the normal_part
            // is 0, then pt = line_part * Axis, hence the point is on the
            // infinite line.

            // Since we are working with an orthonormal basis, we can find
            // components with dot products.

            // To check the finite line, we consider the start of the line
            // the origin. Then the end of the line is line.Magnitude * line.Axis.

            Vector2 lineStart = pos + line.Start;

            float normalPart = Math2.Dot(pt - lineStart, line.Normal);
            if (!Math2.Approximately(normalPart, 0))
                return false;

            float axisPart = Math2.Dot(pt - lineStart, line.Axis);
            return axisPart > -Math2.DEFAULT_EPSILON 
                && axisPart < line.Magnitude + Math2.DEFAULT_EPSILON;
        }

        private static unsafe void FindSortedOverlap(float* projs, bool* isFromLine1)
        {
            // ascending insertion sort while simultaneously updating 
            // isFromLine1
            for (int i = 0; i < 3; i++)
            {
                int best = i;
                for (int j = i + 1; j < 4; j++)
                {
                    if (projs[j] < projs[best])
                    {
                        best = j;
                    }
                }
                if (best != i)
                {
                    float projTmp = projs[i];
                    projs[i] = projs[best];
                    projs[best] = projTmp;
                    bool isFromLine1Tmp = isFromLine1[i];
                    isFromLine1[i] = isFromLine1[best];
                    isFromLine1[best] = isFromLine1Tmp;
                }
            }
        }

        /// <summary>
        /// Checks the type of intersection between the two coincident lines.
        /// </summary>
        /// <param name="a">The first line</param>
        /// <param name="b">The second line</param>
        /// <param name="pos1">The offset for the first line</param>
        /// <param name="pos2">The offset for the second line</param>
        /// <returns>The type of intersection</returns>
        public static unsafe LineInterType CheckCoincidentIntersectionType(Line2 a, Line2 b, Vector2 pos1, Vector2 pos2)
        {
            Vector2 relOrigin = a.Start + pos1;

            float* projs = stackalloc float[4] {
                0,
                a.Magnitude,
                Math2.Dot((b.Start + pos2) - relOrigin, a.Axis),
                Math2.Dot((b.End + pos2) - relOrigin, a.Axis)
            };

            bool* isFromLine1 = stackalloc bool[4] {
                true,
                true,
                false,
                false
            };

            FindSortedOverlap(projs, isFromLine1);

            if (Math2.Approximately(projs[1], projs[2]))
                return LineInterType.CoincidentPoint;
            if (isFromLine1[0] == isFromLine1[1])
                return LineInterType.CoincidentNone;
            return LineInterType.CoincidentLine;
        }

        /// <summary>
        /// Determines if line1 intersects line2, when line1 is offset by pos1 and line2 
        /// is offset by pos2.
        /// </summary>
        /// <param name="line1">Line 1</param>
        /// <param name="line2">Line 2</param>
        /// <param name="pos1">Origin of line 1</param>
        /// <param name="pos2">Origin of line 2</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns>If line1 intersects line2</returns>
        public static unsafe bool Intersects(Line2 line1, Line2 line2, Vector2 pos1, Vector2 pos2, bool strict)
        {
            if (Parallel(line1, line2))
            {
                if (!AlongInfiniteLine(line1, pos1, line2.Start + pos2))
                    return false;
                LineInterType iType = CheckCoincidentIntersectionType(line1, line2, pos1, pos2);
                if (iType == LineInterType.CoincidentNone)
                    return false;
                if (iType == LineInterType.CoincidentPoint)
                    return !strict;
                return true;
            }

            return GetIntersection(line1, line2, pos1, pos2, strict, out Vector2 pt);
        }

        /// <summary>
        /// Finds the intersection of two non-parallel lines a and b. Returns
        /// true if the point of intersection is on both line segments, returns
        /// false if the point of intersection is not on at least one line
        /// segment. In either case, pt is set to where the intersection is
        /// on the infinite lines.
        /// </summary>
        /// <param name="line1">First line</param>
        /// <param name="line2">Second line</param>
        /// <param name="pos1">The shift of the first line</param>
        /// <param name="pos2">The shift of the second line</param>
        /// <param name="strict">True if we should return true if pt is on an edge of a line as well
        /// as in the middle of the line. False to return true only if pt is really within the lines</param>
        /// <returns>True if both segments contain the pt, false otherwise</returns>
        public static unsafe bool GetIntersection(Line2 line1, Line2 line2, Vector2 pos1, Vector2 pos2, bool strict, out Vector2 pt)
        {
            // The infinite lines intersect at exactly one point. The segments intersect
            // if they both contain that point. We will treat the lines as first-degree
            // Bezier lines to skip the vertical case
            // https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection

            float x1 = line1.Start.X + pos1.X;
            float x2 = line1.End.X + pos1.X;
            float x3 = line2.Start.X + pos2.X;
            float x4 = line2.End.X + pos2.X;
            float y1 = line1.Start.Y + pos1.Y;
            float y2 = line1.End.Y + pos1.Y;
            float y3 = line2.Start.Y + pos2.Y;
            float y4 = line2.End.Y + pos2.Y;

            float det = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            // we assume det != 0 (lines not parallel)

            var t = (
                ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / det
            );

            pt = new Vector2(x1 + (x2 - x1) * t, y1 + (y2 - y1) * t);

            float min = strict ? Math2.DEFAULT_EPSILON : -Math2.DEFAULT_EPSILON;
            float max = 1 - min;

            if (t < min || t > max)
                return false;

            float u = -(
                ((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / det
            );
            return u >= min && u <= max;
        }

        /// <summary>
        /// Finds the line of overlap between the the two lines if there is
        /// one. If the two lines are not coincident (i.e., if the infinite
        /// lines are not the same) then they don't share a line of points.
        /// If they are coincident, they may still share no points (two
        /// seperate but coincident line segments), one point (they share
        /// an edge), or infinitely many points (the share a coincident
        /// line segment). In all but the last case, this returns false
        /// and overlap is set to null. In the last case this returns true
        /// and overlap is set to the line of overlap.
        /// </summary>
        /// <param name="a">The first line</param>
        /// <param name="b">The second line</param>
        /// <param name="pos1">The position of the first line</param>
        /// <param name="pos2">the position of the second line</param>
        /// <param name="overlap">Set to null or the line of overlap</param>
        /// <returns>True if a and b overlap at infinitely many points,
        /// false otherwise</returns>
        public static unsafe bool LineOverlap(Line2 a, Line2 b, Vector2 pos1, Vector2 pos2, out Line2 overlap)
        {
            if (!Parallel(a, b))
            {
                overlap = null;
                return false;
            }
            if (!AlongInfiniteLine(a, pos1, b.Start + pos2))
            {
                overlap = null;
                return false;
            }

            Vector2 relOrigin = a.Start + pos1;

            float* projs = stackalloc float[4] {
                0,
                a.Magnitude,
                Math2.Dot((b.Start + pos2) - relOrigin, a.Axis),
                Math2.Dot((b.End + pos2) - relOrigin, a.Axis)
            };

            bool* isFromLine1 = stackalloc bool[4] {
                true,
                true,
                false,
                false
            };

            FindSortedOverlap(projs, isFromLine1);

            if (isFromLine1[0] == isFromLine1[1])
            {
                // at best we overlap at one point, most likely no overlap
                overlap = null;
                return false;
            }

            if (Math2.Approximately(projs[1], projs[2]))
            {
                // Overlap at one point
                overlap = null;
                return false;
            }

            overlap = new Line2(
                relOrigin + projs[1] * a.Axis,
                relOrigin + projs[2] * a.Axis
            );
            return true;
        }

        /// <summary>
        /// Create a human-readable representation of this line
        /// </summary>
        /// <returns>human-readable string</returns>
        public override string ToString()
        {
            return $"[{Start} to {End}]";
        }
    }
}

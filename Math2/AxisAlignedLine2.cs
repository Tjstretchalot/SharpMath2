using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SharpMath2
{
    /// <summary>
    /// Describes a line that's projected onto a specified axis. This is a useful
    /// mathematical concept. Axis aligned lines *do* have position because they 
    /// are only used as an interim calculation, where position won't change.
    /// </summary>
    public class AxisAlignedLine2
    {
        /// <summary>
        /// The axis that this projected line is on. Optional.
        /// </summary>
        public readonly Vector2 Axis;

        /// <summary>
        /// The minimum of this line
        /// </summary>
        public readonly float Min;

        /// <summary>
        /// The maximum of this line
        /// </summary>
        public readonly float Max;

        /// <summary>
        /// Initializes an an axis aligned line. Will autocorrect if min &gt; max
        /// </summary>
        /// <param name="axis">The axis</param>
        /// <param name="min">The min</param>
        /// <param name="max">The max</param>
        public AxisAlignedLine2(Vector2 axis, float min, float max)
        {
            Axis = axis;

            Min = Math.Min(min, max);
            Max = Math.Max(min, max);
        }

        /// <summary>
        /// Determines if line1 intersects line2.
        /// </summary>
        /// <param name="line1">Line 1</param>
        /// <param name="line2">Line 2</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns>If line1 and line2 intersect</returns>
        /// <exception cref="ArgumentException">if line1.Axis != line2.Axis</exception>
        public static bool Intersects(AxisAlignedLine2 line1, AxisAlignedLine2 line2, bool strict)
        {
            if (line1.Axis != line2.Axis)
                throw new ArgumentException($"Lines {line1} and {line2} are not aligned - you will need to convert to Line2 to check intersection.");

            return Intersects(line1.Min, line1.Max, line2.Min, line2.Max, strict, false);
        }

        /// <summary>
        /// Determines the best way for line1 to move to prevent intersection with line2
        /// </summary>
        /// <param name="line1">Line1</param>
        /// <param name="line2">Line2</param>
        /// <returns>MTV for line1</returns>
        public static float? IntersectMTV(AxisAlignedLine2 line1, AxisAlignedLine2 line2)
        {
            if (line1.Axis != line2.Axis)
                throw new ArgumentException($"Lines {line1} and {line2} are not aligned - you will need to convert to Line2 to check intersection.");

            return IntersectMTV(line1.Min, line1.Max, line2.Min, line2.Max, false);
        }

        /// <summary>
        /// Determines if the specified line contains the specified point.
        /// </summary>
        /// <param name="line">The line</param>
        /// <param name="point">The point</param>
        /// <param name="strict">If the edges of the line are excluded</param>
        /// <returns>if line contains point</returns>
        public static bool Contains(AxisAlignedLine2 line, float point, bool strict)
        {
            return Contains(line.Min, line.Max, point, strict, false);
        }

        /// <summary>
        /// Determines if axis aligned line (min1, max1) intersects (min2, max2)
        /// </summary>
        /// <param name="min1">Min 1</param>
        /// <param name="max1">Max 1</param>
        /// <param name="min2">Min 2</param>
        /// <param name="max2">Max 2</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <param name="correctMinMax">If true (default true) mins and maxes will be swapped if in the wrong order</param>
        /// <returns>If (min1, max1) intersects (min2, max2)</returns>
        public static bool Intersects(float min1, float max1, float min2, float max2, bool strict, bool correctMinMax = true)
        {
            if (correctMinMax)
            {
                float tmp1 = min1, tmp2 = max1;
                min1 = Math.Min(tmp1, tmp2);
                max1 = Math.Max(tmp1, tmp2);

                tmp1 = min2;
                tmp2 = max2;
                min2 = Math.Min(tmp1, tmp2);
                max2 = Math.Max(tmp1, tmp2);

            }

            if (strict)
                return (min1 <= min2 && max1 > min2) || (min2 <= min1 && max2 > min1);
            else
                return (min1 <= min2 && max1 >= min2) || (min2 <= min1 && max2 >= min1);
        }

        /// <summary>
        /// Determines the translation to move line 1 to have line 1 not intersect line 2. Returns
        /// null if line1 does not intersect line1.
        /// </summary>
        /// <param name="min1">Line 1 min</param>
        /// <param name="max1">Line 1 max</param>
        /// <param name="min2">Line 2 min</param>
        /// <param name="max2">Line 2 max</param>
        /// <param name="correctMinMax">If mins and maxs might be reversed</param>
        /// <returns>a number to move along the projected axis (positive or negative) or null if no intersection</returns>
        public static float? IntersectMTV(float min1, float max1, float min2, float max2, bool correctMinMax = true)
        {
            if (correctMinMax)
            {
                float tmp1 = min1, tmp2 = max1;
                min1 = Math.Min(tmp1, tmp2);
                max1 = Math.Max(tmp1, tmp2);

                tmp1 = min2;
                tmp2 = max2;
                min2 = Math.Min(tmp1, tmp2);
                max2 = Math.Max(tmp1, tmp2);
            }

            if (min1 <= min2 && max1 > min2)
                return min2 - max1;
            else if (min2 <= min1 && max2 > min1)
                return max2 - min1;
            return null;
        }

        /// <summary>
        /// Determines if the line from (min, max) contains point
        /// </summary>
        /// <param name="min">Min of line</param>
        /// <param name="max">Max of line</param>
        /// <param name="point">Point to check</param>
        /// <param name="strict">If edges are excluded</param>
        /// <param name="correctMinMax">if true (default true) min and max will be swapped if in the wrong order</param>
        /// <returns>if line (min, max) contains point</returns>
        public static bool Contains(float min, float max, float point, bool strict, bool correctMinMax = true)
        {
            if (correctMinMax)
            {
                float tmp1 = min, tmp2 = max;
                min = Math.Min(tmp1, tmp2);
                max = Math.Max(tmp1, tmp2);
            }

            if (strict)
                return min < point && max > point;
            else
                return min <= point && max >= point;
        }

        /// <summary>
        /// Detrmines the shortest distance from the line to get to point. Returns
        /// null if the point is on the line (not strict). Always returns a positive value.
        /// </summary>
        /// <returns>The distance.</returns>
        /// <param name="line">Line.</param>
        /// <param name="point">Point.</param>
        public static float? MinDistance(AxisAlignedLine2 line, float point)
        {
            return MinDistance(line.Min, line.Max, point, false);
        }

        /// <summary>
        /// Determines the shortest distance for line1 to go to touch line2. Returns
        /// null if line1 and line 2 intersect (not strictly)
        /// </summary>
        /// <returns>The distance.</returns>
        /// <param name="line1">Line1.</param>
        /// <param name="line2">Line2.</param>
        public static float? MinDistance(AxisAlignedLine2 line1, AxisAlignedLine2 line2)
        {
            return MinDistance(line1.Min, line1.Max, line2.Min, line2.Max, false);
        }

        /// <summary>
        /// Determines the shortest distance from the line (min, max) to the point. Returns
        /// null if the point is on the line (not strict). Always returns a positive value.
        /// </summary>
        /// <returns>The distance.</returns>
        /// <param name="min">Minimum of line.</param>
        /// <param name="max">Maximum of line.</param>
        /// <param name="point">Point to check.</param>
        /// <param name="correctMinMax">If set to <c>true</c> will correct minimum max being reversed if they are</param>
        public static float? MinDistance(float min, float max, float point, bool correctMinMax = true)
        {
            if (correctMinMax)
            {
                float tmp1 = min, tmp2 = max;
                min = Math.Min(tmp1, tmp2);
                max = Math.Max(tmp1, tmp2);
            }

            if (point < min)
                return min - point;
            else if (point > max)
                return point - max;
            else
                return null;
        }

        /// <summary>
        /// Calculates the shortest distance for line1 (min1, max1) to get to line2 (min2, max2).
        /// Returns null if line1 and line2 intersect (not strictly)
        /// </summary>
        /// <returns>The distance along the mutual axis or null.</returns>
        /// <param name="min1">Min1.</param>
        /// <param name="max1">Max1.</param>
        /// <param name="min2">Min2.</param>
        /// <param name="max2">Max2.</param>
        /// <param name="correctMinMax">If set to <c>true</c> correct minimum max being potentially reversed.</param>
        public static float? MinDistance(float min1, float max1, float min2, float max2, bool correctMinMax = true)
        {
            if (correctMinMax)
            {
                float tmp1 = min1, tmp2 = max1;
                min1 = Math.Min(tmp1, tmp2);
                max1 = Math.Max(tmp1, tmp2);

                tmp1 = min2;
                tmp2 = max2;
                min2 = Math.Min(tmp1, tmp2);
                max2 = Math.Max(tmp1, tmp2);
            }

            if (min1 < min2)
            {
                if (max1 < min2)
                    return min2 - max1;
                else
                    return null;
            }
            else if (min2 < min1)
            {
                if (max2 < min1)
                    return min1 - max2;
                else
                    return null;
            }

            return null;
        }

        /// <summary>
        /// Creates a human-readable representation of this line
        /// </summary>
        /// <returns>string representation of this vector</returns>
        public override string ToString()
        {
            return $"[{Min} -> {Max} along {Axis}]";
        }
    }
}

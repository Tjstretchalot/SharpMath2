using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMath2
{
    /// <summary>
    /// Describes a circle in the x-y plane.
    /// </summary>
    public struct Circle2
    {
        /// <summary>
        /// The radius of the circle
        /// </summary>
        public readonly float Radius;

        /// <summary>
        /// Constructs a circle with the specified radius
        /// </summary>
        /// <param name="radius">Radius of the circle</param>
        public Circle2(float radius)
        {
            Radius = radius;
        }

        /// <summary>
        /// Determines if the first circle is equal to the second circle
        /// </summary>
        /// <param name="c1">The first circle</param>
        /// <param name="c2">The second circle</param>
        /// <returns>If c1 is equal to c2</returns>
        public static bool operator ==(Circle2 c1, Circle2 c2)
        {
            if (ReferenceEquals(c1, null) || ReferenceEquals(c2, null))
                return ReferenceEquals(c1, c2);

            return c1.Radius == c2.Radius;
        }

        /// <summary>
        /// Determines if the first circle is not equal to the second circle
        /// </summary>
        /// <param name="c1">The first circle</param>
        /// <param name="c2">The second circle</param>
        /// <returns>If c1 is not equal to c2</returns>
        public static bool operator !=(Circle2 c1, Circle2 c2)
        {
            if (ReferenceEquals(c1, null) || ReferenceEquals(c2, null))
                return !ReferenceEquals(c1, c2);

            return c1.Radius != c2.Radius;
        }
        
        /// <summary>
        /// Determines if this circle is logically the same as the 
        /// specified object.
        /// </summary>
        /// <param name="obj">The object to compare against</param>
        /// <returns>if it is a circle with the same radius</returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Circle2))
                return false;

            var other = (Circle2)obj;
            return this == other;
        }

        /// <summary>
        /// Calculate a hashcode based solely on the radius of this circle.
        /// </summary>
        /// <returns>hashcode</returns>
        public override int GetHashCode()
        {
            return Radius.GetHashCode();
        }

        /// <summary>
        /// Determines if the circle at the specified position contains the point
        /// </summary>
        /// <param name="circle">The circle</param>
        /// <param name="pos">The top-left of the circles bounding box</param>
        /// <param name="point">The point to check if is in the circle at pos</param>
        /// <param name="strict">If the edges do not count</param>
        /// <returns>If the circle at pos contains point</returns>
        public static bool Contains(Circle2 circle, Vector2 pos, Vector2 point, bool strict)
        {
            var distSq = (point - new Vector2(pos.X + circle.Radius, pos.Y + circle.Radius)).LengthSquared();

            if (strict)
                return distSq < circle.Radius * circle.Radius;
            else
                return distSq <= circle.Radius * circle.Radius;
        }
        
        /// <summary>
        /// Determines if the first circle at the specified position intersects the second circle
        /// at the specified position.
        /// </summary>
        /// <param name="circle1">First circle</param>
        /// <param name="circle2">Second circle</param>
        /// <param name="pos1">Top-left of the bounding box of the first circle</param>
        /// <param name="pos2">Top-left of the bounding box of the second circle</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns>If circle1 at pos1 intersects circle2 at pos2</returns>
        public static bool Intersects(Circle2 circle1, Circle2 circle2, Vector2 pos1, Vector2 pos2, bool strict)
        {
            return Intersects(circle1.Radius, circle2.Radius, pos1, pos2, strict);
        }

        /// <summary>
        /// Determines if the first circle of specified radius and (bounding box top left) intersects
        /// the second circle of specified radius and (bounding box top left)
        /// </summary>
        /// <param name="radius1">Radius of the first circle</param>
        /// <param name="radius2">Radius of the second circle</param>
        /// <param name="pos1">Top-left of the bounding box of the first circle</param>
        /// <param name="pos2">Top-left of the bounding box of the second circle</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns>If circle1 of radius=radius1, topleft=pos1 intersects circle2 of radius=radius2, topleft=pos2</returns>
        public static bool Intersects(float radius1, float radius2, Vector2 pos1, Vector2 pos2, bool strict)
        {
            var vecCenterToCenter = pos1 - pos2;
            vecCenterToCenter.X += radius1 - radius2;
            vecCenterToCenter.Y += radius1 - radius2;
            var distSq = vecCenterToCenter.LengthSquared();
            return distSq < (radius1 + radius2) * (radius1 + radius2);
        }

        /// <summary>
        /// Determines the shortest axis and overlap for which the first circle at the specified position
        /// overlaps the second circle at the specified position. If the circles do not overlap, returns null.
        /// </summary>
        /// <param name="circle1">First circle</param>
        /// <param name="circle2">Second circle</param>
        /// <param name="pos1">Top-left of the first circles bounding box</param>
        /// <param name="pos2">Top-left of the second circles bounding box</param>
        /// <returns></returns>
        public static Tuple<Vector2, float> IntersectMTV(Circle2 circle1, Circle2 circle2, Vector2 pos1, Vector2 pos2)
        {
            return IntersectMTV(circle1.Radius, circle2.Radius, pos1, pos2);
        }

        /// <summary>
        /// Determines the shortest axis and overlap for which the first circle, specified by its radius and its bounding
        /// box's top-left, intersects the second circle specified by its radius and bounding box top-left. Returns null if
        /// the circles do not overlap.
        /// </summary>
        /// <param name="radius1">Radius of the first circle</param>
        /// <param name="radius2"></param>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <returns>The direction and magnitude to move pos1 to prevent intersection</returns>
        public static Tuple<Vector2, float> IntersectMTV(float radius1, float radius2, Vector2 pos1, Vector2 pos2)
        {
            var betweenVec = pos1 - pos2;
            betweenVec.X += (radius1 - radius2);
            betweenVec.Y += (radius1 - radius2);

            var lengthSq = betweenVec.LengthSquared();
            if(lengthSq < (radius1 + radius2) * (radius1 + radius2))
            {
                var len = Math.Sqrt(lengthSq);
                betweenVec *= (float)(1 / len);

                return Tuple.Create(betweenVec, radius1 + radius2 - (float)len);
            }
            return null;
        }

        /// <summary>
        /// Projects the specified circle with the upper-left at the specified position onto
        /// the specified axis. 
        /// </summary>
        /// <param name="circle">The circle</param>
        /// <param name="pos">The position of the circle</param>
        /// <param name="axis">the axis to project along</param>
        /// <returns>Projects circle at pos along axis</returns>
        public static AxisAlignedLine2 ProjectAlongAxis(Circle2 circle, Vector2 pos, Vector2 axis)
        {
            return ProjectAlongAxis(circle.Radius, pos, axis);
        }

        /// <summary>
        /// Projects a circle defined by its radius and the top-left of its bounding box along
        /// the specified axis.
        /// </summary>
        /// <param name="radius">Radius of the circle to project</param>
        /// <param name="pos">Position of the circle</param>
        /// <param name="axis">Axis to project on</param>
        /// <returns></returns>
        public static AxisAlignedLine2 ProjectAlongAxis(float radius, Vector2 pos, Vector2 axis)
        {
            var centerProj = Vector2.Dot(new Vector2(pos.X + radius, pos.Y + radius), axis);

            return new AxisAlignedLine2(axis, centerProj - radius, centerProj + radius);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SharpMath2
{
    /// <summary>
    /// Describes a rectangle. Meant to be reused.
    /// </summary>
    public class Rect2 : Shape2
    {
        /// <summary>
        /// The vertices of this rectangle as a clockwise array.
        /// </summary>
        public readonly Vector2[] Vertices;

        /// <summary>
        /// The corner with the smallest x and y coordinates on this
        /// rectangle.
        /// </summary>
        public Vector2 Min => Vertices[0];

        /// <summary>
        /// The corner with the largest x and y coordinates on this
        /// rectangle
        /// </summary>
        public Vector2 Max => Vertices[2];

        /// <summary>
        /// The corner with the largest x and smallest y coordinates on
        /// this rectangle
        /// </summary>
        public Vector2 UpperRight => Vertices[1];

        /// <summary>
        /// The corner with the smallest x and largest y coordinates on this
        /// rectangle
        /// </summary>
        public Vector2 LowerLeft => Vertices[3];

        /// <summary>
        /// The center of this rectangle
        /// </summary>
        public readonly Vector2 Center;

        /// <summary>
        /// The width of this rectangle
        /// </summary>
        public readonly float Width;

        /// <summary>
        /// The height of this rectangle
        /// </summary>
        public readonly float Height;

        /// <summary>
        /// Creates a bounding box with the specified upper-left and bottom-right.
        /// Will autocorrect if min.X > max.X or min.Y > max.Y
        /// </summary>
        /// <param name="min">Min x, min y</param>
        /// <param name="max">Max x, max y</param>
        /// <exception cref="ArgumentException">If min and max do not make a box</exception>
        public Rect2(Vector2 min, Vector2 max)
        {
            if (Math2.Approximately(min, max))
                throw new ArgumentException($"Min is approximately max: min={min}, max={max} - tha'ts a point, not a box");
            if (Math.Abs(min.X - max.X) <= Math2.DEFAULT_EPSILON)
                throw new ArgumentException($"Min x is approximately max x: min={min}, max={max} - that's a line, not a box");
            if (Math.Abs(min.Y - max.Y) <= Math2.DEFAULT_EPSILON)
                throw new ArgumentException($"Min y is approximately max y: min={min}, max={max} - that's a line, not a box");

            float tmpX1 = min.X, tmpX2 = max.X;
            float tmpY1 = min.Y, tmpY2 = max.Y;

            min.X = Math.Min(tmpX1, tmpX2);
            min.Y = Math.Min(tmpY1, tmpY2);
            max.X = Math.Max(tmpX1, tmpX2);
            max.Y = Math.Max(tmpY1, tmpY2);

            Vertices = new Vector2[]
            {
                min, new Vector2(max.X, min.Y), max, new Vector2(min.X, max.Y)
            };

            Center = new Vector2((Min.X + Max.X) / 2, (Min.Y + Max.Y) / 2);

            Width = Max.X - Min.X;
            Height = Max.Y - Min.Y;
        }

        /// <summary>
        /// Creates a bounding box from the specified points. Will correct if minX > maxX or minY > maxY.
        /// </summary>
        /// <param name="minX">Min or max x (different from maxX)</param>
        /// <param name="minY">Min or max y (different from maxY)</param>
        /// <param name="maxX">Min or max x (different from minX)</param>
        /// <param name="maxY">Min or max y (different from minY)</param>
        public Rect2(float minX, float minY, float maxX, float maxY) : this(new Vector2(minX, minY), new Vector2(maxX, maxY))
        {
        }

        /// <summary>
        /// Determines if box1 with origin pos1 intersects box2 with origin pos2.
        /// </summary>
        /// <param name="box1">Box 1</param>
        /// <param name="box2">Box 2</param>
        /// <param name="pos1">Origin of box 1</param>
        /// <param name="pos2">Origin of box 2</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <returns>If box1 intersects box2 when box1 is at pos1 and box2 is at pos2</returns>
        public static bool Intersects(Rect2 box1, Rect2 box2, Vector2 pos1, Vector2 pos2, bool strict)
        {
            return AxisAlignedLine2.Intersects(box1.Min.X + pos1.X, box1.Max.X + pos1.X, box2.Min.X + pos2.X, box2.Max.X + pos2.X, strict, false)
                && AxisAlignedLine2.Intersects(box1.Min.Y + pos1.Y, box1.Max.Y + pos1.Y, box2.Min.Y + pos2.Y, box2.Max.Y + pos2.Y, strict, false);
        }

        /// <summary>
        /// Determines if the box when at pos contains point.
        /// </summary>
        /// <param name="box">The box</param>
        /// <param name="pos">Origin of box</param>
        /// <param name="point">Point to check</param>
        /// <param name="strict">true if the edges do not count</param>
        /// <returns>If the box at pos contains point</returns>
        public static bool Contains(Rect2 box, Vector2 pos, Vector2 point, bool strict)
        {
            return AxisAlignedLine2.Contains(box.Min.X + pos.X, box.Max.X + pos.X, point.X, strict, false)
                && AxisAlignedLine2.Contains(box.Min.Y + pos.Y, box.Max.Y + pos.Y, point.Y, strict, false);
        }

        /// <summary>
        /// Determines if innerBox is contained entirely in outerBox
        /// </summary>
        /// <param name="outerBox">the (bigger) box that you want to check contains the inner box</param>
        /// <param name="innerBox">the (smaller) box that you want to check is contained in the outer box</param>
        /// <param name="posOuter">where the outer box is located</param>
        /// <param name="posInner">where the inner box is located</param>
        /// <param name="strict">true to return false if innerBox touches an edge of outerBox, false otherwise</param>
        /// <returns>true if innerBox is contained in outerBox, false otherwise</returns>
        public static bool Contains(Rect2 outerBox, Rect2 innerBox, Vector2 posOuter, Vector2 posInner, bool strict)
        {
            return Contains(outerBox, posOuter, innerBox.Min + posInner, strict) && Contains(outerBox, posOuter, innerBox.Max + posInner, strict);
        }

        /// <summary>
        /// Deterimines in the box contains the specified polygon
        /// </summary>
        /// <param name="box">The box</param>
        /// <param name="poly">The polygon</param>
        /// <param name="boxPos">Where the box is located</param>
        /// <param name="polyPos">Where the polygon is located</param>
        /// <param name="strict">true if we return false if the any part of the polygon is on the edge, false otherwise</param>
        /// <returns>true if the poly is contained in box, false otherwise</returns>
        public static bool Contains(Rect2 box, Polygon2 poly, Vector2 boxPos, Vector2 polyPos, bool strict)
        {
            return Contains(box, poly.AABB, boxPos, polyPos, strict);
        }

        /// <summary>
        /// Projects the rectangle at pos along axis.
        /// </summary>
        /// <param name="rect">The rectangle to project</param>
        /// <param name="pos">The origin of the rectangle</param>
        /// <param name="axis">The axis to project on</param>
        /// <returns>The projection of rect at pos along axis</returns>
        public static AxisAlignedLine2 ProjectAlongAxis(Rect2 rect, Vector2 pos, Vector2 axis)
        {
            //return ProjectAlongAxis(axis, pos, Rotation2.Zero, rect.Center, rect.Min, rect.UpperRight, rect.LowerLeft, rect.Max);
            return ProjectAlongAxis(axis, pos, rect.Vertices);
        }
    }
}

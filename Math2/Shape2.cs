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
        /// Determines if polygon at position 1 intersects the rectangle at position 2.
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <param name="rect">Rectangle</param>
        /// <param name="pos1">Origin of polygon</param>
        /// <param name="pos2">Origin of rectangle</param>
        /// <param name="strict">If overlapping is required for intersection</param>
        /// <returns>if poly at pos1 intersects rect at pos2</returns>
        public static bool Intersects(Polygon2 poly, Rect2 rect, Vector2 pos1, Vector2 pos2, bool strict)
        {
            bool checkedX = false, checkedY = false;
            for(int i = 0; i < poly.Normals.Count; i++)
            {
                if (!IntersectsAlongAxis(poly, rect, pos1, pos2, strict, poly.Normals[i]))
                    return false;

                if (poly.Normals[i].X == 0)
                    checkedY = true;
                if (poly.Normals[i].Y == 0)
                    checkedX = true;
            }

            if (!checkedX && !IntersectsAlongAxis(poly, rect, pos1, pos2, strict, Vector2.UnitX))
                return false;
            if (!checkedY && !IntersectsAlongAxis(poly, rect, pos1, pos2, strict, Vector2.UnitY))
                return false;

            return true;
        }

        /// <summary>
        /// Determines the vector, if any, to move poly at pos1 to prevent intersection of rect
        /// at pos2.
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <param name="rect">Rectangle</param>
        /// <param name="pos1">Origin of polygon</param>
        /// <param name="pos2">Origin of rectangle</param>
        /// <returns>The vector to move pos1 by or null</returns>
        public static Vector2? IntersectMTV(Polygon2 poly, Rect2 rect, Vector2 pos1, Vector2 pos2)
        {
            bool checkedX = false, checkedY = false;

            Vector2 bestAxis = Vector2.Zero;
            float bestMagn = float.MaxValue;

            for(int i = 0; i < poly.Normals.Count; i++)
            {
                var mtv = IntersectMTVAlongAxis(poly, rect, pos1, pos2, poly.Normals[i]);
                if (!mtv.HasValue)
                    return null;

                if(mtv.Value < bestMagn)
                {
                    bestAxis = poly.Normals[i];
                    bestMagn = mtv.Value;
                }

                if (poly.Normals[i].X == 0)
                    checkedY = true;
                if (poly.Normals[i].Y == 0)
                    checkedX = true;
            }

            if(!checkedX)
            {
                var mtv = IntersectMTVAlongAxis(poly, rect, pos1, pos2, Vector2.UnitX);
                if (!mtv.HasValue)
                    return null;
                
                if(mtv.Value < bestMagn)
                {
                    bestAxis = Vector2.UnitX;
                    bestMagn = mtv.Value;
                }
            }

            if(!checkedY)
            {
                var mtv = IntersectMTVAlongAxis(poly, rect, pos1, pos2, Vector2.UnitY);
                if (!mtv.HasValue)
                    return null;

                if(mtv.Value < bestMagn)
                {
                    bestAxis = Vector2.UnitY;
                    bestMagn = mtv.Value;
                }
            }

            return bestAxis * bestMagn;
        }

        /// <summary>
        /// Determines the vector to move pos1 to get rect not to intersect poly.
        /// </summary>
        /// <param name="rect">The rectangle</param>
        /// <param name="poly">The polygon</param>
        /// <param name="pos1">Origin of rectangle</param>
        /// <param name="pos2">Origin of </param>
        /// <returns>Offset of pos1 to get rect not to intersect poly</returns>
        public static Vector2? IntersectMTV(Rect2 rect, Polygon2 poly, Vector2 pos1, Vector2 pos2)
        {
            var res = IntersectMTV(poly, rect, pos2, pos1);
            return res.HasValue ? -res.Value : res; 
        }

        /// <summary>
        /// Determines if the rectangle at pos1 intersects the polygon at pos2.
        /// </summary>
        /// <param name="rect">The rectangle</param>
        /// <param name="poly">The polygon</param>
        /// <param name="pos1">Origin of retangle</param>
        /// <param name="pos2">Origin of polygon</param>
        /// <param name="spolyct">If overlap is required for intersection</param>
        /// <returns>If rect at pos1 intersects poly at pos2</returns>
        public static bool Intersects(Rect2 rect, Polygon2 poly, Vector2 pos1, Vector2 pos2, bool strict)
        {
            return Intersects(poly, rect, pos2, pos1, strict);
        }


        /// <summary>
        /// Determines if the specified polygon and rectangle where poly is at pos1 and rect is at pos2 intersect
        /// along the specified axis.
        /// </summary>
        /// <param name="poly">polygon</param>
        /// <param name="rect">Rectangle</param>
        /// <param name="pos1">Origin of polygon</param>
        /// <param name="pos2">Origin of rectangle</param>
        /// <param name="strict">If overlap is required for intersection</param>
        /// <param name="axis">Axis to check</param>
        /// <returns>If poly at pos1 intersects rect at pos2 along axis</returns>
        public static bool IntersectsAlongAxis(Polygon2 poly, Rect2 rect, Vector2 pos1, Vector2 pos2, bool strict, Vector2 axis)
        {
            var proj1 = Polygon2.ProjectAlongAxis(poly, pos1, axis);
            var proj2 = Rect2.ProjectAlongAxis(rect, pos2, axis);

            return AxisAlignedLine2.Intersects(proj1, proj2, strict);
        }
        
        /// <summary>
        /// Determines if the specified rectangle and polygon where rect is at pos1 and poly is at pos2 intersect 
        /// along the specified axis.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="poly"></param>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <param name="strict"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static bool IntersectsAlongAxis(Rect2 rect, Polygon2 poly, Vector2 pos1, Vector2 pos2, bool strict, Vector2 axis)
        {
            return IntersectsAlongAxis(poly, rect, pos2, pos1, strict, axis);
        }

        /// <summary>
        /// Determines the mtv along axis to move poly at pos1 to prevent intersection with rect at pos2.
        /// </summary>
        /// <param name="poly">polygon</param>
        /// <param name="rect">Rectangle</param>
        /// <param name="pos1">Origin of polygon</param>
        /// <param name="pos2">Origin of rectangle</param>
        /// <param name="axis">Axis to check</param>
        /// <returns>Number if poly intersects rect along axis, null otherwise</returns>
        public static float? IntersectMTVAlongAxis(Polygon2 poly, Rect2 rect, Vector2 pos1, Vector2 pos2, Vector2 axis)
        {
            var proj1 = Polygon2.ProjectAlongAxis(poly, pos1, axis);
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
        /// <param name="axis">Axis to check</param>
        /// <returns>Number if rect intersects poly along axis, null otherwise</returns>
        public static float? IntersectMTVAlongAxis(Rect2 rect, Polygon2 poly, Vector2 pos1, Vector2 pos2, Vector2 axis)
        {
            var proj1 = Rect2.ProjectAlongAxis(rect, pos1, axis);
            var proj2 = Polygon2.ProjectAlongAxis(poly, pos2, axis);

            return AxisAlignedLine2.IntersectMTV(proj1, proj2);
        }

        /// <summary>
        /// Projects the polygon from the given points with origin pos along the specified axis.
        /// </summary>
        /// <param name="axis">Axis to project onto</param>
        /// <param name="pos">Origin of polygon</param>
        /// <param name="points">Points of polygon</param>
        /// <returns>Projection of polygon of points at pos along axis</returns>
        protected static AxisAlignedLine2 ProjectAlongAxis(Vector2 axis, Vector2 pos, params Vector2[] points)
        {
            float min = Math2.Dot(points[0].X + pos.X, points[0].Y + pos.Y, axis.X, axis.Y);
            float max = min;

            for (int i = 1; i < points.Length; i++)
            {
                var tmp = Math2.Dot(points[i].X + pos.X, points[i].Y + pos.Y, axis.X, axis.Y);
                min = Math.Min(min, tmp);
                max = Math.Max(max, tmp);
            }

            return new AxisAlignedLine2(axis, min, max);
        }
    }
}

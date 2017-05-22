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
            for(int i = 0; i < poly.Normals.Count; i++)
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
        public static Vector2? IntersectMTV(Polygon2 poly, Rect2 rect, Vector2 pos1, Vector2 pos2, Rotation2 rot1)
        {
            bool checkedX = false, checkedY = false;

            Vector2 bestAxis = Vector2.Zero;
            float bestMagn = float.MaxValue;

            for(int i = 0; i < poly.Normals.Count; i++)
            {
                var norm = Math2.Rotate(poly.Normals[i], Vector2.Zero, rot1);
                var mtv = IntersectMTVAlongAxis(poly, rect, pos1, pos2, rot1, norm);
                if (!mtv.HasValue)
                    return null;

                if(mtv.Value < bestMagn)
                {
                    bestAxis = norm;
                    bestMagn = mtv.Value;
                }

                if (norm.X == 0)
                    checkedY = true;
                if (norm.Y == 0)
                    checkedX = true;
            }

            if(!checkedX)
            {
                var mtv = IntersectMTVAlongAxis(poly, rect, pos1, pos2, rot1, Vector2.UnitX);
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
                var mtv = IntersectMTVAlongAxis(poly, rect, pos1, pos2, rot1, Vector2.UnitY);
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
        /// Determines the vector to move pos1 to get rect not to intersect poly at pos2 rotated
        /// by rot2 radians.
        /// </summary>
        /// <param name="rect">The rectangle</param>
        /// <param name="poly">The polygon</param>
        /// <param name="pos1">Origin of rectangle</param>
        /// <param name="pos2">Origin of </param>
        /// <param name="rot2">Rotation of the polygon</param>
        /// <returns>Offset of pos1 to get rect not to intersect poly</returns>
        public static Vector2? IntersectMTV(Rect2 rect, Polygon2 poly, Vector2 pos1, Vector2 pos2, Rotation2 rot2)
        {
            var res = IntersectMTV(poly, rect, pos2, pos1, rot2);
            return res.HasValue ? -res.Value : res; 
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
        public static Vector2? IntersectMTV(Polygon2 poly, Rect2 rect, Vector2 pos1, Vector2 pos2)
        {
            return IntersectMTV(poly, rect, pos1, pos2, Rotation2.Zero);
        }

        public static Vector2? IntersectMTV(Rect2 rect, Polygon2 poly, Vector2 pos1, Vector2 pos2)
        {
            return IntersectMTV(rect, poly, pos1, pos2, Rotation2.Zero);
        }
        #endregion
    }
}

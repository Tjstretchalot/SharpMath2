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
            foreach(var vert in Vertices)
            {
                Center += vert;
            }
            Center *= (1.0f / Vertices.Length);
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
        /// <param name="poly1">polygon 1</param>
        /// <param name="poly2">polygon 2</param>
        /// <param name="pos1">polygon 1 origin</param>
        /// <param name="pos2">polygon 2 origin</param>
        /// <returns>MTV for polygon 1</returns>
        public static Vector2? IntersectMTV(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2, Rotation2 rot1, Rotation2 rot2)
        {
            Vector2? bestAxis = null;
            float? bestMagn = null;

            foreach (var norm in poly1.Normals.Select((v) => Tuple.Create(v, rot1)).Union(poly2.Normals.Select((v) => Tuple.Create(v, rot2))))
            {
                var axis = Math2.Rotate(norm.Item1, Vector2.Zero, norm.Item2);
                var mtv = IntersectMTVAlongAxis(poly1, poly2, pos1, pos2, rot1, rot2, axis);
                if (!mtv.HasValue)
                    return null;
                else if (!bestAxis.HasValue || Math.Abs(mtv.Value) < Math.Abs(bestMagn.Value))
                {
                    bestAxis = axis;
                    bestMagn = mtv;
                }
            }

            return bestAxis.Value * bestMagn.Value;
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
		/// Returns null if pt is contained in the polygon.
		/// </summary>
		/// <returns>The distance form poly to pt.</returns>
        /// <param name="poly">The polygon</param>
		/// <param name="pos">Origin of the polygon</param>
        /// <param name="rot">Rotation of the polygon</param>
		/// <param name="pt">Point to check.</param>
		public static Tuple<Vector2, float> MinDistance(Polygon2 poly, Vector2 pos, Rotation2 rot, Vector2 pt)
		{
			float? res = null;
			Vector2 axis = Vector2.Zero;
			foreach(var normUnrot in poly.Normals) 
			{
                var norm = Math2.Rotate(normUnrot, Vector2.Zero, rot);
				var proj = ProjectAlongAxis(poly, pos, rot, norm);
				var ptProj = Vector2.Dot(pos, norm);

				var distTo = AxisAlignedLine2.MinDistance(proj, ptProj);
				if (!distTo.HasValue)
					return null;

				if (!res.HasValue || distTo.Value < res.Value)
				{
					res = distTo;
					axis = norm;
				}
			}

			return Tuple.Create(axis, res.Value);
		}

		/// <summary>
		/// Calculates the shortest distance and direction to go from pos1 to pos2. Returns null
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
			float? res = null;
			Vector2 axis = Vector2.Zero;

			foreach(var norm in poly1.Normals.Select((v) => Tuple.Create(v, rot1)).Union(poly2.Normals.Select((v) => Tuple.Create(v, rot2))))
			{
                var newAxis = Math2.Rotate(norm.Item1, Vector2.Zero, norm.Item2);
				var proj1 = ProjectAlongAxis(poly1, pos1, rot1, newAxis);
				var proj2 = ProjectAlongAxis(poly2, pos2, rot2, newAxis);

				var distTo = AxisAlignedLine2.MinDistance(proj1, proj2);
				if (!distTo.HasValue)
					return null;

				if(!res.HasValue || distTo.Value < res.Value)
				{
					res = distTo;
					axis = newAxis;
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
        public static Vector2? IntersectMTV(Polygon2 poly1, Polygon2 poly2, Vector2 pos1, Vector2 pos2)
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

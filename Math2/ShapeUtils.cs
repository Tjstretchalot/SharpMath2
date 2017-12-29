using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SharpMath2
{
    /// <summary>
    /// A simple shape utils class.
    /// </summary>
    public class ShapeUtils
    {
        /// <summary>
        /// A dictionary containing the circle shapes.
        /// </summary>
        private static Dictionary<Tuple<float, float, float, float>, Polygon2> CircleCache = new Dictionary<Tuple<float, float, float, float>, Polygon2>();

        /// <summary>
        /// A dictionary containing the rectangle shapes.
        /// </summary>
        private static Dictionary<Tuple<float, float, float, float>, Polygon2> RectangleCache = new Dictionary<Tuple<float, float, float, float>, Polygon2>();

        /// <summary>
        /// A dictionary containing the convex polygon shapes.
        /// </summary>
        private static Dictionary<int, Polygon2> ConvexPolygonCache = new Dictionary<int, Polygon2>();

        /// <summary>
        /// Creates a convex polygon from the given texture.
        /// </summary>
        /// <param name="Texture"></param>
        public static Polygon2 CreateConvexPolygon(Texture2D Texture)
        {
            var Key = Texture.GetHashCode();

            if (ConvexPolygonCache.ContainsKey(Key))
                return ConvexPolygonCache[Key];

            var data = new uint[Texture.Width * Texture.Height];
            Texture.GetData<uint>(data);

            var Points = new List<Vector2>();

            for (var i = 0; i < Texture.Width; i++)
            for (var j = 0; j < Texture.Height; j++)
                if (data[j * Texture.Width + i] != 0)
                    Points.Add(new Vector2(i, j));

            if (Points == null)
                return null;

            if (Points.Count <= 2)
                throw new Exception("Can not create a convex hull from a line.");

            int n = Points.Count, k = 0;
            List<Vector2> H = new List<Vector2>(new Vector2[2 * n]);

            Points.Sort((a, b) => a.X == b.X ? a.Y.CompareTo(b.Y) : (a.X > b.X ? 1 : -1));

            for (int i = 0; i < n; ++i)
            {
                while (k >= 2 && cross(H[k - 2], H[k - 1], Points[i]) <= 0)
                    k--;
                H[k++] = Points[i];
            }

            for (int i = n - 2, t = k + 1; i >= 0; i--)
            {
                while (k >= t && cross(H[k - 2], H[k - 1], Points[i]) <= 0)
                    k--;
                H[k++] = Points[i];
            }

            Points = H.Take(k - 1).ToList();


            return ConvexPolygonCache[Key] = new Polygon2(Points.ToArray());
        }
        /// <summary>
        /// Returns the cross product of the given three vectors.
        /// </summary>
        /// <param name="v1">Vector 1.</param>
        /// <param name="v2">Vector 2.</param>
        /// <param name="v3">Vector 3.</param>
        /// <returns></returns>
        private static double cross(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            return (v2.X - v1.X) * (v3.Y - v1.Y) - (v2.Y - v1.Y) * (v3.X - v1.X);
        }


        /// <summary>
        /// Creates a new rectangle shape with the given width, height, x and y center.
        /// </summary>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <param name="x">The X center of the rectangle.</param>
        /// <param name="y">The Y center of the rectangle.</param>
        /// <returns>A rectangle shape with the given width, height, x and y center.</returns>
        public static Polygon2 CreateRectangle(float width, float height, float x = 0, float y = 0)
        {
            var Key = new Tuple<float, float, float, float>(width, height, x, y);

            if (RectangleCache.ContainsKey(Key))
                return RectangleCache[Key];

            return RectangleCache[Key] = new Polygon2(new [] {
                 new Vector2(x, y),
                 new Vector2(x + width, y),
                 new Vector2(x + width, y + height),
                 new Vector2(x, y + height)
            });
        }

        /// <summary>
        /// Creates a new circle shape with the given radius, center, and segments.
        /// </summary>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="x">The X center of the circle.</param>
        /// <param name="y">The Y center of the circle.</param>
        /// <param name="segments">The amount of segments (more segments equals higher detailed circle)</param>
        /// <returns>A circle with the given radius, center, and segments, as a polygon2 shape.</returns>
        public static Polygon2 CreateCircle(float radius, float x = 0, float y = 0, int segments = 32)
        {
            var Key = new Tuple<float, float, float, float>(radius, x, y, segments);

            if (CircleCache.ContainsKey(Key))
                return CircleCache[Key];

            var Center = new Vector2(radius + x, radius + y);
            var increment = (Math.PI * 2.0) / segments;
            var theta = 0.0;
            var verts = new List<Vector2>(segments);

            for (var i = 0; i < segments; i++)
            {
                verts.Add(
                    Center + radius * new Vector2(
                        (float) Math.Cos(theta),
                        (float) Math.Sin(theta)
                    )
                );
                theta += increment;
            }

            return CircleCache[Key] = new Polygon2(verts.ToArray());
        }
    }
}

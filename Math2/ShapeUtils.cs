using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SharpMath2
{
    /// <summary>
    /// Contains a collection of functions that generate particular shapes
    /// </summary>
    public class ShapeUtils
    public class ShapeUtils
    {
        /// <summary>
        /// Creates a polygon version of a rectangle. Useful when you have something you don't want to
        /// lock into a rectangle forever.
        /// </summary>
        /// <param name="width">The width of the rectangle</param>
        /// <param name="height">The height of the rectangle</param>
        /// <param name="x">Minimum x of the rectangle</param>
        /// <param name="y">Minimum y of the rectangle</param>
        /// <returns>the polygon that looks like a rectangle</returns>
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
        /// Approximates a circle to a polygon
        /// </summary>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="x">Minimum x coordinate on the circle</param>
        /// <param name="y">Minimum y coordinate on the circle</param>
        /// <param name="segments">Number of segments to approximate with</param>
        /// <returns>The polygon approximating the specified circle</returns>
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

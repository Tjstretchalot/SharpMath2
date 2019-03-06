using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SharpMath2
{
    /// <summary>
    /// Contains a collection of functions that generate particular shapes
    /// </summary>
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
            return new Polygon2(new Vector2[] {
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
            Vector2 center = new Vector2(radius + x, radius + y);

            double increment = Math.PI * 2.0 / segments;
            double theta = 0.0;

            var verts = new List<Vector2>(segments);
            for (int i = 0; i < segments; i++)
            {
                Vector2 v = center + radius * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
                verts.Add(v);
                theta += increment;
            }

            return new Polygon2(verts.ToArray());
        }
    }
}

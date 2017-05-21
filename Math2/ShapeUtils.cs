using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SharpMath2
{
	public class ShapeUtils
	{
		public static Polygon2 CreateRectangle(float width, float height, float x = 0, float y = 0)
		{
			return new Polygon2(new Vector2[] {
				 new Vector2(x, y),
				 new Vector2(x + width, y),
				 new Vector2(x + width, y + height),
				 new Vector2(x, y + height)
			});
		}

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

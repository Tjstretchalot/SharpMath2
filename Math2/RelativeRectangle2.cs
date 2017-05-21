using System;
using Microsoft.Xna.Framework;

namespace SharpMath2
{
	public class RelativeRectangle2 : Rect2
	{
		public RelativeRectangle2(Vector2 min, Vector2 max) : base(min, max)
		{
		} 

		public RelativeRectangle2(float x, float y, float w, float h) : base(new Vector2(x, y), new Vector2(x + w, y + h))
		{
		}

		public Rect2 ToRect(Rect2 original) {
			return new Rect2(original.Min * Min, original.Max * Max);
		}
		 
		public Rect2 ToRect(Rectangle original) {
			return new Rect2(
					new Vector2(original.Left * Min.X, original.Top * Min.Y),
					new Vector2(original.Right * Max.X, original.Bottom * Max.Y)
			);
		}
	}
}

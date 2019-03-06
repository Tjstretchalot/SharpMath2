using System;
using Microsoft.Xna.Framework;

namespace SharpMath2
{
    /// <summary>
    /// Describes a rectangle that is describing the percentages to go
    /// of the true rectangle. Useful in some UI circumstances.
    /// </summary>
	public class RelativeRectangle2 : Rect2
    {
        /// <summary>
        /// Create a new relative rectangle
        /// </summary>
        /// <param name="min">vector of smallest x and y coordinates</param>
        /// <param name="max">vector of largest x and y coordinates</param>
        public RelativeRectangle2(Vector2 min, Vector2 max) : base(min, max)
        {
        }

        /// <summary>
        /// Create a new relative rectangle
        /// </summary>
        /// <param name="x">smallest x</param>
        /// <param name="y">smallest y</param>
        /// <param name="w">width</param>
        /// <param name="h">height</param>
        public RelativeRectangle2(float x, float y, float w, float h) : base(new Vector2(x, y), new Vector2(x + w, y + h))
        {
        }

        /// <summary>
        /// Multiply our min with original min and our max with original max and return
        /// as a rect
        /// </summary>
        /// <param name="original">the original</param>
        /// <returns>scaled rect</returns>
        public Rect2 ToRect(Rect2 original)
        {
            return new Rect2(original.Min * Min, original.Max * Max);
        }

#if !NOT_MONOGAME
        /// <summary>
        /// Multiply our min with original min and our max with original max and return
        /// as a rect
        /// </summary>
        /// <param name="original">the monogame original</param>
        /// <returns>the rect</returns>
        public Rect2 ToRect(Rectangle original) {
			return new Rect2(
				new Vector2(original.Left + original.Width * Min.X, original.Top  + original.Height * Min.Y),
					new Vector2(original.Left + original.Width * Max.X, original.Top + original.Height * Max.Y)
			);
		}
#endif
    }
}

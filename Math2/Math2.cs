using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SharpMath2
{
	public class Math2
	{
		/// <summary>
		/// Default epsilon
		/// </summary>
		public const float DEFAULT_EPSILON = 0.001f;

		/// <summary>
		/// Determines if v1, v2, and v3 are collinear
		/// </summary>
		/// <remarks>
		/// Calculates if the area of the triangle of v1, v2, v3 is less than or equal to epsilon.
		/// </remarks>
		/// <param name="v1">Vector 1</param>
		/// <param name="v2">Vector 2</param>
		/// <param name="v3">Vector 3</param>
		/// <param name="epsilon">How close is close enough</param>
		/// <returns>If v1, v2, v3 is collinear</returns>
		public static bool IsOnLine(Vector2 v1, Vector2 v2, Vector2 v3, float epsilon = DEFAULT_EPSILON)
		{
			return Math.Abs(v1.X * (v2.Y - v3.Y) + v2.X * (v3.Y - v1.Y) + v3.X * (v1.Y - v2.Y)) <= epsilon;
		}

		/// <summary>
		/// Determines if f1 and f2 are approximately the same.
		/// </summary>
		/// <returns>The approximately.</returns>
		/// <param name="f1">F1.</param>
		/// <param name="f2">F2.</param>
		/// <param name="epsilon">Epsilon.</param>
		public static bool Approximately(float f1, float f2, float epsilon = DEFAULT_EPSILON)
		{
			return Math.Abs(f1 - f2) <= epsilon;
		}

		/// <summary>
		/// Determines if vectors v1 and v2 are approximately equal, such that
		/// both coordinates are within epsilon.
		/// </summary>
		/// <returns>If v1 and v2 are approximately equal.</returns>
		/// <param name="v1">V1.</param>
		/// <param name="v2">V2.</param>
		/// <param name="epsilon">Epsilon.</param>
		public static bool Approximately(Vector2 v1, Vector2 v2, float epsilon = DEFAULT_EPSILON)
		{
			return Approximately(v1.X, v2.X, epsilon) && Approximately(v1.Y, v2.Y, epsilon);
		}
	}
}

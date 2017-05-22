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
		/// Finds a vector that is perpendicular to the specified vector.
		/// </summary>
		/// <returns>A vector perpendicular to v</returns>
		/// <param name="v">Vector</param>
		public static Vector2 Perpendicular(Vector2 v)
		{
			return new Vector2(-v.Y, v.X);
		}

		/// <summary>
		/// Finds the dot product of (x1, y1) and (x2, y2)
		/// </summary>
		/// <returns>The dot.</returns>
		/// <param name="x1">The first x value.</param>
		/// <param name="y1">The first y value.</param>
		/// <param name="x2">The second x value.</param>
		/// <param name="y2">The second y value.</param>
		public static float Dot(float x1, float y1, float x2, float y2)
		{
			return x1 * x2 + y1 * y2;
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

        /// <summary>
        /// Rotates the specified vector about the specified vector a rotation of the
        /// specified amount.
        /// </summary>
        /// <param name="vec">The vector to rotate</param>
        /// <param name="about">The point to rotate vec around</param>
        /// <param name="rotation">The rotation</param>
        /// <returns>The vector vec rotated about about rotation.Theta radians.</returns>
        public static Vector2 Rotate(Vector2 vec, Vector2 about, Rotation2 rotation)
        {
            var tmp = vec - about;
            return new Vector2(tmp.X * rotation.CosTheta - tmp.Y * rotation.SinTheta,
                               tmp.X * rotation.SinTheta + tmp.Y * rotation.CosTheta);
        }
	}
}

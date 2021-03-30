using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SharpMath2
{
    /// <summary>
    /// Contains utility functions for doing math in two-dimensions that
    /// don't fit elsewhere. Also contains any necessary constants.
    /// </summary>
    public class Math2
    {
        /// <summary>
        /// Default epsilon
        /// </summary>
        public const float DEFAULT_EPSILON = 0.001f;

        /// <summary>
        /// Determines if v1, v2, and v3 are collinear
        /// </summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <param name="v3">Vector 3</param>
        /// <param name="epsilon">How close is close enough</param>
        /// <returns>If v1, v2, v3 is collinear</returns>
        public static bool IsOnLine(Vector2 v1, Vector2 v2, Vector2 v3, float epsilon = DEFAULT_EPSILON)
        {
            var fromV1ToV2 = v2 - v1;
            var axis = Vector2.Normalize(fromV1ToV2);
            var normal = Perpendicular(axis);

            var fromV1ToV3 = v3 - v1;
            var normalPortion = Dot(fromV1ToV3, normal);

            return Approximately(normalPortion, 0, epsilon);
        }

        /// <summary>
        /// Determines if the given pt is between the line between v1 and v2.
        /// </summary>
        /// <param name="v1">The first edge of the line</param>
        /// <param name="v2">The second edge of the line</param>
        /// <param name="pt">The point to test</param>
        /// <param name="epsilon">How close is close enough (not exactly distance)</param>
        /// <returns>True if pt is on the line between v1 and v2, false otherwise</returns>
        public static bool IsBetweenLine(Vector2 v1, Vector2 v2, Vector2 pt, float epsilon = DEFAULT_EPSILON)
        {
            var fromV1ToV2 = v2 - v1;
            var axis = Vector2.Normalize(fromV1ToV2);
            var normal = Perpendicular(axis);

            var fromV1ToPt = pt - v1;
            var normalPortion = Dot(fromV1ToPt, normal);

            if (!Approximately(normalPortion, 0, epsilon))
                return false; // not on the infinite line

            var axisPortion = Dot(fromV1ToPt, axis);

            if (axisPortion < -epsilon)
                return false; // left of the first point

            if (axisPortion > fromV1ToV2.Length() + epsilon)
                return false; // right of second point

            return true;
        }

        /// <summary>
        /// Computes the triple cross product (A X B) X A
        /// </summary>
        /// <param name="a">First vector</param>
        /// <param name="b">Second vector</param>
        /// <returns>
        /// Result of projecting to 3 dimensions, performing the
        /// triple cross product, and then projecting back down to 2 dimensions.
        /// </returns>
        public static Vector2 TripleCross(Vector2 a, Vector2 b)
        {
            return new Vector2(
                -a.X * a.Y * b.Y + a.Y * a.Y * b.X,
                a.X * a.X * b.Y - a.X * a.Y * b.X
            );
        }

        /// <summary>
        /// Calculates the square of the area of the triangle made up of the specified points.
        /// </summary>
        /// <param name="v1">First point</param>
        /// <param name="v2">Second point</param>
        /// <param name="v3">Third point</param>
        /// <returns>Area of the triangle made up of the given 3 points</returns>
        public static float AreaOfTriangle(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            return 0.5f * Math.Abs((v2.X - v1.X) * (v3.Y - v1.Y) - (v3.X - v1.X) * (v2.Y - v1.Y));
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
        /// Finds the dot product of the two vectors
        /// </summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector</param>
        /// <returns>The dot product between v1 and v2</returns>
        public static float Dot(Vector2 v1, Vector2 v2)
        {
            return Dot(v1.X, v1.Y, v2.X, v2.Y);
        }

        /// <summary>
        /// Finds the dot product of two vectors, where one is specified
        /// by its components
        /// </summary>
        /// <param name="v">The first vector</param>
        /// <param name="x2">The x-value of the second vector</param>
        /// <param name="y2">The y-value of the second vector</param>
        /// <returns>The dot product of v and (x2, y2)</returns>
        public static float Dot(Vector2 v, float x2, float y2)
        {
            return Dot(v.X, v.Y, x2, y2);
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
            if (rotation.Theta == 0)
                return vec;
            var tmp = vec - about;
            return new Vector2(tmp.X * rotation.CosTheta - tmp.Y * rotation.SinTheta + about.X,
                               tmp.X * rotation.SinTheta + tmp.Y * rotation.CosTheta + about.Y);
        }

        /// <summary>
        /// Returns either the vector or -vector such that MakeStandardNormal(vec) == MakeStandardNormal(-vec)
        /// </summary>
        /// <param name="vec">The vector</param>
        /// <returns>Normal such that vec.X is positive (unless vec.X is 0, in which such that vec.Y is positive)</returns>
        public static Vector2 MakeStandardNormal(Vector2 vec)
        {
            if (vec.X < -DEFAULT_EPSILON)
                return -vec;

            if (Approximately(vec.X, 0) && vec.Y < 0)
                return -vec;

            return vec;
        }
    }
}

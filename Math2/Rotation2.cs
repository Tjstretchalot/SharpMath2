using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMath2
{
    /// <summary>
    /// Describes a rotation about the z axis, with sin and cos of theta
    /// cached.
    /// </summary>
    public struct Rotation2
    {
        /// <summary>
        /// Rotation Theta=0
        /// </summary>
        public static readonly Rotation2 Zero = new Rotation2(0, 1, 0);

        /// <summary>
        /// Theta in radians.
        /// </summary>
        public readonly float Theta;

        /// <summary>
        /// Math.Cos(Theta)
        /// </summary>
        public readonly float CosTheta;

        /// <summary>
        /// Math.Sin(Theta)
        /// </summary>
        public readonly float SinTheta;

        public Rotation2(float theta, float cosTheta, float sinTheta)
        {
            Theta = theta;
            CosTheta = cosTheta;
            SinTheta = sinTheta;
        }

        public Rotation2(float theta) : this(theta, (float)Math.Cos(theta), (float)Math.Sin(theta))
        {
        }
    }
}

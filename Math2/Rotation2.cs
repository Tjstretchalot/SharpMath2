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
            if (float.IsInfinity(theta) || float.IsNaN(theta))
                throw new ArgumentException($"Invalid theta: {theta}", nameof(theta));
            if (theta < 0)
            {
                int numToAdd = (int)Math.Ceiling((-theta) / (Math.PI * 2));
                theta += (float)Math.PI * 2 * numToAdd;
            }
            else if (theta >= Math.PI * 2)
            {
                int numToReduce = (int)Math.Floor(theta / (Math.PI * 2));
                theta -= (float)Math.PI * 2 * numToReduce;
            }

            Theta = theta;
            CosTheta = cosTheta;
            SinTheta = sinTheta;
        }

        public Rotation2(float theta) : this(theta, (float)Math.Cos(theta), (float)Math.Sin(theta))
        {
        }

        public static bool operator ==(Rotation2 r1, Rotation2 r2)
        {
            if (ReferenceEquals(r1, null) || ReferenceEquals(r2, null))
                return ReferenceEquals(r1, r2);

            return r1.Theta == r2.Theta;
        }

        public static bool operator !=(Rotation2 r1, Rotation2 r2)
        {
            if (ReferenceEquals(r1, null) || ReferenceEquals(r2, null))
                return ReferenceEquals(r1, r2);

            return r1.Theta != r2.Theta;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Rotation2))
                return false;

            return this == ((Rotation2)obj);
        }

        public override int GetHashCode()
        {
            return Theta.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Theta} rads";
        }
    }
}

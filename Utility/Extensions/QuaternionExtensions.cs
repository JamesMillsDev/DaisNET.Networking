using System.Numerics;

namespace DaisNET.Utility.Extensions
{
    /// <summary>
    /// Extension methods for System.Numerics.Quaternion to convert between quaternions and Euler angles.
    /// </summary>
    public static class QuaternionExtensions
    {
        /// <summary>
        /// Converts a quaternion to Euler angles (in radians).
        /// Returns angles in the order: Roll (X), Pitch (Y), Yaw (Z).
        /// </summary>
        /// <param name="quaternion">The quaternion to convert.</param>
        /// <returns>A Vector3 containing the Euler angles in radians (X=Roll, Y=Pitch, Z=Yaw).</returns>
        public static Vector3 ToEulerAngles(this Quaternion quaternion)
        {
            Vector3 angles = new();

            // Roll (X-axis rotation)
            double sinrCosp = 2 * (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z);
            double cosrCosp = 1 - 2 * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y);
            angles.X = (float)Math.Atan2(sinrCosp, cosrCosp);

            // Pitch (Y-axis rotation)
            double sinp = 2 * (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X);
            if (Math.Abs(sinp) >= 1)
            {
                // Use 90 degrees if out of range
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
            }
            else
            {
                angles.Y = (float)Math.Asin(sinp);
            }

            // Yaw (Z-axis rotation)
            double sinyCosp = 2 * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y);
            double cosyCosp = 1 - 2 * (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
            angles.Z = (float)Math.Atan2(sinyCosp, cosyCosp);

            return angles;
        }

        /// <summary>
        /// Converts a quaternion to Euler angles in degrees.
        /// Returns angles in the order: Roll (X), Pitch (Y), Yaw (Z).
        /// </summary>
        /// <param name="quaternion">The quaternion to convert.</param>
        /// <returns>A Vector3 containing the Euler angles in degrees (X=Roll, Y=Pitch, Z=Yaw).</returns>
        public static Vector3 ToEulerAnglesDegrees(this Quaternion quaternion)
        {
            Vector3 radians = quaternion.ToEulerAngles();
            return new Vector3(
                RadiansToDegrees(radians.X),
                RadiansToDegrees(radians.Y),
                RadiansToDegrees(radians.Z)
            );
        }

        /// <summary>
        /// Creates a quaternion from Euler angles (in radians).
        /// Applies rotations in the order: Yaw (Z), Pitch (Y), Roll (X).
        /// </summary>
        /// <param name="eulerAngles">A Vector3 containing Euler angles in radians (X=Roll, Y=Pitch, Z=Yaw).</param>
        /// <returns>A quaternion representing the rotation.</returns>
        public static Quaternion FromEulerAngles(Vector3 eulerAngles)
        {
            return FromEulerAngles(eulerAngles.X, eulerAngles.Y, eulerAngles.Z);
        }

        /// <summary>
        /// Creates a quaternion from Euler angles (in radians).
        /// Applies rotations in the order: Yaw (Z), Pitch (Y), Roll (X).
        /// </summary>
        /// <param name="roll">Rotation around the X-axis in radians.</param>
        /// <param name="pitch">Rotation around the Y-axis in radians.</param>
        /// <param name="yaw">Rotation around the Z-axis in radians.</param>
        /// <returns>A quaternion representing the rotation.</returns>
        public static Quaternion FromEulerAngles(float roll, float pitch, float yaw)
        {
            double cy = Math.Cos(yaw * 0.5);
            double sy = Math.Sin(yaw * 0.5);
            double cp = Math.Cos(pitch * 0.5);
            double sp = Math.Sin(pitch * 0.5);
            double cr = Math.Cos(roll * 0.5);
            double sr = Math.Sin(roll * 0.5);

            Quaternion q = new()
            {
                W = (float)(cr * cp * cy + sr * sp * sy),
                X = (float)(sr * cp * cy - cr * sp * sy),
                Y = (float)(cr * sp * cy + sr * cp * sy),
                Z = (float)(cr * cp * sy - sr * sp * cy)
            };

            return q;
        }

        /// <summary>
        /// Creates a quaternion from Euler angles in degrees.
        /// Applies rotations in the order: Yaw (Z), Pitch (Y), Roll (X).
        /// </summary>
        /// <param name="eulerAngles">A Vector3 containing Euler angles in degrees (X=Roll, Y=Pitch, Z=Yaw).</param>
        /// <returns>A quaternion representing the rotation.</returns>
        public static Quaternion FromEulerAnglesDegrees(Vector3 eulerAngles)
        {
            return FromEulerAngles(
                DegreesToRadians(eulerAngles.X),
                DegreesToRadians(eulerAngles.Y),
                DegreesToRadians(eulerAngles.Z)
            );
        }

        /// <summary>
        /// Creates a quaternion from Euler angles in degrees.
        /// Applies rotations in the order: Yaw (Z), Pitch (Y), Roll (X).
        /// </summary>
        /// <param name="roll">Rotation around the X-axis in degrees.</param>
        /// <param name="pitch">Rotation around the Y-axis in degrees.</param>
        /// <param name="yaw">Rotation around the Z-axis in degrees.</param>
        /// <returns>A quaternion representing the rotation.</returns>
        public static Quaternion FromEulerAnglesDegrees(float roll, float pitch, float yaw)
        {
            return FromEulerAngles(
                DegreesToRadians(roll),
                DegreesToRadians(pitch),
                DegreesToRadians(yaw)
            );
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        private static float DegreesToRadians(float degrees)
        {
            return degrees * (float)Math.PI / 180f;
        }

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        private static float RadiansToDegrees(float radians)
        {
            return radians * 180f / (float)Math.PI;
        }
    }
}
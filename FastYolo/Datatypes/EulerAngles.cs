using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Yaw (Z), pitch (Y) and roll (X) in degrees, used for 3D rotations around the default
	///   coordinate axes (will not limit to 0-360 like <see cref="Rotation2D" />). Details at:
	///   http://en.wikipedia.org/wiki/Euler_angles
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct EulerAngles
	{
		public EulerAngles(float pitch, float yaw, float roll)
		{
			Pitch = pitch;
			Yaw = yaw;
			Roll = roll;
		}

		[Pure] public float Pitch { get; }
		[Pure] public float Yaw { get; }
		[Pure] public float Roll { get; }

		[Pure]
		public static bool operator !=(EulerAngles euler1, EulerAngles euler2)
		{
			return !euler1.Equals(euler2);
		}

		[Pure]
		public static bool operator ==(EulerAngles euler1, EulerAngles euler2)
		{
			return euler1.Equals(euler2);
		}

		[Pure]
		public bool Equals(EulerAngles other)
		{
			return Pitch.IsNearlyEqual(other.Pitch) && Yaw.IsNearlyEqual(other.Yaw) &&
			       Roll.IsNearlyEqual(other.Roll);
		}

		[Pure]
		public override bool Equals(object other)
		{
			return other is EulerAngles ? Equals((EulerAngles) other) : base.Equals(other);
		}

		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				return Pitch.GetHashCode() * 511 + Yaw.GetHashCode() * 111 + Roll.GetHashCode();
			}
		}

		[Pure]
		public override string ToString()
		{
			return "Pitch: " + Pitch + ", Yaw: " + Yaw + ", Roll: " + Roll;
		}
	}
}
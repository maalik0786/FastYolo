using System;

namespace FastYolo.Extensions
{
	/// <summary>
	///   Extends the System.Math class, but uses floats and degrees. Also provides extra constants.
	/// </summary>
	public static class MathExtensions
	{
		// ReSharper disable MethodNameNotMeaningful
		public static float Abs(this float value)
		{
			return Math.Abs(value);
		}

		public static int Round(this float value)
		{
			return (int) Math.Round(value);
		}

		public static float Sin(float degrees)
		{
			return (float) Math.Sin(degrees * Pi / HalfCircleDegrees);
		}

		public const float Pi = 3.14159265359f;
		public const float FullCircleDegrees = 360f;
		public const float HalfCircleDegrees = FullCircleDegrees / 2f;
		public const float QuarterCircleDegrees = FullCircleDegrees / 4f;

		public static float Cos(float degrees)
		{
			return (float) Math.Cos(degrees * Pi / HalfCircleDegrees);
		}

		public static float Tan(float degrees)
		{
			return (float) Math.Tan(degrees * Pi / HalfCircleDegrees);
		}

		public static float Asin(float value)
		{
			return (float) Math.Asin(value) * HalfCircleDegrees / Pi;
		}

		public static float Acos(float value)
		{
			return (float) Math.Acos(value) * HalfCircleDegrees / Pi;
		}

		public static float RadiansToDegrees(this float radians)
		{
			return radians * HalfCircleDegrees / Pi;
		}

		public static float DegreesToRadians(this float degrees)
		{
			return degrees * Pi / HalfCircleDegrees;
		}

		public static float InvSqrt(this float value)
		{
			return 1.0f / Sqrt(value);
		}

		public static float Sqrt(float value)
		{
			return (float) Math.Sqrt(value);
		}

		public static float Atan2(float y, float x)
		{
			return (float) Math.Atan2(y, x) * HalfCircleDegrees / Pi;
		}

		public static float Round(this float value, int decimals)
		{
			return (float) Math.Round(value, decimals);
		}

		public static int GetNearestMultiple(this int value, int multipleValue)
		{
			var min = (int) (value / (float) multipleValue) * multipleValue;
			var max = ((int) (value / (float) multipleValue) + 1) * multipleValue;
			return max - value < value - min ? max : min;
		}

		public static bool IsNearlyEqual(this float value1, float value2, float difference = Epsilon)
		{
			return Math.Abs(value1 - value2) < difference;
		}

		public const float Epsilon = 0.0001f;

		public static bool IsLessOrNearlyEqual(this float value1, float value2,
			float difference = Epsilon)
		{
			return value1 < value2 || value1.IsNearlyEqual(value2, difference);
		}

		public static int Clamp(this int value, int min, int max)
		{
			return value > max ? max : value < min ? min : value;
		}

		public static float Clamp(this float value, float min, float max)
		{
			return value > max ? max : value < min ? min : value;
		}

		public static float Lerp(this float value1, float value2, float percentage)
		{
			return value1 * (1 - percentage) + value2 * percentage;
		}
	}
}
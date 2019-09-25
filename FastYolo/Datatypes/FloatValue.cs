using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Just stores a float value and makes it available as RenderData for Entities.
	/// </summary>
	[DebuggerDisplay("FloatValue({" + nameof(Value) + "})")]
	public struct FloatValue : IEquatable<FloatValue>, LerpWithParent<FloatValue>
	{
		public FloatValue(float value)
		{
			Value = value;
		}

		public FloatValue(string valueAsString)
		{
			Value = valueAsString.Convert<float>();
		}

		[Pure] public float Value { get; }

		[Pure]
		public bool Equals(FloatValue other)
		{
			return Value == other.Value;
		}

		public override bool Equals(object other)
		{
			return other is FloatValue ? Equals((FloatValue) other) : base.Equals(other);
		}

		[Pure]
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		[Pure]
		public FloatValue Lerp(FloatValue other, float interpolation)
		{
			return new FloatValue(Value.Lerp(other.Value, interpolation));
		}

		[Pure]
		public FloatValue Lerp(FloatValue other, float interpolation, FloatValue parent)
		{
			return new FloatValue(Value.Lerp(other.Value, interpolation) + parent.Value);
		}

		[Pure]
		public static FloatValue operator *(FloatValue first, float factor)
		{
			return new FloatValue(first.Value * factor);
		}

		[Pure]
		public override string ToString()
		{
			return Value.ToInvariantString();
		}
	}
}
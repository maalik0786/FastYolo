using System;
using System.Collections.Generic;
using System.Linq;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Mostly used for testing, but also for deterministic values always returning the same sequence.
	///   PseudoRandom can also be used with a certain seed value to always return the same sequence.
	///   All fixedValues must be between 0.0 and less than 1.0.
	/// </summary>
	public class FixedRandom : Randomizer
	{
		private readonly float[] fixedValues;

		private int index;

		public FixedRandom(float[] fixedValues = null)
		{
			if (fixedValues == null || fixedValues.Length == 0)
				fixedValues = new[] {0.7f, 0.35f, 0.18f, 0.894f, 0.575f};
			this.fixedValues = fixedValues.Clone() as float[];
			if (IsAnyFixedValueOutOfRange(fixedValues))
				throw new FixedValueOutOfRange();
		}

		private static bool IsAnyFixedValueOutOfRange(IEnumerable<float> fixedValues)
		{
			return fixedValues != null && fixedValues.Any(value => value < 0.0f || value >= 1.0f);
		}

		// ReSharper disable once MethodNameNotMeaningful
		public override int Get(int min, int max)
		{
			return (int) Get((float) min, max);
		}

		// ReSharper disable once MethodNameNotMeaningful
		public override float Get(float min = 0.0f, float max = 1.0f)
		{
			return fixedValues[index++ % fixedValues.Length] * (max - min) + min;
		}

		public class FixedValueOutOfRange : Exception
		{
		}
	}
}
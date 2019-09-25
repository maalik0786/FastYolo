using System.Collections.Generic;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   The definition for all random number generators. By default PseudoRandom is used.
	/// </summary>
	public abstract class Randomizer
	{
		public Color GetRandomBrightColor()
		{
			return GetRandomColor(128);
		}

		public Color GetRandomColor(byte lowValue = 16, byte highValue = 255)
		{
			var r = (byte) Get(lowValue, highValue + 1);
			var g = (byte) Get(lowValue, highValue + 1);
			var b = (byte) Get(lowValue, highValue + 1);
			return new Color(r, g, b);
		}

		/// <summary>
		///   Returns an integer greater than or equal to min and strictly less than max
		/// </summary>
		// ReSharper disable MethodNameNotMeaningful
		public abstract int Get(int min, int max);

		/// <summary>
		///   Gets a random element of a list or array within its bounds.
		/// </summary>
		public T Get<T>(IList<T> array)
		{
			return array[Get(0, array.Count)];
		}

		/// <summary>
		///   Returns a float between min and max, by default a value between zero and one.
		/// </summary>
		public abstract float Get(float min = 0.0f, float max = 1.0f);

		/// <summary>
		///   Very nice function to calc random value with help of factors, e.g. when calling with
		///   { 0.1f, 0.1f, 2.0f } we will most likly in >99% of all cases get 2 as result and not 0 or 1!
		/// </summary>
		public int GetRandomArrayIndexWithFactors(List<float> factorArray)
		{
			if (factorArray == null || factorArray.Count == 0)
				return 0;
			float allValuesTogether = 0;
			for (var i = 0; i < factorArray.Count; i++)
				allValuesTogether += factorArray[i];
			var value = Get(0.0f, allValuesTogether);
			var currentValue = 0.0f;
			for (var i = 0; i < factorArray.Count; i++)
			{
				if (value >= currentValue &&
				    value < currentValue + factorArray[i])
					return i;
				currentValue += factorArray[i];
			}

			return 0; //ncrunch: no coverage
		}
	}
}
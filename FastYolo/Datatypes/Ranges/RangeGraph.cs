using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using FastYolo.Extensions;

namespace FastYolo.Datatypes.Ranges
{
	public class RangeGraph<T> : BaseRangeGraph<T> where T : Lerp<T>
	{
		public RangeGraph(T value) : this(new[] {value})
		{
		}

		public RangeGraph(T[] values) : base(values)
		{
		}

		public RangeGraph(List<T> values) : base(values)
		{
		}

		public RangeGraph(string stringRangeGraph)
		{
			var partitions = stringRangeGraph.SplitAndTrim('{', '}');
			if (partitions.Length < 5 || (partitions.Length - 1) % 2 != 0)
				throw new InvalidNumberOfDatatypeComponents<RangeGraph<T>>();
			Values = new T[partitions.Length / 2];
			try
			{
				TryCreateInstance(partitions);
			}
			catch (Exception)
			{
				throw new TypeInStringNotEqualToInitializedType();
			}
		}

		public RangeGraph(T minimum, T maximum) : base(minimum, maximum)
		{
		}

		private void TryCreateInstance(string[] partitions)
		{
			for (var i = 1; i < partitions.Length; i += 2)
				Values[i / 2] = (T) Activator.CreateInstance(typeof(T), partitions[i]);
		}

		[Pure]
		public override T GetInterpolatedValue(float interpolation)
		{
			if (Values.Length == 1)
				return Values[0];
			if (interpolation >= 1.0f)
				return Values[Values.Length - 1];
			var intervalLeft = (int) (interpolation * (Values.Length - 1));
			var intervalInterpolation = interpolation * (Values.Length - 1) - intervalLeft;
			return Values[intervalLeft].Lerp(Values[intervalLeft + 1], intervalInterpolation);
		}

		public override void SetValue(int index, T value)
		{
			if (index >= Values.Length)
				AddValueAfter(Values.Length, value);
			else if (index < 0)
				AddValueBefore(0, value);
			else
				Values[index] = value;
		}

		public void AddValueAfter(int leftIndex, T value)
		{
			if (leftIndex < 0)
				AddValueBefore(0, value);
			else
				ExpandAndAddValue(leftIndex >= Values.Length ? Values.Length : leftIndex + 1, value);
		}

		public void AddValueBefore(int rightIndex, T value)
		{
			if (rightIndex >= Values.Length)
				AddValueAfter(Values.Length, value);
			else
				ExpandAndAddValue(rightIndex < 0 ? 0 : rightIndex, value);
		}

		[Pure]
		public override string ToString()
		{
			var stringOfValues = "({" + Start + "}";
			for (var i = 1; i < Values.Length; i++)
				stringOfValues += ", {" + Values[i] + "}";
			stringOfValues += ")";
			return stringOfValues;
		}
	}
}
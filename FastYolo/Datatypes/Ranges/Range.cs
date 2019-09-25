using System;
using System.Diagnostics.Contracts;
using FastYolo.Extensions;

namespace FastYolo.Datatypes.Ranges
{
	/// <summary>
	///   Interval of two values. Allows a random value in between to be obtained.
	/// </summary>
	public class Range<T> : Lerp<Range<T>>
		where T : Lerp<T>
	{
		public Range()
		{
		}

		public Range(string rangeAsString)
		{
			var partitions = rangeAsString.SplitAndTrim('{', '}', '<', '>');
			if (partitions.Length != 5 || PartitionStartsAndEndsWithBrackets(partitions))
				throw new InvalidNumberOfDatatypeComponents<Range<T>>();
			InitializeComponentsFromStringsIfPossible(partitions[1], partitions[3]);
		}

		public Range(T minimum, T maximum)
		{
			// ReSharper disable DoNotCallOverridableMethodsInConstructor
			Start = minimum;
			End = maximum;
		}

		public virtual T Start { get; set; }
		public virtual T End { get; set; }

		private static bool PartitionStartsAndEndsWithBrackets(string[] partitions)
		{
			return partitions[0] != "(" || partitions[4] != ")" || partitions[2] != ",";
		}

		private void InitializeComponentsFromStringsIfPossible(string startAsString,
			string endAsString)
		{
			try
			{
				Start = (T) Activator.CreateInstance(typeof(T), startAsString);
				End = (T) Activator.CreateInstance(typeof(T), endAsString);
			}
			catch
			{
				throw new TypeInStringNotEqualToInitializedType();
			}
		}

		public class TypeInStringNotEqualToInitializedType : Exception
		{
		}

		[Pure]
		public override string ToString()
		{
			return "({" + Start + "}, {" + End + "})";
		}

		[Pure]
		public T GetRandomValue(float randomValueBetweenZeroAndOne)
		{
			return Start.Lerp(End, randomValueBetweenZeroAndOne);
		}

		public Range<T> Lerp(Range<T> other, float interpolation)
		{
			var start = Start.Lerp(other.Start, interpolation);
			var end = End.Lerp(other.End, interpolation);
			return new Range<T>(start, end);
		}
	}
}
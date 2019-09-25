using System.Collections.Generic;
using FastYolo.Extensions;

namespace FastYolo.Datatypes.Ranges
{
	public abstract class BaseRangeGraph<T> : Range<T>
		where T : Lerp<T>
	{
		protected BaseRangeGraph()
		{
			Values = new T[2];
		}

		protected BaseRangeGraph(List<T> values)
		{
			Values = values.ToArray();
		}

		protected BaseRangeGraph(T[] values)
		{
			Values = values;
		}

		protected BaseRangeGraph(T minimum, T maximum)
		{
			Values = new T[2];
			Values[0] = minimum;
			Values[1] = maximum;
		}

		public T[] Values { get; protected set; }

		public override T Start
		{
			get => Values[0];
			set => Values[0] = value;
		}

		public override T End
		{
			get => Values[Values.Length - 1];
			set => Values[Values.Length - 1] = value;
		}

		public abstract T GetInterpolatedValue(float interpolation);
		public abstract void SetValue(int index, T value);

		protected void ExpandAndAddValue(int insertIndex, T value)
		{
			Values = Values.Insert(value, insertIndex);
		}
	}
}
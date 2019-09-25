using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FastYolo.Extensions
{
	/// <summary>
	///   Provides the number of elements in an enum and some conversion and enumeration methods. Since
	///   .NET 4 a lot of helper methods are now available in .NET (e.g. HasFlag) and disappeared here.
	/// </summary>
	public static class EnumExtensions
	{
		private const int SkipZeroOrNoneFlagsValue = 0;

		/// <summary>
		///   For performance critical code, do not call this every frame or tick or in inner loops.
		///   Instead try to cache the value returned here, there is no need to evaluate it each frame
		///   many times, once a type is loaded it can never change and the enum count is always the same.
		/// </summary>
		public static int GetCount<EnumType>()
		{
			return Enum.GetValues(typeof(EnumType)).Length;
		}

		public static IEnumerable<EnumType> GetEnumValues<EnumType>()
		{
			return from object value in Enum.GetValues(typeof(EnumType)) select (EnumType) value;
		}

		public static int GetCount(this Enum anyEnum)
		{
			return GetEnumValues(anyEnum).Length;
		}

		public static Array GetEnumValues(this Enum anyEnum)
		{
			var enumType = anyEnum.GetType();
			return Enum.GetValues(enumType);
		}

		public static int GetIndex<T>(T searchEnumValue)
		{
			var list = new List<T>(GetEnumValues<T>());
			for (var index = 0; index < list.Count; index++)
				if (list[index].Equals(searchEnumValue))
					return index;
			return -1;
		}

		public static int GetIndex(IList enumValues, Enum searchEnumValue)
		{
			for (var index = 0; index < enumValues.Count; index++)
				if (enumValues[index].Equals(searchEnumValue))
					return index;
			return -1;
		}

		public static IEnumerable<Enum> GetIndividualFlags<EnumType>()
		{
			ulong flag = 0x1;
			foreach (var value in GetEnumValues<EnumType>().Cast<Enum>())
			{
				var bits = Convert.ToUInt64(value);
				if (bits == SkipZeroOrNoneFlagsValue)
					continue;
				while (flag < bits)
					flag <<= 1;
				if (flag == bits)
					yield return value;
			}
		}

		public static IEnumerable<Enum> GetIndividualFlags(this Enum enumValueWithFlagsSet)
		{
			ulong flag = 0x1;
			foreach (var value in GetEnumValues(enumValueWithFlagsSet).Cast<Enum>())
			{
				var bits = Convert.ToUInt64(value);
				if (bits == SkipZeroOrNoneFlagsValue)
					continue;
				while (flag < bits)
					flag <<= 1;
				if (flag == bits && enumValueWithFlagsSet.HasFlag(value))
					yield return value;
			}
		}

		public static EnumType ParseAsEnum<EnumType>(this string enumValue) where EnumType : struct
		{
			return string.IsNullOrEmpty(enumValue)
				? default
				: (EnumType) Enum.Parse(typeof(EnumType), enumValue);
		}

		public static bool TryParseAsEnum<EnumType>(this string enumValue, out EnumType result)
			where EnumType : struct
		{
			return Enum.TryParse(enumValue, true, out result);
		}
	}
}
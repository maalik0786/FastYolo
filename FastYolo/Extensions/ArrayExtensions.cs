using System;
using System.Collections.Generic;
using System.Linq;

namespace FastYolo.Extensions
{
	/// <summary>
	///   Additional array and enumerable manipulation and array to text methods.
	/// </summary>
	public static class ArrayExtensions
	{
		public static bool Compare<T>(this IEnumerable<T> array1, IEnumerable<T> array2)
		{
			return array1 == null && array2 == null || array1 != null && array2 != null &&
			       array1.SequenceEqual(array2);
		}

		/// <summary>
		///   Optimized version for lists and arrays that can be checked a little quicker by checking size.
		/// </summary>
		public static bool Compare<T>(this IList<T> array1, IList<T> array2)
		{
			return array1 == null && array2 == null || array1 != null && array2 != null &&
			       array1.Count == array2.Count && array1.SequenceEqual(array2);
		}

		public static Value GetWithDefault<Key, Value>(Dictionary<Key, object> dict, Key key)
		{
			if (!dict.ContainsKey(key))
				return default;
			Value result;
			try
			{
				result = (Value) dict[key];
			}
			catch
			{
				result = default;
			}

			return result;
		}

		/// <summary>
		///   Combines two arrays, but will not add duplicate entries from append into original.
		/// </summary>
		public static T[] Combine<T>(this T[] original, params T[] append)
		{
			if (original == null || original.Length == 0)
				return append;
			if (append == null || append.Length == 0)
				return original;
			var result = new List<T>(original);
			for (var i = 0; i < append.Length; i++)
				if (!result.Contains(append[i]))
					result.Add(append[i]);
			return result.ToArray();
		}

		// ReSharper disable once MethodNameNotMeaningful
		public static T[] Add<T>(this T[] first, T[] second, int firstLength = 0, int secondLength = 0)
		{
			if (first == null || second == null)
				throw new ArgumentNullException();
			if (firstLength == 0)
				firstLength = first.Length;
			if (secondLength == 0)
				secondLength = second.Length;
			var result = new T[firstLength + secondLength];
			for (var i = 0; i < firstLength; i++)
				result[i] = first[i];
			for (var i = 0; i < secondLength; i++)
				result[firstLength + i] = second[i];
			return result;
		}

		public static string ToText<T>(this IEnumerable<T> texts, string separator = ", ",
			int limit = 0)
		{
			if (texts == null)
				return "";
			if (limit == 0)
				return string.Join(separator, texts);
			var result = "";
			foreach (var text in texts)
			{
				result += (result == "" ? "" : separator) + text;
				limit--;
				if (limit <= 0)
					break;
			}

			return result;
		}

		public static T[] Insert<T>(this T[] array, T value, int insertIndex)
		{
			var result = new T[array.Length + 1];
			for (var i = 0; i < result.Length; i++)
				if (i == insertIndex)
					result[i] = value;
				else if (i > insertIndex)
					result[i] = array[i - 1];
				else
					result[i] = array[i];
			return result;
		}

		public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> enumerable)
		{
			return enumerable ?? Enumerable.Empty<T>();
		}
	}
}
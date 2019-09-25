using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FastYolo.Extensions
{
	/// <summary>
	///   Provides additional and simplified string manipulation methods.
	/// </summary>
	// ReSharper disable once ClassTooBig
	public static class StringExtensions
	{
		//ncrunch: no coverage start, NCrunch has problems with static readonly
		private static readonly Dictionary<Type, Func<string, object>> RegisteredConvertCallbacks =
			new Dictionary<Type, Func<string, object>>(); //ncrunch: no coverage end

		public static string ToInvariantString(this float number)
		{
			return number.ToString(NumberFormatInfo.InvariantInfo);
		}

		public static void AddConvertTypeCreation(Type typeToConvert, Func<string, object> conversion)
		{
			if (!RegisteredConvertCallbacks.ContainsKey(typeToConvert))
				RegisteredConvertCallbacks.Add(typeToConvert, conversion);
		}

		public static bool CanConvertToString(Type instanceType)
		{
			return RegisteredConvertCallbacks.ContainsKey(instanceType) ||
			       instanceType == typeof(string) || instanceType.IsPrimitive || instanceType.IsEnum ||
			       instanceType == typeof(DateTime) || instanceType == typeof(Dictionary<string, string>);
		}

		public static string ToInvariantString(object someObj)
		{
			if (someObj == null)
				return "null";
			if (someObj is float)
				return ((float) someObj).ToString("0.###", NumberFormatInfo.InvariantInfo);
			if (someObj is double)
				return ((double) someObj).ToString("0.####", NumberFormatInfo.InvariantInfo);
			if (someObj is decimal)
				return ((decimal) someObj).ToString(NumberFormatInfo.InvariantInfo);
			if (someObj is DateTime)
				return ((DateTime) someObj).ToString(CultureInfo.InvariantCulture);
			if (someObj is IEnumerable<KeyValuePair<string, string>>)
				return ConvertEnumerableKeyValuePairToString(
					(IEnumerable<KeyValuePair<string, string>>) someObj);
			return someObj.ToString();
		}

		private static string ConvertEnumerableKeyValuePairToString(
			IEnumerable<KeyValuePair<string, string>> enumerableKvp)
		{
			var separatedValues = "";
			foreach (var kvp in enumerableKvp)
				separatedValues += (separatedValues == "" ? "" : ";") + kvp.Key + "|" + kvp.Value;
			return separatedValues;
		}

		public static float[] SplitIntoFloats(this string value)
		{
			return SplitIntoFloats(value, ',', ';', '(', ')', '{', '}', ' ');
		}

		public static float[] SplitIntoFloats(this string value, params char[] separators)
		{
			return SplitIntoFloats(value.Split(separators, StringSplitOptions.RemoveEmptyEntries));
		}

		public static float[] SplitIntoFloats(this string[] components)
		{
			var floats = new float[components.Length];
			for (var i = 0; i < floats.Length; i++)
				floats[i] = components[i].Convert<float>();
			return floats;
		}

		/// <summary>
		///   Converts strings into build in type (string, int, float, etc.) or any Enum, Date or
		///   AddConvertTypeCreation registered type. Used for datatypes and general string conversion.
		/// </summary>
		public static T Convert<T>(this string value)
		{
			return (T) Convert(value, typeof(T));
		}

		public static object Convert(this string value, Type type)
		{
			if (type == typeof(string))
				return value;
			if (type.IsPrimitive)
				return ConvertPrimitive(value, type);
			if (type.IsEnum)
				return ConvertEnumValue(value, type);
			if (type == typeof(DateTime))
				return DateExtensions.Parse(value);
			if (RegisteredConvertCallbacks.TryGetValue(type, out var callback))
				return callback(value);
			if (type == typeof(Dictionary<string, string>))
				return ConvertStringToDictionary(value);
			throw new TypeWasNotRegisteredForConversionFromString(type);
		}

		private static object ConvertEnumValue(string value, Type type)
		{
			try
			{
				return Enum.Parse(type, value);
			}
			catch (ArgumentException)
			{
				throw new EnumValueNotFound(value, type);
			}
		}

		private static object ConvertPrimitive(string value, Type type)
		{
			if (type == typeof(int))
				return System.Convert.ToInt32(value);
			if (type == typeof(long))
				return System.Convert.ToInt64(value);
			if (type == typeof(byte))
				return System.Convert.ToByte(value);
			if (type == typeof(double))
				return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
			if (type == typeof(float))
				return System.Convert.ToSingle(value, CultureInfo.InvariantCulture);
			if (type == typeof(bool))
				return System.Convert.ToBoolean(value);
			if (type == typeof(char))
				return System.Convert.ToChar(value);
			throw new TypeWasNotRegisteredForConversionFromString(type);
		}

		private static object ConvertStringToDictionary(string value)
		{
			var dictionary = new Dictionary<string, string>();
			if (value == null)
				return dictionary;
			var splitValues = value.Split('|', ';');
			for (var i = 0; i < splitValues.Length - 1; i += 2)
				dictionary.Add(splitValues[i], splitValues[i + 1]);
			return dictionary;
		}

		public static string ConvertFirstCharacterToUpperCase(this string word)
		{
			if (string.IsNullOrEmpty(word) || !word.IsFirstCharacterInLowerCase())
				return word;
			return (char) (word[0] - 32) + word.Substring(1);
		}

		public static bool IsFirstCharacterInLowerCase(this string word)
		{
			if (string.IsNullOrEmpty(word))
				return true;
			var firstChar = word[0];
			return firstChar < 'A' || firstChar > 'Z';
		}

		public static byte[] ToByteArray(string text)
		{
			return Encoding.UTF8.GetBytes(text);
		}

		public static string FromByteArray(byte[] byteArray)
		{
			return Encoding.UTF8.GetString(byteArray);
		}

		public static string GetFilenameWithoutForbiddenCharactersOrSpaces(string filename)
		{
			if (string.IsNullOrWhiteSpace(filename))
				return "";
			var cleanFileName = "";
			foreach (var character in filename)
				if (character > ' ' && IsAllowedFilenameCharacter(character))
					cleanFileName += character;
			return cleanFileName;
		}

		private static bool IsAllowedFilenameCharacter(char character)
		{
			return character >= 'A' && character <= 'Z' || character >= 'a' && character <= 'z' ||
			       character >= '0' && character <= '9' || character == '.';
		}

		public static string ToInvariantString(this float number, string format)
		{
			return number.ToString(format, NumberFormatInfo.InvariantInfo);
		}

		public static float[] SplitIntoFloats(this string value, params string[] separators)
		{
			return SplitIntoFloats(value.Split(separators, StringSplitOptions.RemoveEmptyEntries));
		}

		public static string MaxStringLength(this string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
				return value;
			if (maxLength < 2)
				maxLength = 2;
			return value.Substring(0, maxLength - 2).TrimEnd() + "..";
		}

		public static string[] SplitAndTrim(this string value, params char[] separators)
		{
			return TrimAndRemoveEmptyElements(value.Split(separators, StringSplitOptions.None));
		}

		private static string[] TrimAndRemoveEmptyElements(string[] values)
		{
			var nonEmptyElements = new List<string>();
			for (var i = 0; i < values.Length; i++)
			{
				var trimmedElement = values[i].Trim();
				if (trimmedElement.Length > 0)
					nonEmptyElements.Add(trimmedElement);
			}

			return nonEmptyElements.ToArray();
		}

		public static string[] SplitAndTrim(this string value, params string[] separators)
		{
			return TrimAndRemoveEmptyElements(value.Split(separators, StringSplitOptions.None));
		}

		public static string[] SplitLines(this string text)
		{
			return text.Split(new[] {Environment.NewLine, "\n"}, StringSplitOptions.None);
		}

		/// <summary>
		///   Compare with Invariant Culture and ignore case, used quite often for content and paths.
		/// </summary>
		public static bool Compare(this string value, string other)
		{
			return string.Compare(value, other, StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		public static bool ContainsCaseInsensitive(this string value, string searchText)
		{
			return value != null && value.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public static bool StartsWith(this string name, params string[] partialNames)
		{
			return partialNames.Any(x => name.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));
		}

		public static IList<string> SplitWords(this string stringToSplit,
			bool convertFirstLetterOfEachWordAfterFirstWordToLowerCase = false)
		{
			if (string.IsNullOrEmpty(stringToSplit))
				return new string[0];
			var words = new List<string>();
			var currentWord = "";
			for (var i = 0; i < stringToSplit.Length; ++i)
			{
				var letter = stringToSplit[i];
				if (letter == char.ToUpper(letter) && i > 0 &&
				    IsValidLetterOrWhitespace(letter, stringToSplit[i - 1]))
					currentWord = AddWord(words, currentWord,
						convertFirstLetterOfEachWordAfterFirstWordToLowerCase
							? char.ToLowerInvariant(letter)
							: letter);
				else
					currentWord += letter;
			}

			if (currentWord.Length > 0)
				words.Add(currentWord);
			return words;
		}

		private static bool IsValidLetterOrWhitespace(char letter, char lastLetter)
		{
			return (letter >= 'A' || char.IsWhiteSpace(letter)) && !char.IsNumber(lastLetter);
		}

		private static string AddWord(List<string> words, string currentWord, char letter)
		{
			if (currentWord.Length > 0)
			{
				words.Add(currentWord);
				currentWord = "";
			}

			if (!char.IsWhiteSpace(letter))
				currentWord = letter + "";
			return currentWord;
		}

		public static int TryParse(this string text, int defaultValue)
		{
			return int.TryParse(text, out var result) ? result : defaultValue;
		}

		public static float TryParse(this string text, float defaultValue)
		{
			return float.TryParse(text, NumberStyles.Number, NumberFormatInfo.InvariantInfo,
				out var result)
				? result
				: defaultValue;
		}

		/// <summary>
		///   Tries to convert any text to a enum, if it fails defaultValue is returned. If you know the
		///   text will be in the correct format use <see cref="StringExtensions.Convert{T}" />.
		/// </summary>
		public static T TryParse<T>(this string text, T defaultValue) where T : struct
		{
			if (Enum.TryParse(text, true, out T result))
				return result;
			if (ExceptionExtensions.IsDebugMode && Environment.UserInteractive)
				//ncrunch: no coverage start, on TeamCity build agent services we are not UserInteractive
				Console.WriteLine("Failed to parse enum value " + text + ", using default: " +
				                  defaultValue);
			//ncrunch: no coverage end
			return defaultValue;
		}

		/// <summary>
		///   Unlike <see cref="string.GetHashCode" /> this computes a platform indepentent hash code
		///   which is guaranteed not to change in the future and is quite fast, <see cref="MurmurHash" />
		/// </summary>
		public static int GetStableHashCode(this string value)
		{
			return new MurmurHash().ComputeHash(Encoding.UTF8.GetBytes(value));
		}

		/// <summary>
		///   Write bytes, KB, MB, GB, TB message. 1 KB = 1024 Bytes, 1 MB = 1024 KB = 1048576 Bytes,
		///   1 GB = 1024 MB = 1073741824 Bytes, 1 TB = 1024 GB = 1099511627776 Bytes, 100 will return
		///   "100 Bytes", 2048 will return "2.00 KB", 2500 will return "2.44 KB", 1534905 will return
		///   "1.46 MB", 23045904850904 will return "20.96 TB"
		/// </summary>
		public static string WriteBytesKbMbGbNumber(long value)
		{
			return value < 0
				? "-" + WriteBytesKbMbGbNumber(-value)
				: value <= 999
					? value + " Bytes"
					: value <= 999 * 1024
						? WriteNumber(value / 1024.0, "KB")
						: value <= 999 * 1024 * 1024
							? WriteNumber(value / (1024.0 * 1024.0), "MB")
							: value <= 999L * 1024L * 1024L * 1024L
								? WriteNumber(value / (1024.0 * 1024.0 * 1024.0), "GB")
								: WriteNumber(value / (1024.0 * 1024.0 * 1024.0 * 1024.0), "TB");
		}

		private static string WriteNumber(double value, string numberPostfix)
		{
			return value.ToString("#.##", CultureInfo.InvariantCulture) + " " + numberPostfix;
		}

		public class EnumValueNotFound : Exception
		{
			public EnumValueNotFound(string value, Type enumType)
				: base(value + ", Enum " + enumType + ": " + enumType.GetEnumNames().ToText())
			{
			}
		}

		public class TypeWasNotRegisteredForConversionFromString : Exception
		{
			public TypeWasNotRegisteredForConversionFromString(Type type) : base(type.ToString())
			{
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace FastYolo.Extensions
{
	public static class TypeExtensions
	{
		public static bool IsValueEquals<T>(this T obj, T other)
		{
			return EqualityComparer<T>.Default.Equals(obj, other);
		}

		/// <summary>
		///   Formats the type name containing the generic components if existing in a C# like syntax,
		///   e.g. <code>List&lt;String&gt;</code> instead of the CLR name <code>List`1</code>.
		/// </summary>
		public static string GetNameWithGenericComponents(this Type type)
		{
			if (!type.IsGenericType)
				return type.Name;
			var genericArguments = type.GetGenericArguments();
			var typeName = type.GetNameWithoutGenericArity();
			return typeName + "<" + string.Join(", ",
				       genericArguments.Select(GetNameWithGenericComponents)) + ">";
		}

		/// <summary>
		///   Removes the generic arities from a CLR type name if it exists,
		///   e.g. <code>List`1</code> will be formatted as <code>List</code>.
		/// </summary>
		public static string GetNameWithoutGenericArity(this Type type)
		{
			var name = type.Name;
			var index = name.IndexOf('`');
			return index == -1 ? name : name.Substring(0, index);
		}
	}
}
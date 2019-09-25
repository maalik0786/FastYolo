using System;
using System.Globalization;
using System.Reflection;

namespace FastYolo.Extensions
{
	/// <summary>
	///   Helps creating types and getting to their fields, properties and methods without having to
	///   duplicate the kind of difficult and cumbersome to use Activator, BindingFlags and Type classes
	/// </summary>
	public static class ActivatorExtensions
	{
		/// <summary>
		///   Grabs all public, protected or private properties or fields from the type and base types.
		///   Note that while public and protected base type fields and properties are found, private
		///   members of base types are not returned. Make those fields are at least protected if needed.
		/// </summary>
		private const BindingFlags PublicOrNonPublicInstance =
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		/// <summary>
		///   GetFields, GetProperties, etc. all return public instance and static results, which is
		///   usually not what we want, so this is used in all the GetPublic methods here.
		/// </summary>
		private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

		/// <summary>
		///   Limits the search to just public methods, fields or properties defined in the type.
		/// </summary>
		private const BindingFlags DeclaredPublicInstance =
			BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

		/// <summary>
		///   Limits the search to just fields, properties or methods declared in the specified type.
		/// </summary>
		private const BindingFlags DeclaredPublicOrNonPublicInstance =
			DeclaredPublicInstance | BindingFlags.NonPublic;

		public static PropertyInfo GetPublicNonPublicOrDerivedProperty(this Type type, string name)
		{
			return type.GetProperty(name, PublicOrNonPublicInstance);
		}

		public static FieldInfo GetPublicNonPublicOrDerivedField(this Type type, string name)
		{
			return type.GetField(name, PublicOrNonPublicInstance);
		}

		public static void SetPublicNonPublicOrDerivedValue(this PropertyInfo property,
			object objectToFill, object value)
		{
			property.SetValue(objectToFill, value, PublicOrNonPublicInstance, null, null, null);
		}

		public static void SetPublicNonPublicOrDerivedValue(this FieldInfo field, object objectToFill,
			object value)
		{
			field.SetValue(objectToFill, value, PublicOrNonPublicInstance, null, null);
		}

		public static PropertyInfo[] GetPublicNonPublicAndDerivedProperties(this Type type)
		{
			return type.GetProperties(PublicOrNonPublicInstance);
		}

		public static FieldInfo[] GetPublicAndNonPublicAndDerivedFields(this Type type)
		{
			return type.GetFields(PublicOrNonPublicInstance);
		}

		public static PropertyInfo[] GetPublicAndDerivedProperties(this Type type)
		{
			return type.GetProperties(PublicInstance);
		}

		public static FieldInfo[] GetPublicAndDerivedFields(this Type type)
		{
			return type.GetFields(PublicInstance);
		}

		public static MethodInfo[] GetPublicAndDerivedMethods(this Type type)
		{
			return type.GetMethods(PublicInstance);
		}

		public static MethodInfo GetPublicMethod(this Type type, string name)
		{
			return type.GetMethod(name, PublicInstance);
		}

		public static MethodInfo[] GetPublicMethodsDeclaredInType(this Type type)
		{
			return type.GetMethods(DeclaredPublicOrNonPublicInstance);
		}

		public static MethodInfo[] GetPublicAndNonPublicMethodsDeclaredInType(this Type type)
		{
			return type.GetMethods(DeclaredPublicOrNonPublicInstance);
		}

		public static ConstructorInfo[] GetPublicAndNonPublicConstructorsDeclaredInType(this Type type)
		{
			return type.GetConstructors(DeclaredPublicOrNonPublicInstance);
		}

		public static PropertyInfo[] GetPublicAndNonPublicPropertiesDeclaredInType(this Type type)
		{
			return type.GetProperties(DeclaredPublicOrNonPublicInstance);
		}

		public static FieldInfo[] GetPublicAndNonPublicFieldsDeclaredInType(this Type type)
		{
			return type.GetFields(DeclaredPublicOrNonPublicInstance);
		}

		public static CreateType CreatePublicOrNonPublicInstance<CreateType>(params object[] parameters)
		{
			return (CreateType) Activator.CreateInstance(typeof(CreateType), PublicOrNonPublicInstance,
				Type.DefaultBinder, parameters, CultureInfo.CurrentCulture);
		}

		public static object CreatePublicOrNonPublicInstance(this Type type, params object[] parameters)
		{
			return Activator.CreateInstance(type, PublicOrNonPublicInstance, Type.DefaultBinder,
				parameters,
				CultureInfo.CurrentCulture);
		}

		public static bool HasParameterlessConstructor(this Type type)
		{
			return type.IsValueType ||
			       type.GetConstructor(PublicOrNonPublicInstance, null, Type.EmptyTypes, null) != null;
		}
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace FastYolo.Extensions
{
	/// <summary>
	///   Allows to easily save and recreate binary data objects with the full type names like other
	///   Serializers, but way faster (100x). Before reconstructing types load all needed assemblies.
	///   Also will stop at content types and only send their names.
	/// </summary>
	public static class BinaryDataExtensions
	{
		private const char GenericTypeSeparator = '.';

		private static readonly Dictionary<string, Type> TypeMap = new Dictionary<string, Type>();
		private static readonly Dictionary<Type, string> ShortNames = new Dictionary<Type, string>();

		private static readonly Dictionary<string, Type> GenericTypeMap =
			new Dictionary<string, Type>();

		private static readonly Dictionary<Type, string> GenericShortNames =
			new Dictionary<Type, string>();

		//ncrunch: no coverage start (faster to not profile this code, lots of calls, but very fast)
		static BinaryDataExtensions()
		{
			AddPrimitiveTypes();
			AppDomain.CurrentDomain.AssemblyLoad += (o, args) =>
			{
				if (ShouldLoadTypes(args.LoadedAssembly))
					AddAssemblyTypes(args.LoadedAssembly);
			};
			RegisterAvailableBinaryDataImplementation();
		}

		private static void AddPrimitiveTypes()
		{
			AddType(typeof(object));
			AddType(typeof(bool));
			AddType(typeof(byte));
			AddType(typeof(char));
			AddType(typeof(decimal));
			AddType(typeof(double));
			AddType(typeof(float));
			AddType(typeof(string));
			AddType(typeof(sbyte));
			AddPrimitiveIntegerTypes();
		}

		private static void AddType(Type type)
		{
			var shortName = type.Name;
			if (TypeMap.ContainsKey(shortName))
			{
				shortName = type.FullName;
				if (TypeMap.ContainsKey(shortName))
					return;
			}

			ShortNames.Add(type, shortName);
			TypeMap.Add(shortName, type);
			if (!type.IsGenericType || GenericTypeMap.ContainsKey(shortName.Replace("`1", "")))
				return;
			GenericTypeMap.Add(shortName.Replace("`1", ""), type);
			GenericShortNames.Add(type, shortName.Replace("`1", ""));
		}

		private static void AddPrimitiveIntegerTypes()
		{
			AddType(typeof(short));
			AddType(typeof(int));
			AddType(typeof(long));
			AddType(typeof(ushort));
			AddType(typeof(uint));
			AddType(typeof(ulong));
		}

		private static bool ShouldLoadTypes(Assembly assembly)
		{
			var name = assembly.GetName().Name;
			return name == "DeltaEngine" || name.EndsWith("Messages") ||
			       !name.StartsWith("nunit") && !name.EndsWith(".Xml") && assembly.IsAllowed() &&
			       !AssemblyExtensions.IsFrameworkAndNotTestAssembly(name);
		}

		private static void AddAssemblyTypes(Assembly assembly)
		{
			try
			{
				TryAddAssemblyTypes(assembly);
			}
			catch (ReflectionTypeLoadException)
			{
				//foreach (var failedLoader in ex.LoaderExceptions)
				//Logger.Error(failedLoader);
			}
			catch (Exception)
			{
				//Logger.Error(ex);
			}
		}

		private static void TryAddAssemblyTypes(Assembly assembly)
		{
			lock (TypeMap)
			{
				var types = assembly.GetTypes();
				foreach (var type in types)
					if (IsValidBinaryDataType(type))
						AddType(type);
			}
		}

		/// <summary>
		///   Any concrete type that is not dynamic or tests is valid, also include all MarshalByRefObject
		///   types like ContentFile, which are still abstract, but needed for SceneSerializer.
		/// </summary>
		private static bool IsValidBinaryDataType(Type type)
		{
			return (!type.IsAbstract || typeof(MarshalByRefObject).IsAssignableFrom(type)) &&
			       !typeof(Exception).IsAssignableFrom(type) &&
			       !type.Name.StartsWith("<") && !type.Name.StartsWith("__") &&
			       !type.Name.EndsWith(".Tests");
		}

		private static void RegisterAvailableBinaryDataImplementation()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
				if (ShouldLoadTypes(assembly))
					AddAssemblyTypes(assembly);
		}

		public static string GetShortName(object data)
		{
			return GetShortName(data.GetType());
		}

		public static string GetShortName(Type type)
		{
			if (ShortNames.TryGetValue(type, out var value))
				return value;
			if (IsGenericType(type))
				return CreateGenericType(type);
			throw new NoShortNameStoredFor(type);
		}

		private static bool IsGenericType(Type type)
		{
			return type.IsGenericType && GenericShortNames.ContainsKey(type.GetGenericTypeDefinition());
		}

		private static string CreateGenericType(Type type)
		{
			return GenericShortNames[type.GetGenericTypeDefinition()] + GenericTypeSeparator +
			       ShortNames[type.GetGenericArguments()[0]];
		}

		public static void WriteObjectTypeVersionNumber(this BinaryWriter writer, object data)
		{
			WriteVersionNumber(writer, data.GetType().Assembly.GetName().Version);
		}

		/// <summary>
		///   Stores just the Major and Minor as 2 bytes and the Build number as ushort, ignore Revision.
		/// </summary>
		public static void WriteVersionNumber(this BinaryWriter writer, Version dataVersion)
		{
			writer.Write((byte) dataVersion.Major);
			writer.Write((byte) dataVersion.Minor);
			writer.Write((ushort) dataVersion.Build);
		}

		public static Version ReadVersionNumber(this BinaryReader reader)
		{
			return new Version(reader.ReadByte(), reader.ReadByte(), reader.ReadUInt16());
		}

		public static Type GetTypeFromShortNameOrFullNameIfNotFound(string typeName)
		{
			return TypeMap.TryGetValue(typeName, out var value)
				? value
				: GetGenericTypeFromShortNameOrFullName(typeName);
		}

		private static Type GetGenericTypeFromShortNameOrFullName(string typeName)
		{
			if (typeName.Contains(GenericTypeSeparator + ""))
			{
				var typeParts = typeName.Split(GenericTypeSeparator);
				if (typeParts.Length == 2 && GenericTypeMap.ContainsKey(typeParts[0]) &&
				    TypeMap.ContainsKey(typeParts[1]))
					return GenericTypeMap[typeParts[0]].MakeGenericType(TypeMap[typeParts[1]]);
			} //ncrunch: no coverage

			return GetTypeFromFullName(typeName);
		}

		private static Type GetTypeFromFullName(string typeName)
		{
			if (IsTypeNameWithFullAssemblyInformation(typeName))
				return Type.GetType(typeName, true);
			var requestedTypeNamespace = Path.GetFileNameWithoutExtension(typeName);
			var requestedParentNamespace = Path.GetFileNameWithoutExtension(requestedTypeNamespace);
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
				if (assembly.GetName().Name == requestedTypeNamespace ||
				    assembly.GetName().Name == requestedParentNamespace)
					return assembly.GetType(typeName, true, false);
			throw new TypeLoadException("Unable to find type: " + typeName);
		}

		private static bool IsTypeNameWithFullAssemblyInformation(string typeName)
		{
			return typeName.Contains(",");
		}

		/// <summary>
		///   Writes 0-253 as a single byte into the writer. However if number is bigger, 254 will be
		///   stored plus the size as an ushort, or 255 if the number only fits into a full int (5 bytes).
		///   Because this is a length information, negative numbers are not supported.
		/// </summary>
		public static void WriteLengthNumberMostlyBelow254(this BinaryWriter writer, int number)
		{
			if (number < 254)
			{
				writer.Write((byte) number);
			}
			else if (number <= ushort.MaxValue)
			{
				writer.Write((byte) 254);
				writer.Write((ushort) number);
			}
			else
			{
				writer.Write((byte) 255);
				writer.Write(number);
			}
		}

		public static int ReadLengthNumberMostlyBelow254(this BinaryReader reader)
		{
			int number = reader.ReadByte();
			return number == 254 ? reader.ReadUInt16() : number == 255 ? reader.ReadInt32() : number;
		}

		public static string GetShortNameOrFullNameIfNotFound(object data)
		{
			return GetShortNameOrFullNameIfNotFound(GetTypeOrObjectType(data));
		}

		public static string GetShortNameOrFullNameIfNotFound(Type type)
		{
			if (ShortNames.TryGetValue(type, out var shortTypeName))
				return shortTypeName;
			if (IsGenericType(type))
				return CreateGenericType(type); //ncrunch: no coverage
			if (ShortNames.TryGetValue(type.BaseType, out shortTypeName))
				return shortTypeName; //ncrunch: no coverage
			return type.AssemblyQualifiedName;
		}

		public static Type GetTypeOrObjectType(object element)
		{
			return element?.GetType() ?? typeof(object);
		}

		public static bool NeedToSaveTypeName(Type fieldType)
		{
			return fieldType.AssemblyQualifiedName != null && fieldType != typeof(string) &&
			       !fieldType.IsArray && !typeof(IList).IsAssignableFrom(fieldType) &&
			       !typeof(IDictionary).IsAssignableFrom(fieldType) && fieldType != typeof(MemoryStream);
		}

		public static bool DoNotNeedToSaveType(this FieldInfo field,
			FieldAttributes fieldAttributes)
		{
			return fieldAttributes.HasFlag(FieldAttributes.NotSerialized) ||
			       field.IsNotSupportedTypeforSerialization() ||
			       field.GetCustomAttributes(typeof(DoNotSerializeAttribute), false).Length > 0 ||
			       IsMatchingPropertyTypeUsingDoNotSerializeAttribute(field);
		}

		private static bool IsNotSupportedTypeforSerialization(this FieldInfo field)
		{
			return field.FieldType == typeof(Action) || field.FieldType == typeof(Action<>) ||
			       field.FieldType.BaseType == typeof(MulticastDelegate) ||
			       field.FieldType == typeof(ISerializable) || field.FieldType == typeof(BinaryWriter) ||
			       field.FieldType == typeof(BinaryReader) || field.FieldType == typeof(Pointer) ||
			       field.FieldType == typeof(IntPtr);
		}

		private static bool IsMatchingPropertyTypeUsingDoNotSerializeAttribute(FieldInfo field)
		{
			if (!field.Name.StartsWith("<"))
				return false;
			var parts = field.Name.Split('<', '>');
			if (parts.Length < 3)
				return false; //ncrunch: no coverage, just in case
			var propertyType = field.DeclaringType.GetProperty(parts[1]);
			if (propertyType == null)
				return false; //ncrunch: no coverage, just in case
			return propertyType.GetCustomAttributes(typeof(DoNotSerializeAttribute), false).Length > 0;
		}

		internal class NoShortNameStoredFor : Exception
		{
			public NoShortNameStoredFor(Type type) : base(type.ToString())
			{
			}
		}
	}
}
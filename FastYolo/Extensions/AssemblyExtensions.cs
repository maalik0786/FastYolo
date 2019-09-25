using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FastYolo.Extensions
{
	/// <summary>
	///   Additional methods for assembly related actions.
	/// </summary>
	public static class AssemblyExtensions
	{
		private static string testOrProjectName;

		//ncrunch: no coverage start
		public static string GetTestNameOrProjectName()
		{
			return testOrProjectName ?? (testOrProjectName = StackTraceExtensions.GetEntryName());
		}

		public static void SetProjectName(string setTestOrProjectName)
		{
			testOrProjectName = setTestOrProjectName;
		}

		public static bool IsAllowed(this Assembly assembly)
		{
			return !assembly.IsDynamic && !assembly.GlobalAssemblyCache &&
			       IsAllowed(assembly.GetName().Name);
		}

		/// <summary>
		///   Is the assembly valid to load for types in AssemblyTypeLoader and BinaryDataExtensions?
		///   Optimized for performance, most likely cases first and slower code last
		/// </summary>
		// ReSharper disable once CyclomaticComplexity
		public static bool IsAllowed(string assemblyName)
		{
			return assemblyName.StartsWith("DeltaEngine") || assemblyName == "LogoApp" ||
			       assemblyName == "EmptyApp" || !IsExternalAssembly(assemblyName);
		}

		public static bool IsExternalAssembly(string assemblyName)
		{
			return IsMicrosoftAssembly(assemblyName) || IsUsedLibrary(assemblyName) ||
			       IsIdeHelperTool(assemblyName) || IsThirdPartyLibrary(assemblyName);
		}

		// ReSharper disable once CyclomaticComplexity
		public static bool IsMicrosoftAssembly(string name)
		{
			return name == "mscorlib" || name == "System" || name.StartsWith("System.") ||
			       name.StartsWith("Microsoft.") || name.StartsWith("Windows") ||
			       name.StartsWith("PresentationFramework") || name.StartsWith("PresentationCore") ||
			       name == "Accessibility" || name.StartsWith("FSharp") || name.StartsWith("MetaData") ||
			       name == "SizeOfAssembly" || name == "GuardOfAssembly";
		}

		// ReSharper disable StringLiteralTypo
		private static bool IsUsedLibrary(string assemblyName)
		{
			return assemblyName.StartsWith("nunit.") || assemblyName.StartsWith("pnunit") ||
			       assemblyName.StartsWith("JetBrains.") || assemblyName == "Ionic.Zip.Reduced" ||
			       assemblyName.StartsWith("LZ4") || assemblyName == "NVorbis" ||
			       assemblyName == "AsfMojo" ||
			       assemblyName == "SMDiagnostics" || assemblyName == "Steamworks.NET";
		}

		// ReSharper disable once CyclomaticComplexity
		private static bool IsIdeHelperTool(string name)
		{
			return name.StartsWith("nunit.") || name.StartsWith("pnunit.") || name.StartsWith("xunit.") ||
			       name.StartsWith("JetBrains.") || name.StartsWith("NCrunch.") ||
			       name.StartsWith("nCrunch.") || name.StartsWith("ReSharper.") ||
			       name.StartsWith("vshost") ||
			       name.EndsWith(".vshost") || name.StartsWith("rm.") || name.StartsWith("NVidia.") ||
			       name.StartsWith("NDepend") || name.StartsWith("NuGet.") || name.StartsWith("EnvDTE") ||
			       name.StartsWith("RedGate.") || name.StartsWith("SyntaxTree.");
		}

		public static bool IsThirdPartyLibrary(string assemblyName)
		{
			var assemblyNameLowerCase = assemblyName.ToLowerInvariant();
			return ThirdPartyLibsFullNames.Contains(assemblyNameLowerCase) ||
			       ThirdPartyLibsPartialNames.Any(assemblyNameLowerCase.StartsWith) ||
			       VisualStudioExtensionAssemblyPartialNames.Any(assemblyNameLowerCase.StartsWith);
		}

		private static readonly string[] ThirdPartyLibsFullNames =
		{
			"opengl32", "openal32", "scopeenginemanaged",
			"wrap_oal", "libegl", "spine-csharp", "mono.cecil", "gallio", "reflector", "libgles",
			"libglesv2", "csogg", "csvorbis", "autofac", "moq", "opentk", "newtonsoft.json", "nvorbis",
			"naudio", "dotnetzip.reduced", "ionic.zip", "jitter", "uiautomationtypes", "log4net",
			"mono.security", "mono.posix", "unityeditor", "unityengine", "unityengine.ui",
			"unityvs.versionspecific", "i18n", "csteamworks", "castle.core", "xunit", "wpftoolkit",
			"wpfcontrib", "vscodedebugging", "sprache", "sharpcompress", "protobuf-net", "owin"
		};

		private static readonly string[] ThirdPartyLibsPartialNames =
		{
			"libvlc", "libpxc", "actiprosoftware", "analytics.bll", "antlr", "bltoolkit", "devexpress",
			"pencil.gaming", "avalondock", "farseer", "mvvmlight", "sharpdx", "slimdx", "toymp3",
			"entityframework", "nhibernate", "approval", "system.io.abstractions", "supersocket",
			"system.windows.interactivity", "asfmojo", "ionic.", "xceed", "wpflocalizeextension",
			"xamlmarkupextensions", "glfw", "monogame", "galasoft.mvvmlight", "aurelienribon.", "naudio",
			"boo.lang", "unityscript", "unity.", "unityeditor.", "assembly-csharp", "steam_api",
			"cookcomputing", "dotmemory", "libgeneratorutil", "mono.cecil", "mono.debug", "nvelocity",
			"vestris.", "psigen.", "bridge", "spine", "corapi"
		};

		private static readonly string[] VisualStudioExtensionAssemblyPartialNames =
		{
			"dynamicproxygen", "vslangproj", "easyhook", "uiautomationprovider", "devart.",
			"preemptive.", "concurrencyplugins", "clwwizard", "vsgraphicsdebuggerpkg", "silverlight",
			"typescript", "vsdebug", "gammajul", "awssdk", "anonymously hosted", "custommarshalers",
			"yworks", "yfiles", "graphml"
		};

		public static bool IsAllowed(this AssemblyName assemblyName)
		{
			return IsAllowed(assemblyName.Name);
		}

		public static bool IsFrameworkAndNotTestAssembly(string assemblyName)
		{
			return !assemblyName.EndsWith(".Tests") && !assemblyName.EndsWith(".Remote") &&
			       assemblyName.StartsWith("DeltaEngine.Frameworks");
		}

		/// <summary>
		///   See http://geekswithblogs.net/rupreet/archive/2005/11/02/58873.aspx
		/// </summary>
		public static bool IsManagedAssembly(string fileName)
		{
			using (Stream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				var reader = new BinaryReader(fs);
				GoToDataDictionaryOfPeOptionalHeaders(fs, reader);
				return GetDataDictionaryRva(reader)[14] != 0;
			}
		}

		private static uint[] GetDataDictionaryRva(BinaryReader reader)
		{
			var dataDictionaryRva = new uint[16];
			for (var i = 0; i < 15; i++)
			{
				dataDictionaryRva[i] = reader.ReadUInt32();
				reader.ReadUInt32();
			}

			return dataDictionaryRva;
		}

		/// <summary>
		///   See http://msdn.microsoft.com/en-us/library/windows/desktop/ms680313(v=vs.85).aspx
		/// </summary>
		private static void GoToDataDictionaryOfPeOptionalHeaders(Stream fs, BinaryReader reader)
		{
			fs.Position = 0x3C;
			fs.Position = reader.ReadUInt32();
			reader.ReadBytes(24);
			fs.Position = Convert.ToUInt16(Convert.ToUInt16(fs.Position) + 0x60);
		}

		public static bool IsVisualStudioOrEditorAssembly(string assemblyName)
		{
			return assemblyName.Contains("VisualStudio") || assemblyName.StartsWith("DeltaEngine.Editor");
		}

		public static bool IsActionOrEvent(Type type)
		{
			return type == typeof(Action) || type == typeof(Action<>) ||
			       typeof(MulticastDelegate).IsAssignableFrom(type);
		}

		// ReSharper disable once CyclomaticComplexity
		public static bool IsNotBasicDeltaEngineAssembly(string assemblyName)
		{
			return assemblyName != "DeltaEngine" && assemblyName != "DeltaEngine.Entities" &&
			       assemblyName != "DeltaEngine.Content" && assemblyName != "DeltaEngine.Resolvers" &&
			       assemblyName != "DeltaEngine.Networking" && assemblyName != "DeltaEngine.Multimedia" &&
			       assemblyName != "DeltaEngine.Input" && assemblyName != "DeltaEngine.Graphics" &&
			       assemblyName != "DeltaEngine.Xml" && assemblyName != "DeltaEngine.Sprites" &&
			       assemblyName != "DeltaEngine.Fonts" && assemblyName != "DeltaEngine.Tests" &&
			       assemblyName != "DeltaEngine.Mocks" &&
			       IsNotOptionalBasicDeltaEngineAssembly(assemblyName);
		}

		private static bool IsNotOptionalBasicDeltaEngineAssembly(string assemblyName)
		{
			return assemblyName != "DeltaEngine.Graphs" && assemblyName != "DeltaEngine.Achievements" &&
			       assemblyName != "DeltaEngine.InAppPurchase" &&
			       assemblyName != "DeltaEngine.Analytics" &&
			       assemblyName != "DeltaEngine.Ads" && assemblyName != "DeltaEngine.Authentication" &&
			       assemblyName != "DeltaEngine.Shapes2D" && assemblyName != "DeltaEngine.Physics2D" &&
			       assemblyName != "DeltaEngine.Shapes3D" && assemblyName != "DeltaEngine.Physics3D" &&
			       assemblyName != "DeltaEngine.Profiling";
		}

		// ReSharper disable once CyclomaticComplexity
		public static bool IsFrameworkOrResolverDependency(string assemblyName)
		{
			return assemblyName == "DeltaEngine" || assemblyName == "DeltaEngine.Content" ||
			       assemblyName == "DeltaEngine.Entities" || assemblyName == "DeltaEngine.Fonts" ||
			       assemblyName == "DeltaEngine.Graphics" || assemblyName == "DeltaEngine.Input" ||
			       assemblyName == "DeltaEngine.Multimedia" || assemblyName == "DeltaEngine.Sprites" ||
			       assemblyName == "DeltaEngine.Networking" || assemblyName == "DeltaEngine.Xml" ||
			       assemblyName.StartsWith("DeltaEngine.Frameworks.");
		}
	}
}
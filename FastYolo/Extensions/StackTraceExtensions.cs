using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FastYolo.Extensions
{
	/// <summary>
	///   Provides additional check methods on stack traces to find out where we are (e.g. in tests).
	///   Calls are optimized as best as it can be by caching results. However you still should not
	///   call any reflection or stack trace methods in inner loops, cache the results yourself first!
	/// </summary>
	// ReSharper disable once ClassTooBig
	public static class StackTraceExtensions
	{
		private const string TestAttribute = "NUnit.Framework.TestAttribute";

		private const string SetUpAttribute = "NUnit.Framework.SetUpAttribute";
		private const string FixtureSetUpAttribute = "NUnit.Framework.TestFixtureSetUpAttribute";

		private const string ApproveFirstFrameScreenshotAttribute =
			"DeltaEngine.Mocks.Resolvers.ApproveFirstFrameScreenshotAttribute";

		private static bool? wasStartedFromNCrunch;

		private static bool? wasStartedFromNUnitOrTeamCity;

		private static bool? wasStartedFromNCrunchOrForcedToUseMockResolver;

		private static readonly string[] EntryPointMethodNames =
		{
			"Main", "StartEntryPoint", "Start", "OnDrawGizmos"
		};

		private static string unitTestMethodName;

		private static string unitTestClassFullName;

		private static string unitTestClassName;

		private static readonly string[] MethodNamesToExclude =
		{
			"DeltaEngine.Resolvers.", "DeltaEngine.Networking",
			"DeltaEngine.Content.ContentFileLoader", "DeltaEngine.Content.ContentDeserializer",
			"DeltaEngine.Mocks.Resolvers.TestWithMocksOrVisually", "System.Action",
			"System.RuntimeMethodHandle", "System.Reflection", "TestDriven", "System.Threading",
			"System.AppDomain", "System.Activator", "System.Runtime", "NUnit.Framework",
			"Microsoft.VisualStudio.HostingProcess", "System.Windows.", "System.Net.", "MS.Win32.",
			"MS.Internal.", "NUnit.Core.", "xUnit.", "JetBrains.ReSharper.", "nCrunch.", "Autofac.",
			"System.Reactive", "lambda_method", "System.Collections", "System.ThrowHelper", "System.Linq"
		};

		//ncrunch: no coverage start (cost perf and most lines can only be reached from non-test code)
		/// <summary>
		///   These calls are quite slow, but have to be made for all tests, especially for
		///   TestWithMocksOrVisually to use the MockResolver while using the real resolver for
		///   visual testing via ReSharper or TestDriven.NET. Cache for one runner process.
		///   See http://www.ncrunch.net/documentation/troubleshooting_ncrunch-specific-overrides
		/// </summary>
		public static bool StartedFromNCrunch
		{
			get
			{
				if (wasStartedFromNCrunch != null)
					return wasStartedFromNCrunch.Value;
				wasStartedFromNCrunch = Environment.GetEnvironmentVariable("NCrunch") == "1";
				return wasStartedFromNCrunch.Value;
			}
		}

		/// <summary>
		///   Only returns true for NUnit console processes started from TeamCity, not for ReSharper or
		///   local NUnit console. NUnit2 uses "test-domain-", NUnit3 uses "domain-", other runners might
		///   use "NUnit Domain", Resharper however always uses "NUnit "+assemblyName.
		/// </summary>
		private static bool StartedFromNUnitInTeamCity
		{
			get
			{
				if (wasStartedFromNUnitOrTeamCity != null)
					return wasStartedFromNUnitOrTeamCity.Value;
				var domainName = AppDomain.CurrentDomain.FriendlyName;
				wasStartedFromNUnitOrTeamCity = (domainName.StartsWith("test-domain-") ||
				                                 domainName.StartsWith("domain-") ||
				                                 domainName.StartsWith("NUnit Domain")) &&
				                                Environment.GetEnvironmentVariable(
					                                "TeamCityCheckoutPath") == "true";
				return wasStartedFromNUnitOrTeamCity.Value;
			}
		}

		public static bool StartedFromNUnitConsoleOrReSharper
		{
			get
			{
				var domainName = AppDomain.CurrentDomain.FriendlyName.ToLower();
				return domainName.StartsWith("vstest") || domainName.StartsWith("test-domain-") ||
				       domainName.StartsWith("domain-") || domainName.StartsWith("nunit");
			}
		}

		public static bool StartedFromNCrunchOrNUnitInTeamCity
			=> StartedFromNCrunch || StartedFromNUnitInTeamCity;

		public static bool StartedFromNUnitConsoleButNotFromNCrunch
			=> StartedFromNCrunchOrNUnitInTeamCity && !StartedFromNCrunch;

		/// <summary>
		///   RunAllTestsWithMocks is set by TeamCity to run Tests with MockResolver for faster
		///   DeltaEngine.CI builds. Not used in Nightly builds or when running R# or visual tests.
		/// </summary>
		public static bool StartedFromNCrunchOrForcedToUseMockResolver
		{
			get
			{
				if (wasStartedFromNCrunchOrForcedToUseMockResolver != null)
					return wasStartedFromNCrunchOrForcedToUseMockResolver.Value;
				wasStartedFromNCrunchOrForcedToUseMockResolver = StartedFromNCrunch ||
				                                                 Environment.GetEnvironmentVariable(
					                                                 "RunAllTestsWithMocks") == "true";
				return wasStartedFromNCrunchOrForcedToUseMockResolver.Value;
			}
		}

		public static bool StartedFromProgramMain
			=> !StartedFromNCrunchOrNUnitInTeamCity &&
			   new StackTrace().GetFrames().Any(IsMethodMainOrStart);

		private static bool IsMethodMainOrStart(StackFrame frame)
		{
			var methodName = frame.GetMethod().Name;
			return EntryPointMethodNames.Any(m => m == methodName);
		}

		/// <summary>
		///   Same as stackTraceFrames.Any(frame => IsTestAttribute(frame) || IsInTestSetUp(frame));
		///   but faster, this is called in almost any test, so we want to have this fast.
		/// </summary>
		public static bool IsUnitTest()
		{
			var frames = new StackTrace().GetFrames();
			foreach (var frame in frames)
			foreach (var attribute in frame.GetMethod().GetCustomAttributes(false))
			{
				var attributeName = attribute.GetType().Name;
				if (attributeName == "TestAttribute" || attributeName == "SetUpAttribute")
					return true;
			}

			return false;
		}

		private static bool IsTestAttribute(StackFrame frame)
		{
			return frame.HasAttribute(TestAttribute);
		}

		/// <summary>
		///   When we don't know the attribute type we cannot use Attribute.IsAttribute. Use this instead.
		/// </summary>
		public static bool HasAttribute(this StackFrame frame, string name)
		{
			var attributes = frame.GetMethod().GetCustomAttributes(false);
			return attributes.Any(attribute => attribute.GetType().ToString() == name);
		}

		private static bool IsInTestSetUp(StackFrame frame)
		{
			return frame.HasAttribute(SetUpAttribute) || frame.HasAttribute(FixtureSetUpAttribute);
		}

		/// <summary>
		///   Get entry name from stack frame, which is either the namespace name where the main method
		///   is located or if we are started from a test, the name of the test method. For tests it isn't
		///   the namespace required for the initial scene name, use GetEntryNamespaceForInitialSceneName.
		/// </summary>
		public static string GetEntryName()
		{
			var frames = new StackTrace().GetFrames();
			var testName = GetTestMethodName(frames);
			if (!string.IsNullOrEmpty(testName))
				return testName;
			foreach (var frame in
				frames.Where(frame => EntryPointMethodNames.Any(m => m == frame.GetMethod().Name)))
				return GetNamespaceName(frame);
			return "Delta Engine";
		}

		public static string GetTestMethodName(this StackFrame[] frames)
		{
			foreach (var frame in frames)
			{
				if (IsTestAttribute(frame))
					return frame.GetMethod().Name;
				if (!string.IsNullOrEmpty(unitTestMethodName) && IsInTestSetUp(frame))
					return unitTestMethodName;
			}

			return string.Empty;
		}

		private static string GetNamespaceName(StackFrame frame)
		{
			var classType = frame.GetMethod().DeclaringType;
			return classType != null ? classType.Namespace : "";
		}

		public static string GetEntryNamespaceForInitialSceneName()
		{
			if (string.IsNullOrEmpty(unitTestClassFullName))
				unitTestClassFullName =
					AppDomain.CurrentDomain.GetData(nameof(unitTestClassFullName)) as string;
			if (!string.IsNullOrEmpty(unitTestClassFullName))
				return GetNamespaceNameFromClassFullName(unitTestClassFullName);
			var assembly = Assembly.GetEntryAssembly();
			if (assembly != null)
				return assembly.GetName().Name;
			var frames = new StackTrace().GetFrames();
			return GetNamespaceOrAssemblyNameOrWindowsServiceName(frames);
		}

		/// <summary>
		///   Required to inform the scene domain about which project or test is being started.
		/// </summary>
		public static void SetUnitTestClassFullNameToDomain(AppDomain domain)
		{
			domain.SetData(nameof(unitTestClassFullName), unitTestClassFullName);
		}

		private static string GetNamespaceNameFromClassFullName(string classFullName)
		{
			return Path.GetFileNameWithoutExtension(classFullName);
		}

		private static string GetNamespaceOrAssemblyNameOrWindowsServiceName(StackFrame[] frames)
		{
			foreach (var frame in
				frames.Where(frame => EntryPointMethodNames.Any(m => m == frame.GetMethod().Name)))
				return GetNamespaceName(frame);
			foreach (var frame in frames.Where(IsTestOrTestSetupMethod))
				return frame.GetMethod().DeclaringType.Assembly.GetName().Name;
			if (IsRunningAsWindowsService(frames))
				return GetNamespaceNameForWindowsService(frames);
			return GetExecutingAssemblyName();
		}

		private static string GetExecutingAssemblyName()
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a =>
				!a.GlobalAssemblyCache && !a.GetName().Name.Contains("nCrunch") &&
				!a.GetName().Name.StartsWith("DeltaEngine.Frameworks.") &&
				AssemblyExtensions.IsNotBasicDeltaEngineAssembly(a.GetName().Name)))
				return assembly.GetName().Name;
			throw new ExecutingAssemblyOrNamespaceNotFound();
		}

		private static bool IsTestOrTestSetupMethod(StackFrame frame)
		{
			return IsTestAttribute(frame) || IsInTestSetUp(frame);
		}

		private static bool IsRunningAsWindowsService(StackFrame[] frames)
		{
			return frames.Any(frame => frame.GetMethod().Name == "ServiceQueuedMainCallback");
		}

		private static string GetNamespaceNameForWindowsService(StackFrame[] frames)
		{
			var index = Array.FindIndex(frames,
				frame => frame.GetMethod().Name == "ServiceQueuedMainCallback");
			return frames[index - 1].GetMethod().DeclaringType.Namespace;
		}

		public static string GetApprovalTestName()
		{
			var frames = new StackTrace().GetFrames();
			foreach (var frame in frames)
			{
				if (IsTestAttribute(frame) && frame.HasAttribute(ApproveFirstFrameScreenshotAttribute))
				{
					// Make sure unitTestClassFullName and unitTestMethodName are set correctly for the
					// GetApprovalTestMaxImageDifference call later on when comparing the images.
					SetUnitTestName(frame.GetMethod().DeclaringType.FullName);
					return unitTestClassName + "." + unitTestMethodName;
				}

				if (!string.IsNullOrEmpty(unitTestMethodName) && IsInTestSetUp(frame) &&
				    HasRunningTestAttribute(ApproveFirstFrameScreenshotAttribute))
					return unitTestClassName + "." + unitTestMethodName;
			}

			return "";
		}

		public static float GetApprovalTestMaxImageDifference()
		{
			var testClassType = GetClassTypeFromRunningAssemblies(unitTestClassFullName);
			var method = testClassType?.GetMethod(unitTestMethodName);
			if (method == null)
				return 0;
			var attributes = method.GetCustomAttributes(false);
			var attribute = attributes.FirstOrDefault(
				a => a.GetType().ToString() == ApproveFirstFrameScreenshotAttribute);
			if (attribute != null)
				return (float) attribute.GetType().GetProperty("MaxImageDifference")
					.GetValue(attribute, null);
			return 0;
		}

		public static string GetClassName(this StackFrame[] frames)
		{
			foreach (var frame in frames.Where(IsTestAttribute))
				return frame.GetMethod().DeclaringType.Name;
			return string.Empty;
		}

		private static bool HasRunningTestAttribute(string attributeFullName)
		{
			var testClassType = GetClassTypeFromRunningAssemblies(unitTestClassFullName);
			var method = testClassType?.GetMethod(unitTestMethodName);
			if (method == null)
				return false;
			var attributes = method.GetCustomAttributes(false);
			return attributes.FirstOrDefault(a => a.GetType().ToString() == attributeFullName) != null ||
			       DoesSetupMethodHaveAttribute(attributeFullName, testClassType);
		}

		private static Type GetClassTypeFromRunningAssemblies(string classFullName)
		{
			var runningAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in runningAssemblies)
				if (classFullName.StartsWith(assembly.GetName().Name))
					return assembly.GetType(classFullName);
			return null;
		}

		private static bool DoesSetupMethodHaveAttribute(string attributeFullName, Type testClassType)
		{
			foreach (var anyMethod in testClassType.GetMethods())
			{
				var allAttributes = anyMethod.GetCustomAttributes(false);
				if (allAttributes.Any(a => a.GetType().ToString() == SetUpAttribute) &&
				    allAttributes.Any(a => a.GetType().ToString() == attributeFullName))
					return true;
			}

			return false;
		}

		public static bool IsCloseAfterFirstFrameAttributeUsed()
		{
			return IsAttributeUsed("DeltaEngine.Mocks.Resolvers.CloseAfterFirstFrameAttribute");
		}

		private static bool IsAttributeUsed(string attributeTypeFullName)
		{
			var frames = new StackTrace().GetFrames();
			foreach (var frame in frames)
			{
				if ((IsTestAttribute(frame) || IsInTestSetUp(frame)) &&
				    frame.HasAttribute(attributeTypeFullName))
					return true;
				if (!string.IsNullOrEmpty(unitTestMethodName) && IsInTestSetUp(frame))
					return HasRunningTestAttribute(attributeTypeFullName);
			}

			return false;
		}

		public static bool IsCreateEntityInstancesAttributeUsed()
		{
			return IsAttributeUsed(
				"DeltaEngine.Mocks.Resolvers.CreateInitialEntitiesLikeInNormalRunAttribute");
		}

		public static bool IsApproveFirstFrameScreenshotAttributeUsed()
		{
			return IsAttributeUsed(ApproveFirstFrameScreenshotAttribute);
		}

		/// <summary>
		///   Since we do not initialize or run the resolver in a test, we need to set the current unit
		///   test name up beforehand so we can find out if the test uses ApproveFirstFrameScreenshot.
		/// </summary>
		public static void SetUnitTestName(string fullTestMethodName)
		{
			var fullNameWithoutArguments = fullTestMethodName.Split('(').First();
			var nameParts = fullNameWithoutArguments.Split('.');
			if (nameParts.Length < 3)
				throw new InvalidFullMethodNameDoNotJustSubmitTheAssemblyName(fullTestMethodName);
			unitTestMethodName = nameParts[nameParts.Length - 1];
			unitTestClassName = nameParts[nameParts.Length - 2];
			unitTestClassFullName = nameParts[0];
			for (var num = 1; num < nameParts.Length - 1; num++)
				unitTestClassFullName += "." + nameParts[num];
		}

		/// <summary>
		///   Shows the callstack as multiline text output to help figure out who called what. Removes the
		///   first callstack line (this method) and all non-helpful System, NUnit and nCrunch lines.
		/// </summary>
		public static string FormatStackTraceIntoClickableMultilineText(int stackFramesToSkip = 0)
		{
			var output = "";
			foreach (var frame in new StackTrace(true).GetFrames().Skip(1 + stackFramesToSkip))
				if (!IsSystemOrTestMethodToExclude(frame.GetMethod()))
					output += "   at " + GetMethodWithParameters(frame.GetMethod()) +
					          GetFilenameAndLineInfo(frame) + "\n";
			// If there is a stacktrace, it will always end with a newline. This is important to create
			// valid text output for Asserts, which are clickable via NCrunch and Resharper runners.
			// However this empty line might be annoying for files and is removed in TextFileLogger.
			return output;
		}

		private static bool IsSystemOrTestMethodToExclude(MethodBase method)
		{
			return method == null || method.DeclaringType == null ||
			       method.DeclaringType.FullName.StartsWith(MethodNamesToExclude) ||
			       method.DeclaringType.Assembly.GetName().Name.StartsWith("nCrunch");
		}

		private static string GetMethodWithParameters(MethodBase method)
		{
			return method == null
				? ""
				: method.DeclaringType + "." + method.Name + "(" + GetParameters(method) + ")";
		}

		private static string GetParameters(MethodBase method)
		{
			var parametersText = "";
			foreach (var parameter in method.GetParameters())
				parametersText += (parametersText.Length > 0 ? ", " : "") + parameter.ParameterType + " " +
				                  parameter.Name;
			return parametersText;
		}

		private static string GetFilenameAndLineInfo(StackFrame frame)
		{
			var filename = frame.GetFileName();
			var lineNumber = frame.GetFileLineNumber();
			if (string.IsNullOrEmpty(filename) || lineNumber == 0)
				return "";
			return " in " + filename + ":line " + lineNumber;
		}

		public static string FormatExceptionToTypeAndMessage(Exception exception)
		{
			return exception.GetType().Name.SplitWords(true).ToText(" ") + " " + exception.Message;
		}

		public static string FormatExceptionIntoClickableMultilineText(Exception exception)
		{
			var messageLines = exception.ToString().SplitAndTrim('\n');
			var output = "";
			foreach (var line in messageLines)
				if (line.Trim().StartsWith("at ") || line.Trim().StartsWith("bei ") ||
				    line.Contains(" DeltaEngine."))
					output = FormatLineWithSourceLocation(line, output);
				else if (line !=
				         "--- End of stack trace from previous location where exception was thrown ---")
					output += (output.Length > 0 ? "\n" : "") + line;
			return output;
		}

		private static string FormatLineWithSourceLocation(string line, string output)
		{
			var trimmedLine = line.Trim();
			var removeFirstWord = line.Substring(trimmedLine.IndexOf(' ')).TrimStart();
			var skipLine = false;
			foreach (var nameToExclude in MethodNamesToExclude)
				if (removeFirstWord.StartsWith(nameToExclude))
				{
					skipLine = true;
					break;
				}

			if (!skipLine)
				output += (output.Length > 0 ? "\n   " : "") + trimmedLine;
			return output;
		}

		public static string GetCallerMethodName(int stackFramesToSkip)
		{
			return new StackTrace(true).GetFrames().Skip(1 + stackFramesToSkip).First().GetMethod().Name;
		}

		public static Version GetEntryAssemblyVersion()
		{
			return GetEntryAssembly().GetName().Version;
		}

		public static Assembly GetEntryAssembly()
		{
			return Assembly.GetEntryAssembly() ?? FindEntryAssemblyByWalkingUpTheStack();
		}

		private static Assembly FindEntryAssemblyByWalkingUpTheStack()
		{
			var methodFrames = new StackTrace().GetFrames().Select(t => t.GetMethod()).ToArray();
			MethodBase entryMethod = null;
			var firstInvokeMethod = 0;
			for (var i = 0; i < methodFrames.Length; i++)
			{
				var method = methodFrames[i] as MethodInfo;
				if (method == null)
					continue;
				if (IsMainEntryPoint(method))
					entryMethod = method;
				else if (firstInvokeMethod == 0 && WasInvokedViaReflection(method))
					firstInvokeMethod = i;
			}

			if (!string.IsNullOrEmpty(unitTestClassFullName))
				return GetAssemblyFromNamespace(GetNamespaceNameFromClassFullName(unitTestClassFullName));
			if (entryMethod == null)
				entryMethod = firstInvokeMethod != 0
					? methodFrames[firstInvokeMethod - 1]
					: methodFrames.Last();
			return entryMethod.Module.Assembly;
		}

		private static bool IsMainEntryPoint(MethodInfo method)
		{
			return method.Name == "Main" && method.ReturnType == typeof(void);
		}

		private static bool WasInvokedViaReflection(MethodBase method)
		{
			return method.Name == "InvokeMethod" && method.IsStatic &&
			       method.DeclaringType == typeof(RuntimeMethodHandle);
		}

		private static Assembly GetAssemblyFromNamespace(string namespaceName)
		{
			do
			{
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
					if (assembly.GetName().Name == namespaceName)
						return assembly;
				namespaceName = Path.GetFileNameWithoutExtension(namespaceName);
			} while (!string.IsNullOrEmpty(namespaceName));

			throw new ExecutingAssemblyOrNamespaceNotFound();
		}

		private class ExecutingAssemblyOrNamespaceNotFound : Exception
		{
		}

		public class InvalidFullMethodNameDoNotJustSubmitTheAssemblyName : Exception
		{
			public InvalidFullMethodNameDoNotJustSubmitTheAssemblyName(string fullTestMethodName)
				: base(fullTestMethodName)
			{
			}
		}
	}
}
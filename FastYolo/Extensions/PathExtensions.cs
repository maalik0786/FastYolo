using System;
using System.IO;

namespace FastYolo.Extensions
{
	/// <summary>
	///   Helps finding common paths to .NET, the MyDocuments folder (which is the current folder for
	///   services, NCrunch or when we got no access rights) and provides several path helper methods.
	/// </summary>
	public static class PathExtensions
	{
		private const string DeltaEngineSolutionFilename = "DeltaEngine.sln";

		public static string GetMyDocumentsAppFolder()
		{
			if (StackTraceExtensions.StartedFromNCrunchOrForcedToUseMockResolver ||
			    !Environment.UserInteractive)
				return Directory.GetCurrentDirectory();
			//ncrunch: no coverage start
			var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			if (string.IsNullOrEmpty(documents))
				return Directory.GetCurrentDirectory();
			var appPath = Path.Combine(documents, "DeltaEngine",
				StackTraceExtensions.GetEntryNamespaceForInitialSceneName());
			if (!Directory.Exists(appPath))
				Directory.CreateDirectory(appPath);
			return appPath;
		} //ncrunch: no coverage end

		public static string GetOriginalSolutionPath()
		{
			var nCrunchOriginalSolutionFilePath =
				Environment.GetEnvironmentVariable("NCrunch.OriginalSolutionPath");
			if (!string.IsNullOrEmpty(nCrunchOriginalSolutionFilePath))
				return Path.GetDirectoryName(nCrunchOriginalSolutionFilePath);
			//ncrunch: no coverage start
			var teamCityCheckoutPath = Environment.GetEnvironmentVariable("TeamCityCheckoutPath");
			if (!string.IsNullOrEmpty(teamCityCheckoutPath))
				return teamCityCheckoutPath;
			// Should never go here except for Debug mode in VS Editor where we check for outdated dlls
			var fallbackCodePath = @"c:\code\DeltaEngine";
			if (Directory.Exists(fallbackCodePath))
				return fallbackCodePath;
			return GetSolutionFolderByCheckingForPackagesSubFolder(AppDomain.CurrentDomain.BaseDirectory);
		} //ncrunch: no coverage end

		internal static string GetSolutionFolderByCheckingForPackagesSubFolder(string currentPath,
			bool lookForDeltaEngineProject = true)
		{
			var path = GetAbsolutePath(Path.Combine(currentPath, "..", ".."));
			var lastAbsolutePath = path;
			while (!Directory.Exists(Path.Combine(path, "packages")) ||
			       lookForDeltaEngineProject && !File.Exists(Path.Combine(path, "DeltaEngine.csproj")))
			{
				path = GetAbsolutePath(Path.Combine(path, ".."));
				if (path != lastAbsolutePath)
					lastAbsolutePath = path;
				else if (lookForDeltaEngineProject)
					return GetSolutionFolderByCheckingForPackagesSubFolder(currentPath, false);
				else
					throw new UnableToFindNuGetPackagesFolder(currentPath);
			}

			return path;
		}

		public static void CreateDirectoryIfNotExists(string path)
		{
			if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
				Directory.CreateDirectory(path);
		}

		//ncrunch: no coverage start, slow helper methods
		/// <summary>
		///   Reference: http://msdn.microsoft.com/en-us/library/y549e41e.aspx
		/// </summary>
		public static string GetDotNetFrameworkPath(Version runtimeVersion)
		{
			return Path.Combine(GetDotNetPath(), "Framework",
				CutOffTailingNulls("v" + RemoveRevisionFromVersion(runtimeVersion)));
		}

		private static string GetDotNetPath()
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
				"Microsoft.NET");
		}

		private static string CutOffTailingNulls(string versionText)
		{
			if (versionText.EndsWith(".0.0"))
				return versionText.Substring(0, versionText.Length - ".0.0".Length);
			if (versionText.EndsWith(".0"))
				return versionText.Substring(0, versionText.Length - ".0".Length);
			return versionText;
		}

		private static Version RemoveRevisionFromVersion(Version runtimeVersion)
		{
			return new Version(runtimeVersion.Major, runtimeVersion.Minor, runtimeVersion.Build);
		}
		//ncrunch: no coverage end

		public static string GetAbsolutePath(string directoryOrFilePath)
		{
			if (string.IsNullOrEmpty(directoryOrFilePath))
				return Environment.CurrentDirectory;
			return Path.GetFullPath(directoryOrFilePath);
		}

		//ncrunch: no coverage start
		public static string GetPathOnDesktop(string folderNameOnDesktop)
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
				"Desktop", folderNameOnDesktop);
		}
		//ncrunch: no coverage end

		public static string MakeRelativePath(string fromPath, string toPath)
		{
			if (fromPath.StartsWith(toPath))
				return fromPath.Substring(toPath.Length +
				                          (toPath.EndsWith(Path.DirectorySeparatorChar + "") ? 0 : 1));
			if (!fromPath.EndsWith(Path.DirectorySeparatorChar + ""))
				fromPath += Path.DirectorySeparatorChar;
			var fromUri = new Uri(fromPath);
			var toUri = new Uri(toPath);
			var relativeUri = fromUri.MakeRelativeUri(toUri);
			var relativePath = Uri.UnescapeDataString(relativeUri.ToString());
			return toUri.Scheme.ToUpperInvariant() == "FILE"
				? relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
				: relativePath;
		}

		public static string GetDeltaEngineSolutionFilePath()
		{
			return File.Exists(Path.Combine(GetOriginalSolutionPath(), DeltaEngineSolutionFilename))
				? Path.Combine(GetOriginalSolutionPath(), DeltaEngineSolutionFilename)
				: Path.Combine(GetOriginalSolutionPath(), "..", "DeltaEngine", DeltaEngineSolutionFilename);
		}

		public static string GetDeltaEngineSolutionPath()
		{
			return Path.GetDirectoryName(GetDeltaEngineSolutionFilePath());
		}

		public class UnableToFindNuGetPackagesFolder : Exception
		{
			public UnableToFindNuGetPackagesFolder(string currentPath) : base(currentPath)
			{
			}
		}
	}
}
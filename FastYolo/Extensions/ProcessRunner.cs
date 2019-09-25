using System;
using System.Diagnostics;
using System.Threading;

namespace FastYolo.Extensions
{
	/// <summary>
	///   Starts a command line process with given argument and optional timeout. Supports events or
	///   can be used synchronously with checking Error or Output afterwards. Exceptions are thrown
	///   when things go bad (ExitCode not 0 or process times out).
	/// </summary>
	public sealed class ProcessRunner : IDisposable
	{
		private const int DefaultTwoMinuteTimeout = 60 * 1000 * 2;

		public static readonly int OneMinuteTimeout = 60 * 1000;
		public static readonly int NoTimeout = -1;
		private readonly int timeoutInMs;
		private AutoResetEvent errorWaitHandle;

		private Process nativeProcess;

		private AutoResetEvent outputWaitHandle;

		//ncrunch: no coverage start
		public ProcessRunner(string filePath, string argumentsLine = "",
			int timeoutInMs = DefaultTwoMinuteTimeout)
		{
			FilePath = filePath;
			ArgumentsLine = argumentsLine;
			this.timeoutInMs = timeoutInMs;
			WorkingDirectory = Environment.CurrentDirectory;
			IsWaitingForExit = true;
			IsExitCodeRelevant = true;
			Output = "";
			Errors = "";
		}

		public string FilePath { get; }
		public string ArgumentsLine { get; }
		public string WorkingDirectory { get; set; }
		public bool IsWaitingForExit { get; set; }
		public bool IsExitCodeRelevant { get; set; }
		public string Output { get; private set; }
		public string Errors { get; private set; }

		public void Dispose()
		{
			if (nativeProcess == null)
				return;
			outputWaitHandle.Dispose();
			errorWaitHandle.Dispose();
			nativeProcess.Dispose();
			nativeProcess = null;
		}

		public event Action<string> StandardOutputEvent;
		public event Action<string> ErrorOutputEvent;

		public void Start()
		{
			nativeProcess = new Process();
			SetupStartInfo();
			InitializeProcessOutputStreams();
			if (IsWaitingForExit)
				StartNativeProcessAndWaitForExit();
			else
				StartNativeProcess();
		}

		/// <summary>
		///   Use useful arguments for redirecting the standard output and error steams to us. Also:
		///   http://stackoverflow.com/questions/5702340/process-start-is-blocking-hanging-randomly-on-some-clients
		/// </summary>
		private void SetupStartInfo()
		{
			nativeProcess.StartInfo.FileName = FilePath;
			nativeProcess.StartInfo.Arguments = ArgumentsLine;
			nativeProcess.StartInfo.WorkingDirectory = WorkingDirectory;
			nativeProcess.StartInfo.CreateNoWindow = true;
			nativeProcess.StartInfo.UseShellExecute = false;
			nativeProcess.StartInfo.RedirectStandardOutput = true;
			nativeProcess.StartInfo.RedirectStandardError = true;
		}

		/// <summary>
		///   Helpful post how to avoid the possible deadlock of a process
		///   http://stackoverflow.com/questions/139593/processstartinfo-hanging-on-waitforexit-why
		/// </summary>
		private void InitializeProcessOutputStreams()
		{
			outputWaitHandle = new AutoResetEvent(false);
			errorWaitHandle = new AutoResetEvent(false);
			nativeProcess.OutputDataReceived += OnStandardOutputDataReceived;
			nativeProcess.ErrorDataReceived += OnErrorOutputDataReceived;
		}

		private void OnStandardOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (string.IsNullOrEmpty(e.Data))
			{
				outputWaitHandle.Set();
				return;
			}

			StandardOutputEvent?.Invoke(e.Data);
			Output += e.Data + Environment.NewLine;
		}

		private void OnErrorOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (string.IsNullOrEmpty(e.Data))
			{
				errorWaitHandle.Set();
				return;
			}

			ErrorOutputEvent?.Invoke(e.Data);
			Errors += e.Data + Environment.NewLine;
		}

		private void StartNativeProcessAndWaitForExit()
		{
			StartNativeProcess();
			WaitForExit();
			if (IsExitCodeRelevant)
				CheckExitCode();
		}

		private void StartNativeProcess()
		{
			try
			{
				nativeProcess.Start();
				nativeProcess.BeginOutputReadLine();
				nativeProcess.BeginErrorReadLine();
			}
			catch (Exception ex)
			{
				throw new FailedToRunProcess(FilePath, ArgumentsLine, ex);
			}
		}

		private void WaitForExit()
		{
			if (!outputWaitHandle.WaitOne(timeoutInMs))
				throw new StandardOutputHasTimedOutException(FilePath, ArgumentsLine);
			if (!errorWaitHandle.WaitOne(timeoutInMs))
				throw new ErrorOutputHasTimedOutException(FilePath, ArgumentsLine);
			if (!nativeProcess.WaitForExit(timeoutInMs))
				throw new ProcessHasTimedOutException(FilePath, ArgumentsLine);
		}

		private void CheckExitCode()
		{
			if (nativeProcess.ExitCode != 0)
				throw new ProcessTerminatedWithError(Errors + "\n" + Output + "\n" + ArgumentsLine);
		}

		public string GetErrorsOrOutput()
		{
			return string.IsNullOrEmpty(Errors) ? Output : Errors;
		}

		private class FailedToRunProcess : Exception
		{
			public FailedToRunProcess(string filePath, string arguments, Exception inner)
				: base(filePath + " " + arguments, inner)
			{
			}
		}

		public class StandardOutputHasTimedOutException : Exception
		{
			public StandardOutputHasTimedOutException(string filePath, string arguments)
				: base(filePath + " " + arguments)
			{
			}
		}

		public class ErrorOutputHasTimedOutException : Exception
		{
			public ErrorOutputHasTimedOutException(string filePath, string arguments)
				: base(filePath + " " + arguments)
			{
			}
		}

		public class ProcessHasTimedOutException : Exception
		{
			public ProcessHasTimedOutException(string filePath, string arguments)
				: base(filePath + " " + arguments)
			{
			}
		}

		public class ProcessTerminatedWithError : Exception
		{
			public ProcessTerminatedWithError(string errors) : base(errors)
			{
			}
		}
	}
}
using System;
using System.Threading;

namespace FastYolo.Extensions
{
	/// <summary>
	///   Useful wrapper functions for threading.
	/// </summary>
	public static class ThreadExtensions
	{
		public static Thread Start(this Action threadRunCode)
		{
			var newThread = new Thread(new ThreadStart(threadRunCode));
			newThread.Start();
			return newThread;
		}

		public static bool EnqueueWorkerThread(Action threadAction)
		{
			return ThreadPool.QueueUserWorkItem(state => threadAction());
		}

		//ncrunch: no coverage start
		public static Thread StartAfterDelay(this Action threadRunCode, int delayInMs)
		{
			var newThread = new Thread(() =>
			{
				Thread.Sleep(delayInMs);
				threadRunCode();
			});
			newThread.Start();
			return newThread;
		} //ncrunch: no coverage end

		public static Thread Start(string threadName, Action threadRunCode)
		{
			var newThread = new Thread(new ThreadStart(threadRunCode)) {Name = threadName};
			newThread.Start();
			return newThread;
		}

		public static Thread Start(this Action<object> threadRunCode, object param)
		{
			var newThread = new Thread(new ParameterizedThreadStart(threadRunCode));
			newThread.Start(param);
			return newThread;
		}

		public static void EnqueueWorkerThread<T>(T argument1, Action<T> threadAction)
		{
			ThreadPool.QueueUserWorkItem(state => threadAction((T) state), argument1);
		}

		public static void EnqueueWorkerThread<T1, T2>(T1 argument1, T2 argument2,
			Action<T1, T2> threadAction)
		{
			EnqueueWorkerThread(new {Arg1 = argument1, Arg2 = argument2},
				args => threadAction(args.Arg1, args.Arg2));
		}
	}
}
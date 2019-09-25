using System;
using System.Collections.Generic;
using System.Threading;

namespace FastYolo.Extensions
{
	/// <summary>
	///   Provides an object which can be scoped and is static within a thread (e.g. tests). Based on
	///   http://startbigthinksmall.wordpress.com/2008/04/24/nice-free-and-reusable-net-ambient-context-pattern-implementation/
	/// </summary>
	public class ThreadStatic<T>
	{
		[ThreadStatic] private static Dictionary<ThreadStatic<T>, ThreadStaticValue> threadStatics;

		private readonly T fallback;
		private readonly bool isFallbackDefined;

		private ThreadStaticValue innerMost;

		public ThreadStatic()
		{
		}

		public ThreadStatic(T fallback)
		{
			this.fallback = fallback;
			isFallbackDefined = true;
		}

		public T CurrentOrDefault => HasCurrent ? Current : default;
		public bool HasCurrent => isFallbackDefined || ThreadStatics.ContainsKey(this);

		private static Dictionary<ThreadStatic<T>, ThreadStaticValue> ThreadStatics
			=> threadStatics ?? (threadStatics = new Dictionary<ThreadStatic<T>, ThreadStaticValue>(1));

		public T Current
		{
			get
			{
				ThreadStatics.TryGetValue(this, out var current);
				if (current != null)
					return current.value;
				if (isFallbackDefined)
					return fallback;
				throw new NoValueAvailable();
			}
		}

		// ReSharper disable once MethodNameNotMeaningful
		public IDisposable Use(T value)
		{
			ThreadStatics.TryGetValue(this, out var old);
			return ThreadStatics[this] = new ThreadStaticValue(this, value, old);
		}

		private void DisposeScope(ThreadStaticValue valueToDispose)
		{
			if (Thread.CurrentThread.ManagedThreadId != valueToDispose.threadId)
				throw new DisposingOnDifferentThreadToCreation();
			ThreadStatics.TryGetValue(this, out innerMost);
			while (innerMost != valueToDispose)
				DisposeInnerMostScopes();
			DisposeCurrentScope(valueToDispose);
		}

		private void DisposeInnerMostScopes()
		{
			innerMost.MarkAsDisposed();
			innerMost = innerMost.previous;
		}

		private void DisposeCurrentScope(ThreadStaticValue valueToDispose)
		{
			valueToDispose.MarkAsDisposed();
			if (valueToDispose.previous == null)
				ThreadStatics.Remove(this);
			else
				ThreadStatics[this] = valueToDispose.previous;
		}

		public class NoValueAvailable : Exception
		{
		}

		private sealed class ThreadStaticValue : IDisposable
		{
			private readonly ThreadStatic<T> key;
			internal readonly ThreadStaticValue previous;

			internal readonly int threadId;
			internal readonly T value;

			private bool isDisposed;

			internal ThreadStaticValue(ThreadStatic<T> key, T value, ThreadStaticValue previous)
			{
				threadId = Thread.CurrentThread.ManagedThreadId;
				this.key = key;
				this.value = value;
				this.previous = previous;
			}

			public void Dispose()
			{
				if (!isDisposed)
					key.DisposeScope(this);
			}

			internal void MarkAsDisposed()
			{
				isDisposed = true;
				if (!StackTraceExtensions.StartedFromNCrunchOrNUnitInTeamCity)
					// ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
					GC.SuppressFinalize(this); //ncrunch: no coverage
			}
		}

		public class DisposingOnDifferentThreadToCreation : Exception
		{
		}
	}
}
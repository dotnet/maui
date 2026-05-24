#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>A disposable struct for profiling code execution.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CA1815 // Override equals and operator equals on value types
	public struct Profile : IDisposable
	{
		const int Capacity = 1000;

		[DebuggerDisplay("{Name,nq} {Id} {Ticks}")]
		public struct Datum
#pragma warning restore CA1815 // Override equals and operator equals on value types
		{
			public string Name;
			public string Id;
			public long Ticks;
			public int Depth;
			public int Line;
		}
		/// <summary>Gets the collected profiling data.</summary>
		public static List<Datum> Data;

		/// <summary>Gets whether profiling is enabled.</summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool IsEnabled { get; private set; } = false;

		static Stack<Profile> Stack;
		static int Depth = 0;
		static bool Running = false;
		static Stopwatch Stopwatch;

		readonly long _start;
		readonly string _name;
		readonly int _slot;

		/// <summary>Enables profiling and initializes data structures.</summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Enable()
		{
			if (!IsEnabled)
			{
				IsEnabled = true;
				Data = new List<Datum>(Capacity);
				Stack = new Stack<Profile>(Capacity);
				Stopwatch = new Stopwatch();
			}
		}

		/// <summary>Starts profiling.</summary>
		public static void Start()
		{
			if (!IsEnabled)
				return;

			Running = true;
		}

		/// <summary>Stops profiling and unwinds the stack.</summary>
		public static void Stop()
		{
			if (!IsEnabled)
				return;

			// unwind stack
			Running = false;
			while (Stack.Count > 0)
				Stack.Pop();
		}

		/// <summary>Begins a new profiling frame.</summary>
		public static void FrameBegin(
			[CallerMemberName] string name = "",
			[CallerLineNumber] int line = 0)
		{
			if (!IsEnabled || !Running)
				return;

			FrameBeginBody(name, null, line);
		}

		/// <summary>Ends the current profiling frame.</summary>
		public static void FrameEnd(
			[CallerMemberName] string name = "")
		{
			if (!IsEnabled || !Running)
				return;

			FrameEndBody(name);
		}

		/// <summary>Creates a partition within the current frame.</summary>
		public static void FramePartition(
			string id,
			[CallerLineNumber] int line = 0)
		{
			if (!IsEnabled || !Running)
				return;

			FramePartitionBody(id, line);
		}

		static void FrameBeginBody(
			string name,
			string id,
			int line)
		{
			if (!Stopwatch.IsRunning)
				Stopwatch.Start();

			Stack.Push(new Profile(name, id, line));
		}

		static void FrameEndBody(string name)
		{
			var profile = Stack.Pop();
			if (profile._name != name)
				throw new InvalidOperationException(
					$"Expected to end frame '{profile._name}', not '{name}'.");
			profile.Dispose();
		}

		static void FramePartitionBody(
			string id,
			int line)
		{
			var profile = Stack.Pop();
			var name = profile._name;
			profile.Dispose();

			FrameBeginBody(name, id, line);
		}

		Profile(
			string name,
			string id,
			int line)
		{
			this = default(Profile);
			_start = Stopwatch.ElapsedTicks;

			_name = name;

			_slot = Data.Count;
			Data.Add(new Datum()
			{
				Depth = Depth,
				Name = name,
				Id = id,
				Ticks = -1,
				Line = line
			});

			Depth++;
		}

		/// <summary>Disposes the profile and records the elapsed time.</summary>
		public void Dispose()
		{
			if (!IsEnabled)
				return;
			if (Running && _start == 0)
				return;

			var ticks = Stopwatch.ElapsedTicks - _start;
			--Depth;

			var datum = Data[_slot];
			datum.Ticks = ticks;
			Data[_slot] = datum;
		}
	}
}

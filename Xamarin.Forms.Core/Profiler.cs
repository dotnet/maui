using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public struct Profile : IDisposable
	{
		const int Capacity = 1000;

		[DebuggerDisplay("{Name,nq} {Id} {Ticks}")]
		public struct Datum
		{
			public string Name;
			public string Id;
			public long Ticks;
			public int Depth;
			public int Line;
		}
		public static List<Datum> Data;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool IsEnabled { get; private set; } = false;

		static Stack<Profile> Stack;
		static int Depth = 0;
		static bool Running = false;
		static Stopwatch Stopwatch;

		readonly long _start;
		readonly string _name;
		readonly int _slot;

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

		public static void Start()
		{
			if (!IsEnabled)
				return;

			Running = true;
		}

		public static void Stop()
		{
			if (!IsEnabled)
				return;

			// unwind stack
			Running = false;
			while (Stack.Count > 0)
				Stack.Pop();
		}

		public static void FrameBegin(
			[CallerMemberName] string name = "",
			[CallerLineNumber] int line = 0)
		{
			if (!IsEnabled || !Running)
				return;

			FrameBeginBody(name, null, line);
		}

		public static void FrameEnd(
			[CallerMemberName] string name = "")
		{
			if (!IsEnabled || !Running)
				return;

			FrameEndBody(name);
		}

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
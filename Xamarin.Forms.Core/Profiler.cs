using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

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
		public static List<Datum> Data = new List<Datum>(Capacity);

		static FormsStack<Profile> Stack = new FormsStack<Profile>(Capacity);
		static int Depth = 0;
		static bool Running = false;
		static Stopwatch Stopwatch = new Stopwatch();

		readonly long _start;
		readonly string _name;
		readonly int _slot;

		public static void Start() 
		{
			Running = true;
		}

		public static void Stop()
		{
			// unwind stack
			Running = false;
			while (Stack.Count > 0)
				Stack.Pop();
		}

		public static void FrameBegin(
			[CallerMemberName] string name = "",
			[CallerLineNumber] int line = 0)
		{
			if (!Running)
				return;

			FrameBeginBody(name, null, line);
		}

		public static void FrameEnd(
			[CallerMemberName] string name = "")
		{
			if (!Running)
				return;

			FrameEndBody(name);
		}

		public static void FramePartition(
			string id,
			[CallerLineNumber] int line = 0)
		{
			if (!Running)
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

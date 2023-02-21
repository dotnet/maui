#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Profile.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.Profile']/Docs/*" />
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
		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Profile.xml" path="//Member[@MemberName='Data']/Docs/*" />
		public static List<Datum> Data;

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Profile.xml" path="//Member[@MemberName='IsEnabled']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool IsEnabled { get; private set; } = false;

		static Stack<Profile> Stack;
		static int Depth = 0;
		static bool Running = false;
		static Stopwatch Stopwatch;

		readonly long _start;
		readonly string _name;
		readonly int _slot;

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Profile.xml" path="//Member[@MemberName='Enable']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Profile.xml" path="//Member[@MemberName='Start']/Docs/*" />
		public static void Start()
		{
			if (!IsEnabled)
				return;

			Running = true;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Profile.xml" path="//Member[@MemberName='Stop']/Docs/*" />
		public static void Stop()
		{
			if (!IsEnabled)
				return;

			// unwind stack
			Running = false;
			while (Stack.Count > 0)
				Stack.Pop();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Profile.xml" path="//Member[@MemberName='FrameBegin']/Docs/*" />
		public static void FrameBegin(
			[CallerMemberName] string name = "",
			[CallerLineNumber] int line = 0)
		{
			if (!IsEnabled || !Running)
				return;

			FrameBeginBody(name, null, line);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Profile.xml" path="//Member[@MemberName='FrameEnd']/Docs/*" />
		public static void FrameEnd(
			[CallerMemberName] string name = "")
		{
			if (!IsEnabled || !Running)
				return;

			FrameEndBody(name);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Profile.xml" path="//Member[@MemberName='FramePartition']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Profile.xml" path="//Member[@MemberName='Dispose']/Docs/*" />
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

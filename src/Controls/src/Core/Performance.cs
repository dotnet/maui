#nullable disable
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IPerformanceProvider
	{
		void Stop(string reference, string tag, string path, string member);

		void Start(string reference, string tag, string path, string member);
	}

	/// <summary>For internal use by the Microsoft.Maui.Controls platform.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class Performance
	{
		static long Reference;

		/// <summary>For internal use by the Microsoft.Maui.Controls platform.</summary>
		public static IPerformanceProvider Provider { get; private set; }

		/// <summary>For internal use by the Microsoft.Maui.Controls platform.</summary>
		/// <param name="instance">For internal use by the Microsoft.Maui.Controls platform.</param>
		public static void SetProvider(IPerformanceProvider instance)
		{
			Provider = instance;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Performance.xml" path="//Member[@MemberName='Start'][1]/Docs/*" />
		public static void Start(out string reference, string tag = null, [CallerFilePath] string path = null, [CallerMemberName] string member = null)
		{
			if (Provider == null)
			{
				reference = String.Empty;
				return;
			}

			reference = Interlocked.Increment(ref Reference).ToString();
			Provider.Start(reference, tag, path, member);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Performance.xml" path="//Member[@MemberName='Start'][2]/Docs/*" />
		public static void Start(string reference, string tag = null, [CallerFilePath] string path = null,
			[CallerMemberName] string member = null)
		{
			Provider?.Start(reference, tag, path, member);
		}

		/// <summary>For internal use by the Microsoft.Maui.Controls platform.</summary>
		/// <param name="reference">For internal use by the Microsoft.Maui.Controls platform.</param>
		/// <param name="tag">For internal use by the Microsoft.Maui.Controls platform.</param>
		/// <param name="path">For internal use by the Microsoft.Maui.Controls platform.</param>
		/// <param name="member">For internal use by the Microsoft.Maui.Controls platform.</param>
		public static void Stop(string reference, string tag = null, [CallerFilePath] string path = null, [CallerMemberName] string member = null)
		{
			Provider?.Stop(reference, tag, path, member);
		}

		internal static IDisposable StartNew(string tag = null, [CallerFilePath] string path = null, [CallerMemberName] string member = null)
		{
			return new DisposablePerformanceReference(tag, path, member);
		}

		class DisposablePerformanceReference : IDisposable
		{
			string _reference;
			string _tag;
			string _path;
			string _member;

			public DisposablePerformanceReference(string tag, string path, string member)
			{
				_tag = tag;
				_path = path;
				_member = member;
				Start(out string reference, _tag, _path, _member);
				_reference = reference;
			}

			public void Dispose()
			{
				Stop(_reference, _tag, _path, _member);
			}
		}
	}
}

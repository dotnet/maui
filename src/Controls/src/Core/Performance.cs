#nullable disable
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This type is obsolete and will be removed in a future version.", true)]
	public interface IPerformanceProvider
	{
		void Stop(string reference, string tag, string path, string member);

		void Start(string reference, string tag, string path, string member);
	}

	/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
	/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This type is obsolete and will be removed in a future version.", true)]
	public class Performance
	{
		static long Reference;

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		public static IPerformanceProvider Provider { get; private set; }

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		/// <param name="instance">Internal parameter for platform use.</param>
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

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		/// <param name="reference">Internal parameter for platform use.</param>
		/// <param name="tag">Internal parameter for platform use.</param>
		/// <param name="path">Internal parameter for platform use.</param>
		/// <param name="member">Internal parameter for platform use.</param>
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

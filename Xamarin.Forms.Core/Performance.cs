using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IPerformanceProvider
	{
		void Stop(string reference, string tag, string path, string member);

		void Start(string reference, string tag, string path, string member);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public class Performance
	{
		static long Reference;

		public static IPerformanceProvider Provider { get; private set; }

		public static void SetProvider(IPerformanceProvider instance)
		{
			Provider = instance;
		}

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

		public static void Start(string reference, string tag = null, [CallerFilePath] string path = null,
			[CallerMemberName] string member = null)
		{
			Provider?.Start(reference, tag, path, member);
		}

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
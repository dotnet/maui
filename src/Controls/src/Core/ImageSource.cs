#nullable disable
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ImageSource.xml" path="Type[@FullName='Microsoft.Maui.Controls.ImageSource']/Docs/*" />
	[System.ComponentModel.TypeConverter(typeof(ImageSourceConverter))]
	public abstract partial class ImageSource : Element
	{
		readonly object _synchandle = new object();
		CancellationTokenSource _cancellationTokenSource;

		TaskCompletionSource<bool> _completionSource;

		readonly WeakEventManager _weakEventManager = new WeakEventManager();

		protected ImageSource()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageSource.xml" path="//Member[@MemberName='IsEmpty']/Docs/*" />
		public virtual bool IsEmpty => false;

		public static bool IsNullOrEmpty(ImageSource imageSource) =>
			imageSource == null || imageSource.IsEmpty;

		private protected CancellationTokenSource CancellationTokenSource
		{
			get { return _cancellationTokenSource; }
			private set
			{
				if (_cancellationTokenSource == value)
				{
				{
					return;
				}

				if (_cancellationTokenSource != null)
				{
					_cancellationTokenSource.Cancel();
				}

				_cancellationTokenSource = value;
			}
		}

		bool IsLoading
		{
			get { return _cancellationTokenSource != null; }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageSource.xml" path="//Member[@MemberName='Cancel']/Docs/*" />
		public virtual Task<bool> Cancel()
		{
			if (!IsLoading)

/* Unmerged change from project 'Controls.Core(net8.0)'
Before:
				return Task.FromResult(false);

			var tcs = new TaskCompletionSource<bool>();
			TaskCompletionSource<bool> original = Interlocked.CompareExchange(ref _completionSource, tcs, null);
			if (original == null)
			{
				_cancellationTokenSource.Cancel();
			}
			else
				tcs = original;
After:
			{
				return Task.FromResult(false);
			}

			var tcs = new TaskCompletionSource<bool>();
			TaskCompletionSource<bool> original = Interlocked.CompareExchange(ref _completionSource, tcs, null);
			if (original == null)
			{
				_cancellationTokenSource.Cancel();
			}
			else
			{
				tcs = original;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
				return Task.FromResult(false);

			var tcs = new TaskCompletionSource<bool>();
			TaskCompletionSource<bool> original = Interlocked.CompareExchange(ref _completionSource, tcs, null);
			if (original == null)
			{
				_cancellationTokenSource.Cancel();
			}
			else
				tcs = original;
After:
			{
				return Task.FromResult(false);
			}

			var tcs = new TaskCompletionSource<bool>();
			TaskCompletionSource<bool> original = Interlocked.CompareExchange(ref _completionSource, tcs, null);
			if (original == null)
			{
				_cancellationTokenSource.Cancel();
			}
			else
			{
				tcs = original;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Before:
				return Task.FromResult(false);

			var tcs = new TaskCompletionSource<bool>();
			TaskCompletionSource<bool> original = Interlocked.CompareExchange(ref _completionSource, tcs, null);
			if (original == null)
			{
				_cancellationTokenSource.Cancel();
			}
			else
				tcs = original;
After:
			{
				return Task.FromResult(false);
			}

			var tcs = new TaskCompletionSource<bool>();
			TaskCompletionSource<bool> original = Interlocked.CompareExchange(ref _completionSource, tcs, null);
			if (original == null)
			{
				_cancellationTokenSource.Cancel();
			}
			else
			{
				tcs = original;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Before:
				return Task.FromResult(false);

			var tcs = new TaskCompletionSource<bool>();
			TaskCompletionSource<bool> original = Interlocked.CompareExchange(ref _completionSource, tcs, null);
			if (original == null)
			{
				_cancellationTokenSource.Cancel();
			}
			else
				tcs = original;
After:
			{
				return Task.FromResult(false);
			}

			var tcs = new TaskCompletionSource<bool>();
			TaskCompletionSource<bool> original = Interlocked.CompareExchange(ref _completionSource, tcs, null);
			if (original == null)
			{
				_cancellationTokenSource.Cancel();
			}
			else
			{
				tcs = original;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Before:
				return Task.FromResult(false);

			var tcs = new TaskCompletionSource<bool>();
			TaskCompletionSource<bool> original = Interlocked.CompareExchange(ref _completionSource, tcs, null);
			if (original == null)
			{
				_cancellationTokenSource.Cancel();
			}
			else
				tcs = original;
After:
			{
				return Task.FromResult(false);
			}

			var tcs = new TaskCompletionSource<bool>();
			TaskCompletionSource<bool> original = Interlocked.CompareExchange(ref _completionSource, tcs, null);
			if (original == null)
			{
				_cancellationTokenSource.Cancel();
			}
			else
			{
				tcs = original;
			}
*/
			{
				return Task.FromResult(false);

/* Unmerged change from project 'Controls.Core(net8.0)'
Before:
				throw new ArgumentException("uri is relative");
			return new UriImageSource { Uri = uri };
After:
			{
				throw new ArgumentException("uri is relative");
			}

			return new UriImageSource { Uri = uri };
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
				throw new ArgumentException("uri is relative");
			return new UriImageSource { Uri = uri };
After:
			{
				throw new ArgumentException("uri is relative");
			}

			return new UriImageSource { Uri = uri };
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Before:
				throw new ArgumentException("uri is relative");
			return new UriImageSource { Uri = uri };
After:
			{
				throw new ArgumentException("uri is relative");
			}

			return new UriImageSource { Uri = uri };
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Before:
				throw new ArgumentException("uri is relative");
			return new UriImageSource { Uri = uri };
After:
			{
				throw new ArgumentException("uri is relative");
			}

			return new UriImageSource { Uri = uri };
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Before:
				throw new ArgumentException("uri is relative");
			return new UriImageSource { Uri = uri };
After:
			{
				throw new ArgumentException("uri is relative");
			}

			return new UriImageSource { Uri = uri };
*/

/* Unmerged change from project 'Controls.Core(net8.0)'
Before:
				return null;
After:
			{
				return null;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
				return null;
After:
			{
				return null;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Before:
				return null;
After:
			{
				return null;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Before:
				return null;
After:
			{
				return null;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Before:
				return null;
After:
			{
				return null;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0)'
Before:
				throw new ArgumentException("uri is relative");
			return FromUri(uri);
After:
			{
				throw new ArgumentException("uri is relative");
			}

			return FromUri(uri);
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
				throw new ArgumentException("uri is relative");
			return FromUri(uri);
After:
			{
				throw new ArgumentException("uri is relative");
			}

			return FromUri(uri);
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Before:
				throw new ArgumentException("uri is relative");
			return FromUri(uri);
After:
			{
				throw new ArgumentException("uri is relative");
			}

			return FromUri(uri);
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Before:
				throw new ArgumentException("uri is relative");
			return FromUri(uri);
After:
			{
				throw new ArgumentException("uri is relative");
			}

			return FromUri(uri);
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Before:
				throw new ArgumentException("uri is relative");
			return FromUri(uri);
After:
			{
				throw new ArgumentException("uri is relative");
			}

			return FromUri(uri);
*/

/* Unmerged change from project 'Controls.Core(net8.0)'
Before:
				return;
After:
			{
				return;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
				return;
After:
			{
				return;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Before:
				return;
After:
			{
				return;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Before:
				return;
After:
			{
				return;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Before:
				return;
After:
			{
				return;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0)'
Before:
				tcs.SetResult(cancelled);
After:
			{
				tcs.SetResult(cancelled);
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
				tcs.SetResult(cancelled);
After:
			{
				tcs.SetResult(cancelled);
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Before:
				tcs.SetResult(cancelled);
After:
			{
				tcs.SetResult(cancelled);
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Before:
				tcs.SetResult(cancelled);
After:
			{
				tcs.SetResult(cancelled);
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Before:
				tcs.SetResult(cancelled);
After:
			{
				tcs.SetResult(cancelled);
			}
*/
			}

			var tcs = new TaskCompletionSource<bool>();
			TaskCompletionSource<bool> original = Interlocked.CompareExchange(ref _completionSource, tcs, null);
			if (original == null)
			{
				_cancellationTokenSource.Cancel();
			}
			else
			{
				tcs = original;
			}

			return tcs.Task;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageSource.xml" path="//Member[@MemberName='FromFile']/Docs/*" />
		public static ImageSource FromFile(string file)
		{
			return new FileImageSource { File = file };
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageSource.xml" path="//Member[@MemberName='FromResource'][2]/Docs/*" />
		public static ImageSource FromResource(string resource, Type resolvingType)
		{
			return FromResource(resource, resolvingType.Assembly);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageSource.xml" path="//Member[@MemberName='FromResource'][1]/Docs/*" />
		public static ImageSource FromResource(string resource, Assembly sourceAssembly = null)
		{
			sourceAssembly = sourceAssembly ?? Assembly.GetCallingAssembly();

			return FromStream(() => sourceAssembly.GetManifestResourceStream(resource));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageSource.xml" path="//Member[@MemberName='FromStream'][1]/Docs/*" />
		public static ImageSource FromStream(Func<Stream> stream)
		{
			return new StreamImageSource { Stream = token => Task.Run(stream, token) };
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageSource.xml" path="//Member[@MemberName='FromStream'][2]/Docs/*" />
		public static ImageSource FromStream(Func<CancellationToken, Task<Stream>> stream)
		{
			return new StreamImageSource { Stream = stream };
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ImageSource.xml" path="//Member[@MemberName='FromUri']/Docs/*" />
		public static ImageSource FromUri(Uri uri)
		{
			if (!uri.IsAbsoluteUri)
			{
				throw new ArgumentException("uri is relative");
			}

			return new UriImageSource { Uri = uri };
		}

		public static implicit operator ImageSource(string source)
		{
			Uri uri;
			return Uri.TryCreate(source, UriKind.Absolute, out uri) && uri.Scheme != "file" ? FromUri(uri) : FromFile(source);
		}

		public static implicit operator ImageSource(Uri uri)
		{
			if (uri == null)
			{
				return null;
			}

			if (!uri.IsAbsoluteUri)
			{
				throw new ArgumentException("uri is relative");
			}

			return FromUri(uri);
		}

		private protected void OnLoadingCompleted(bool cancelled)
		{
			if (!IsLoading || _completionSource == null)
			{
				return;
			}

			TaskCompletionSource<bool> tcs = Interlocked.Exchange(ref _completionSource, null);
			if (tcs != null)
			{
				tcs.SetResult(cancelled);
			}

			lock (_synchandle)
			{
				CancellationTokenSource = null;
			}
		}

		private protected void OnLoadingStarted()
		{
			lock (_synchandle)
			{
				CancellationTokenSource = new CancellationTokenSource();
			}
		}

		protected void OnSourceChanged()
		{
			_weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(SourceChanged));
		}

		internal event EventHandler SourceChanged
		{
			add { _weakEventManager.AddEventHandler(value); }
			remove { _weakEventManager.RemoveEventHandler(value); }
		}
	}
}

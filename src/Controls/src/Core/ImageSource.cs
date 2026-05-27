#nullable disable
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	/// <summary>Abstract class whose implementors load images from files, URIs, or streams.</summary>
	[System.ComponentModel.TypeConverter(typeof(ImageSourceConverter))]
	public abstract partial class ImageSource : Element
	{
		readonly SemaphoreSlim _cancellationTokenSourceLock = new(1, 1);
		CancellationTokenSource _cancellationTokenSource;

		TaskCompletionSource<bool> _completionSource;

		readonly WeakEventManager _weakEventManager = new WeakEventManager();

		internal readonly MergedStyle _mergedStyle;

		protected ImageSource()
		{
			_mergedStyle = new MergedStyle(GetType(), this);
		}

		/// <summary>Gets a value indicating whether this image source is empty.</summary>
		public virtual bool IsEmpty => false;

		public static bool IsNullOrEmpty(ImageSource imageSource) =>
			imageSource == null || imageSource.IsEmpty;

		private protected CancellationTokenSource CancellationTokenSource
		{
			get { return _cancellationTokenSource; }
			private set
			{
				if (_cancellationTokenSource == value)
					return;
				_cancellationTokenSource?.Cancel();
				_cancellationTokenSource = value;
			}
		}

		bool IsLoading
		{
			get { return _cancellationTokenSource != null; }
		}

		/// <summary>Cancels the pending image load operation, if any.</summary>
		/// <returns>A task that returns <see langword="true"/> if the cancel succeeded.</returns>
		public virtual Task<bool> Cancel()
		{
			if (!IsLoading)
				return Task.FromResult(false);

			TaskCompletionSource<bool> original = Interlocked.CompareExchange(ref _completionSource, new TaskCompletionSource<bool>(), null);
			if (original is null)
			{
				_cancellationTokenSource.Cancel();
				return Task.FromResult(false);
			}

			return original.Task;
		}

		/// <summary>Creates an <see cref="ImageSource"/> from the specified file path.</summary>
		/// <param name="file">The path to the image file.</param>
		/// <returns>A <see cref="FileImageSource"/> for the specified file.</returns>
		public static ImageSource FromFile(string file)
		{
			return new FileImageSource { File = file };
		}

		/// <summary>Creates an <see cref="ImageSource"/> from an embedded resource in the assembly containing the specified type.</summary>
		/// <param name="resource">The name of the embedded resource.</param>
		/// <param name="resolvingType">A type whose assembly contains the resource.</param>
		/// <returns>A <see cref="StreamImageSource"/> for the embedded resource.</returns>
		public static ImageSource FromResource(string resource, Type resolvingType)
		{
			return FromResource(resource, resolvingType.Assembly);
		}

		/// <summary>Creates an <see cref="ImageSource"/> from an embedded resource in the specified assembly.</summary>
		/// <param name="resource">The name of the embedded resource.</param>
		/// <param name="sourceAssembly">The assembly containing the resource, or <see langword="null"/> to use the calling assembly.</param>
		/// <returns>A <see cref="StreamImageSource"/> for the embedded resource.</returns>
		public static ImageSource FromResource(string resource, Assembly sourceAssembly = null)
		{
			sourceAssembly = sourceAssembly ?? Assembly.GetCallingAssembly();

			return FromStream(() => sourceAssembly.GetManifestResourceStream(resource));
		}

		/// <summary>Creates an <see cref="ImageSource"/> from a stream factory function.</summary>
		/// <param name="stream">A factory function that returns a stream containing the image data.</param>
		/// <returns>A <see cref="StreamImageSource"/> for the stream.</returns>
		public static ImageSource FromStream(Func<Stream> stream)
		{
			return new StreamImageSource { Stream = token => Task.Run(stream, token) };
		}

		/// <summary>Creates an <see cref="ImageSource"/> from an async stream factory function.</summary>
		/// <param name="stream">A cancellable async factory function that returns a stream containing the image data.</param>
		/// <returns>A <see cref="StreamImageSource"/> for the stream.</returns>
		public static ImageSource FromStream(Func<CancellationToken, Task<Stream>> stream)
		{
			return new StreamImageSource { Stream = stream };
		}

		/// <summary>Creates an <see cref="ImageSource"/> from an absolute URI.</summary>
		/// <param name="uri">The absolute URI of the image.</param>
		/// <returns>A <see cref="UriImageSource"/> for the URI.</returns>
		public static ImageSource FromUri(Uri uri)
		{
			if (!uri.IsAbsoluteUri)
				throw new ArgumentException("uri is relative");
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
				return null;

			if (!uri.IsAbsoluteUri)
				throw new ArgumentException("uri is relative");
			return FromUri(uri);
		}

		private protected async Task OnLoadingCompleted(bool cancelled)
		{
			if (!IsLoading || _completionSource == null)
				return;

			TaskCompletionSource<bool> tcs = Interlocked.Exchange(ref _completionSource, null);
			tcs?.SetResult(cancelled);

			await _cancellationTokenSourceLock.WaitAsync();
			try
			{
				CancellationTokenSource = null;
			}
			finally
			{
				_cancellationTokenSourceLock.Release();
			}
		}

		private protected async Task OnLoadingStarted()
		{
			await _cancellationTokenSourceLock.WaitAsync();
			try
			{
				CancellationTokenSource = new CancellationTokenSource();
			}
			finally
			{
				_cancellationTokenSourceLock.Release();
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

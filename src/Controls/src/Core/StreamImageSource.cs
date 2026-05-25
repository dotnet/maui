#nullable disable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	/// <summary>An <see cref="ImageSource"/> that loads an image from a <see cref="System.IO.Stream"/>.</summary>
	public partial class StreamImageSource : ImageSource, IStreamImageSource
	{
		/// <summary>Bindable property for <see cref="Stream"/>.</summary>
		public static readonly BindableProperty StreamProperty = BindableProperty.Create(nameof(Stream), typeof(Func<CancellationToken, Task<Stream>>), typeof(StreamImageSource),
			default(Func<CancellationToken, Task<Stream>>));

		/// <summary>Gets a value indicating whether this image source is empty.</summary>
		public override bool IsEmpty => Stream == null;

		/// <summary>Gets or sets a cancellable async function that returns a stream containing the image data. This is a bindable property.</summary>
		public virtual Func<CancellationToken, Task<Stream>> Stream
		{
			get { return (Func<CancellationToken, Task<Stream>>)GetValue(StreamProperty); }
			set { SetValue(StreamProperty, value); }
		}

		protected override void OnPropertyChanged(string propertyName)
		{
			if (propertyName == StreamProperty.PropertyName)
				OnSourceChanged();
			base.OnPropertyChanged(propertyName);
		}

		async Task<Stream> IStreamImageSource.GetStreamAsync(CancellationToken userToken)
		{
			if (IsEmpty)
				return null;

			await OnLoadingStarted();
			userToken.Register(CancellationTokenSource.Cancel);
			Stream stream = null;
			try
			{
				stream = await Stream(CancellationTokenSource.Token);
				await OnLoadingCompleted(false);
			}
			catch (OperationCanceledException)
			{
				await OnLoadingCompleted(true);
				throw;
			}
			return stream;
		}
	}
}
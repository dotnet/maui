using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Forms
{
	public class StreamImageSource : ImageSource
	{
		public static readonly BindableProperty StreamProperty = BindableProperty.Create("Stream", typeof(Func<CancellationToken, Task<Stream>>), typeof(StreamImageSource),
			default(Func<CancellationToken, Task<Stream>>));

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

		internal async Task<Stream> GetStreamAsync(CancellationToken userToken = default(CancellationToken))
		{
			if (Stream == null)
				return null;

			OnLoadingStarted();
			userToken.Register(CancellationTokenSource.Cancel);
			Stream stream = null;
			try
			{
				stream = await Stream(CancellationTokenSource.Token);
				OnLoadingCompleted(false);
			}
			catch (OperationCanceledException)
			{
				OnLoadingCompleted(true);
				throw;
			}
			return stream;
		}
	}
}
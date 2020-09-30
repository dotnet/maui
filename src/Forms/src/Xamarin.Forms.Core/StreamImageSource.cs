using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Forms
{
	public class StreamImageSource : ImageSource, IStreamImageSource
	{
		public static readonly BindableProperty StreamProperty = BindableProperty.Create("Stream", typeof(Func<CancellationToken, Task<Stream>>), typeof(StreamImageSource),
			default(Func<CancellationToken, Task<Stream>>));

		public override bool IsEmpty => Stream == null;

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
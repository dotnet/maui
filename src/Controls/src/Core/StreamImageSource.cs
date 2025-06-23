#nullable disable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/StreamImageSource.xml" path="Type[@FullName='Microsoft.Maui.Controls.StreamImageSource']/Docs/*" />
	public partial class StreamImageSource : ImageSource, IStreamImageSource
	{
		/// <summary>Bindable property for <see cref="Stream"/>.</summary>
		public static readonly BindableProperty StreamProperty = BindableProperty.Create(nameof(Stream), typeof(Func<CancellationToken, Task<Stream>>), typeof(StreamImageSource),
			default(Func<CancellationToken, Task<Stream>>));

		/// <include file="../../docs/Microsoft.Maui.Controls/StreamImageSource.xml" path="//Member[@MemberName='IsEmpty']/Docs/*" />
		public override bool IsEmpty => Stream == null;

		/// <include file="../../docs/Microsoft.Maui.Controls/StreamImageSource.xml" path="//Member[@MemberName='Stream']/Docs/*" />
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
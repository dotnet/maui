#nullable enable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides data for the <see cref="Image.ImageFailed"/> event.
	/// </summary>
	public sealed class ImageFailedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ImageFailedEventArgs"/> class.
		/// </summary>
		/// <param name="exception">The exception that caused the image to fail to load.</param>
		public ImageFailedEventArgs(Exception exception)
		{
			Exception = exception ?? throw new ArgumentNullException(nameof(exception));
		}

		/// <summary>
		/// Gets the exception that caused the image to fail to load.
		/// </summary>
		/// <remarks>
		/// For images downloaded from a URI, this is typically a <see cref="System.Net.Http.HttpRequestException"/>
		/// (with the HTTP status code when the server responded) or a <see cref="TimeoutException"/>.
		/// </remarks>
		public Exception Exception { get; }
	}
}

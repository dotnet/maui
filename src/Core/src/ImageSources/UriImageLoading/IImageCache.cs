#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides access to the cache of downloaded images (for example images loaded by a <c>UriImageSource</c>).
	/// </summary>
	/// <remarks>Resolve this service from the app's <see cref="IServiceProvider"/>.</remarks>
	public interface IImageCache
	{
		/// <summary>
		/// Removes all downloaded images from the memory and disk caches.
		/// </summary>
		/// <param name="cancellationToken">A token to cancel the operation.</param>
		/// <remarks>Images that are already displayed stay visible. They are downloaded again the next time they are loaded.</remarks>
		Task ClearAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Removes the image downloaded from <paramref name="uri"/> from the memory and disk caches.
		/// </summary>
		/// <param name="uri">The URI of the image to remove.</param>
		/// <param name="cancellationToken">A token to cancel the operation.</param>
		Task RemoveAsync(Uri uri, CancellationToken cancellationToken = default);
	}
}

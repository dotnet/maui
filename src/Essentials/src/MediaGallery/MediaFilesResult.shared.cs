using System;
using System.Collections.Generic;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Media
{
	/// <summary>
	/// Describes the result of the <see cref="MediaGallery"/> methods
	/// </summary>
	public sealed class MediaFilesResult
	{
		internal MediaFilesResult(IEnumerable<MediaFileResult> files)
			=> Files = files;

		/// <summary>
		/// User-selected media files. Can return an null or empty value
		/// </summary>
		public IEnumerable<MediaFileResult> Files { get; }
	}
}


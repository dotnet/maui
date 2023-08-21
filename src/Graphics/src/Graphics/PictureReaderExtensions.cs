// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics
{
	public static class PictureReaderExtensions
	{
		public static IPicture Read(this IPictureReader target, Stream stream, string hash = null)
		{
			if (!(stream is MemoryStream memoryStream))
			{
				memoryStream = new MemoryStream();
				stream.CopyTo(memoryStream);
			}

			var bytes = memoryStream.ToArray();
			return target.Read(bytes);
		}

		public static async Task<IPicture> ReadAsync(this IPictureReader target, Stream stream, string hash = null)
		{
			if (!(stream is MemoryStream memoryStream))
			{
				memoryStream = new MemoryStream();
				await stream.CopyToAsync(memoryStream);
			}

			var bytes = memoryStream.ToArray();
			return target.Read(bytes);
		}
	}
}

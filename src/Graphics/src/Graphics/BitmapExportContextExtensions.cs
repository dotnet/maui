// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;

namespace Microsoft.Maui.Graphics
{
	public static class BitmapExportContextExtensions
	{
		public static void WriteToFile(this BitmapExportContext exportContext, string filename)
		{
			using (var outputStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
			{
				exportContext.WriteToStream(outputStream);
			}
		}
	}
}

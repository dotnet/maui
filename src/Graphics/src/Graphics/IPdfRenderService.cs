// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;

namespace Microsoft.Maui.Graphics
{
	public interface IPdfRenderService
	{
		IPdfPage CreatePage(Stream stream, int pageNumber = -1);
	}
}

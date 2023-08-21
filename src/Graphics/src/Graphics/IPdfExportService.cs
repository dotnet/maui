// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Graphics
{
	internal interface IPdfExportService
	{
		PdfExportContext CreateContext(float width = -1, float height = -1);
	}
}

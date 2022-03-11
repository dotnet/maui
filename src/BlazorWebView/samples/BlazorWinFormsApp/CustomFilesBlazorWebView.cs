// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.FileProviders;
using WebViewAppShared;

namespace BlazorWpfApp
{
	public class CustomFilesBlazorWebView : BlazorWebView
	{
		public override IFileProvider CreateFileProvider(string contentRootDir)
		{
			var inMemoryFiles = new InMemoryStaticFileProvider(
				fileContentsMap: new Dictionary<string, string>
				{
					{ "customindex.html", StaticFilesContents.CustomIndexHtmlContent },
				},
				// The contentRoot is ignored here because in WinForms it would include the absolute physical path to the app's content, which this provider doesn't care about
				contentRoot: null);

			return new CompositeFileProvider(inMemoryFiles, base.CreateFileProvider(contentRootDir));
		}
	}
}

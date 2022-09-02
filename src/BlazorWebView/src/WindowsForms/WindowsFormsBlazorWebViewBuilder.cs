// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Components.WebView.WindowsForms
{
	internal class WindowsFormsBlazorWebViewBuilder : IWindowsFormsBlazorWebViewBuilder
	{
		public IServiceCollection Services { get; }

		public WindowsFormsBlazorWebViewBuilder(IServiceCollection services)
		{
			Services = services;
		}
	}
}

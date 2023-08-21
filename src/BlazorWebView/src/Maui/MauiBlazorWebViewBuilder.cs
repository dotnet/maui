// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal class MauiBlazorWebViewBuilder : IMauiBlazorWebViewBuilder
	{
		public IServiceCollection Services { get; }

		public MauiBlazorWebViewBuilder(IServiceCollection services)
		{
			Services = services;
		}
	}
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Components.WebView.Gtk
{
	/// <summary>
	/// A builder for Gtk Blazor WebViews.
	/// </summary>
	public interface IGtkBlazorWebViewBuilder
	{
		/// <summary>
		/// Gets the builder service collection.
		/// </summary>
		IServiceCollection Services { get; }
	}
}

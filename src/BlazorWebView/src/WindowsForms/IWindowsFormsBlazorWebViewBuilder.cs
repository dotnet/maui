// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Components.WebView.WindowsForms
{
	/// <summary>
	/// A builder for Windows Forms Blazor WebViews.
	/// </summary>
	public interface IWindowsFormsBlazorWebViewBuilder
	{
		/// <summary>
		/// Gets the builder service collection.
		/// </summary>
		IServiceCollection Services { get; }
	}
}

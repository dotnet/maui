// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.AspNetCore.Components.WebView.Wpf
{
	/// <summary>
	/// Represents the capabilities supported by the underlying webview component in the underlying platform.
	/// </summary>
	public partial class BlazorWebViewCapabilities
	{
		/// <summary>
		/// Initializes a new instance of <see cref="BlazorWebViewCapabilities"/>.
		/// </summary>
		public BlazorWebViewCapabilities()
		{
			DefineCommonCapabilities();
		}
	}
}

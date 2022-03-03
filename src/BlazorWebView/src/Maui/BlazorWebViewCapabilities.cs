// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

#if WEBVIEW2_MAUI
namespace Microsoft.AspNetCore.Components.WebView.Maui
#elif WEBVIEW2_WPF
namespace Microsoft.AspNetCore.Components.WebView.Wpf
#else
namespace Microsoft.AspNetCore.Components.WebView.WindowsForms
#endif
{
	/// <summary>
	/// Represents the capabilities supported by the underlying webview component in the underlying platform.
	/// </summary>
	public partial class BlazorWebViewCapabilities
	{
		private Dictionary<string, object> _capabilities = new();

		private void DefineCommonCapabilities()
		{
			DefineCapability(BlazorWebViewCapabilityNames.DevelopmentMode, Debugger.IsAttached);
		}

		/// <summary>
		/// Determines whether or not the capability is supported.
		/// </summary>
		/// <param name="capabilityName">The capability name.</param>
		/// <returns><c>true</c> if supported; <c>false</c> otherwise.</returns>
		public bool IsSupported(string capabilityName) => _capabilities.ContainsKey(capabilityName);

		// This is used by the different flavors of WebView to define the available capabilities.
		internal void DefineCapability(string capabilityName, object defaultValue) => _capabilities.Add(capabilityName, defaultValue);

		/// <summary>
		/// Sets the capability with <paramref name="capabilityName"/> to the given <paramref name="value"/>.
		/// </summary>
		/// <param name="capabilityName">The capability name.</param>
		/// <param name="value">The capability value.</param>
		public void SetCapability(string capabilityName, object value)
		{
			if (!IsSupported(capabilityName))
			{
				throw new InvalidOperationException($"Capability '{capabilityName}' not supported.");
			}
			else
			{
				_capabilities[capabilityName] = value;
			}
		}

		/// <summary>
		/// Gets the capability with <paramref name="capabilityName"/> if it is supported.
		/// </summary>
		/// <param name="capabilityName">The capability name.</param>
		/// <returns>The capability value.</returns>
		/// <exception cref="InvalidOperationException">The capability is not supported.</exception>
		public object GetCapability(string capabilityName)
		{
			if (!IsSupported(capabilityName))
			{
				throw new InvalidOperationException($"Capability '{capabilityName}' not supported.");
			}
			else
			{
				return _capabilities[capabilityName];
			}
		}
	}
}

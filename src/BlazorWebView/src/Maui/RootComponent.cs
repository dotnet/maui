// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// Describes a root component that can be added to a <see cref="BlazorWebView"/>.
	/// </summary>
	public class RootComponent
	{
		/// <summary>
		/// Gets or sets the CSS selector string that specifies where in the document the component should be placed.
		/// This must be unique among the root components within the <see cref="BlazorWebView"/>.
		/// </summary>
		public string? Selector { get; set; }

		/// <summary>
		/// Gets or sets the type of the root component. This type must implement <see cref="IComponent"/>.
		/// </summary>
		public Type? ComponentType { get; set; }

		/// <summary>
		/// Gets or sets an optional dictionary of parameters to pass to the root component.
		/// </summary>
		public IDictionary<string, object?>? Parameters { get; set; }

		/// <summary>
		/// Validates this root component and adds it to the specified <see cref="WebViewManager"/>.
		/// </summary>
		/// <param name="webViewManager">The <see cref="WebViewManager"/> to add this root component to.</param>
		/// <returns>A <see cref="Task"/> that completes when the root component has been added.</returns>
		/// <remarks>
		/// This is intended for handlers implementing <see cref="IBlazorWebViewHandler"/> so that they can
		/// apply the same validation and registration behavior as the built-in handlers. Call this on the
		/// <see cref="WebViewManager.Dispatcher"/> thread.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="webViewManager"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="Selector"/> or <see cref="ComponentType"/> has not been set.</exception>
		public Task AddToWebViewManagerAsync(WebViewManager webViewManager)
		{
			ArgumentNullException.ThrowIfNull(webViewManager);

			// As a characteristic of XAML, we can't rely on non-default constructors. So we have to
			// validate that the required properties were set. We could skip validating this and allow
			// the lower-level renderer code to throw, but that would be harder for developers to understand.

			if (string.IsNullOrWhiteSpace(Selector))
			{
				throw new InvalidOperationException($"{nameof(RootComponent)} requires a value for its {nameof(Selector)} property, but no value was set.");
			}

			if (ComponentType is null)
			{
				throw new InvalidOperationException($"{nameof(RootComponent)} requires a value for its {nameof(ComponentType)} property, but no value was set.");
			}

			var parameterView = Parameters == null ? ParameterView.Empty : ParameterView.FromDictionary(Parameters);
			return webViewManager.AddRootComponentAsync(ComponentType, Selector, parameterView);
		}

		/// <summary>
		/// Validates this root component and removes it from the specified <see cref="WebViewManager"/>.
		/// </summary>
		/// <param name="webViewManager">The <see cref="WebViewManager"/> to remove this root component from.</param>
		/// <returns>A <see cref="Task"/> that completes when the root component has been removed.</returns>
		/// <remarks>
		/// This is intended for handlers implementing <see cref="IBlazorWebViewHandler"/> so that they can
		/// apply the same validation and removal behavior as the built-in handlers. Call this on the
		/// <see cref="WebViewManager.Dispatcher"/> thread.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="webViewManager"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="Selector"/> has not been set.</exception>
		public Task RemoveFromWebViewManagerAsync(WebViewManager webViewManager)
		{
			ArgumentNullException.ThrowIfNull(webViewManager);

			if (string.IsNullOrWhiteSpace(Selector))
			{
				throw new InvalidOperationException($"{nameof(RootComponent)} requires a value for its {nameof(Selector)} property, but no value was set.");
			}
			return webViewManager.RemoveRootComponentAsync(Selector);
		}
	}
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	static class PlaceholderElement
	{
		/// <summary>Bindable property for <see cref="IPlaceholderElement.Placeholder"/>.</summary>
		public static readonly BindableProperty PlaceholderProperty =
			BindableProperty.Create(nameof(IPlaceholderElement.Placeholder), typeof(string), typeof(IPlaceholderElement), default(string));

		/// <summary>Bindable property for <see cref="IPlaceholderElement.PlaceholderColor"/>.</summary>
		public static readonly BindableProperty PlaceholderColorProperty =
			BindableProperty.Create(nameof(IPlaceholderElement.PlaceholderColor), typeof(Color), typeof(IPlaceholderElement), default(Color));
	}
}
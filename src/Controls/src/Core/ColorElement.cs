// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	static class ColorElement
	{
		/// <summary>Bindable property for <see cref="Color"/>.</summary>
		public static readonly BindableProperty ColorProperty =
			BindableProperty.Create(nameof(IColorElement.Color), typeof(Color), typeof(IColorElement), null);
	}
}
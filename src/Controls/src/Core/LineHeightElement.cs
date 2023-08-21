// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	static class LineHeightElement
	{
		/// <summary>Bindable property for <see cref="ILineHeightElement.LineHeight"/>.</summary>
		public static readonly BindableProperty LineHeightProperty =
			BindableProperty.Create(nameof(ILineHeightElement.LineHeight), typeof(double), typeof(ILineHeightElement), -1.0d,
									propertyChanged: OnLineHeightChanged);

		static void OnLineHeightChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((ILineHeightElement)bindable).OnLineHeightChanged((double)oldValue, (double)newValue);
		}

	}
}
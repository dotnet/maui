// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.UI.Xaml;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Platform
{
	internal interface IEmptyView
	{
		WVisibility EmptyViewVisibility { get; set; }
		void SetEmptyView(FrameworkElement emptyView, View formsEmptyView);
	}
}
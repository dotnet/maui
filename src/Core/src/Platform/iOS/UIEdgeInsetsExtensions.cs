// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using UIKit;

namespace Microsoft.Maui.Platform;

public static class UIEdgeInsetsExtensions
{
	public static Thickness ToThickness(this UIEdgeInsets insets) => new(insets.Left, insets.Top, insets.Right, insets.Bottom);
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Controls
{
	public static class LayoutDirectionExtensions
	{
		public static FlowDirection ToFlowDirection(this LayoutDirection layoutDirection) =>
			layoutDirection == LayoutDirection.RightToLeft
				? FlowDirection.RightToLeft
				: FlowDirection.LeftToRight;
	}
}
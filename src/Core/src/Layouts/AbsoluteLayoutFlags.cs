// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.Layouts
{
	[Flags]
	public enum AbsoluteLayoutFlags
	{
		None = 0,
		XProportional = 1 << 0,
		YProportional = 1 << 1,
		WidthProportional = 1 << 2,
		HeightProportional = 1 << 3,
		PositionProportional = 1 | 1 << 1,
		SizeProportional = 1 << 2 | 1 << 3,
		All = ~0
	}
}
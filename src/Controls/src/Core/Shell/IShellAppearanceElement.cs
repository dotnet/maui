// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public interface IShellAppearanceElement
	{
		Color EffectiveTabBarBackgroundColor { get; }
		Color EffectiveTabBarDisabledColor { get; }
		Color EffectiveTabBarForegroundColor { get; }
		Color EffectiveTabBarTitleColor { get; }
		Color EffectiveTabBarUnselectedColor { get; }
	}
}
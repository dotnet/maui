// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using WStretch = Microsoft.UI.Xaml.Media.Stretch;

namespace Microsoft.Maui.Platform
{
	public static class AspectExtensions
	{
		public static WStretch ToStretch(this Aspect aspect) =>
			aspect switch
			{
				Aspect.AspectFit => WStretch.Uniform,
				Aspect.AspectFill => WStretch.UniformToFill,
				Aspect.Fill => WStretch.Fill,
				Aspect.Center => WStretch.None,
				_ => WStretch.Uniform,
			};
	}
}
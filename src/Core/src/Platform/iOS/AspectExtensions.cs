// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class AspectExtensions
	{
		public static UIViewContentMode ToUIViewContentMode(this Aspect aspect) =>
			aspect switch
			{
				Aspect.AspectFit => UIViewContentMode.ScaleAspectFit,
				Aspect.AspectFill => UIViewContentMode.ScaleAspectFill,
				Aspect.Fill => UIViewContentMode.ScaleToFill,
				Aspect.Center => UIViewContentMode.Center,
				_ => UIViewContentMode.ScaleAspectFit,
			};
	}
}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PageHandlerTests
	{
		public UIView GetNativePageContent(PageHandler handler)
		{
			int childCount = 0;
			if (handler.PlatformView is UIView view)
			{
				childCount = view.Subviews.Length;
				if (childCount == 1)
					return view.Subviews[0];
			}

			Assert.Equal(1, childCount);
			return null;
		}
	}
}
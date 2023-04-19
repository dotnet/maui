using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LayoutTests
	{
		void ValidateInputTransparentOnPlatformView(IView view)
		{
			if (view is ILayout)
			{
				Assert.True(view.ToPlatform(MauiContext) is not WrapperView wv || !wv.InputTransparent);
				return;
			}

			if (view.InputTransparent)
			{
				Assert.True(view.ToPlatform(MauiContext) is WrapperView wv && wv.InputTransparent);
			}
			else
			{
				Assert.True(view.ToPlatform(MauiContext) is not WrapperView wv || !wv.InputTransparent);
			}
		}
	}
}

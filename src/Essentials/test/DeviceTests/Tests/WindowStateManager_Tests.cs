using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Authentication;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("WindowStateManager")]
	public class WindowStateManager_Tests
	{
#if __IOS__
		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Machine)]
		public void GetCurrentUIWindow()
		{
			var window = WindowStateManager.Default.GetCurrentUIWindow();
			Assert.NotNull(window);
		}

		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Machine)]
		public void GetCurrentUIViewController()
		{
			var windows = WindowStateManager.Default.GetCurrentUIViewController();
			Assert.NotNull(windows);
			Assert.NotEmpty(windows);
		}
#endif
	}
}

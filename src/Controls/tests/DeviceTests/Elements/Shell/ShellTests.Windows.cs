using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;


namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests : HandlerTestBase
	{
		[Fact(DisplayName = "Flyout Locked Offset")]
		public async Task FlyoutLockedOffset()
		{
			SetupBuilder();
			var label = new StackLayout()
			{ 
				HeightRequest = 10
			};

			var shell = new Shell()
			{
				FlyoutHeader = label,
				Items =
				{
					new ContentPage()
				},
				FlyoutBehavior = FlyoutBehavior.Locked
			};

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, (handler) =>
			{
				var rootManager = handler.MauiContext.GetNavigationRootManager();
				var position = label.GetLocationRelativeTo(rootManager.AppTitleBar);
				var distance = rootManager.AppTitleBar.Height - position.Value.Y;
				Assert.True(Math.Abs(distance) < 1);
				return Task.CompletedTask;
			});
		}
	}
}

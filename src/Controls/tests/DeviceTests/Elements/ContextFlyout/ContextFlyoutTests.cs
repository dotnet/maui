using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{

	[Category(TestCategory.MenuFlyout)]
	public partial class ContextFlyoutTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Layout, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Page, PageHandler>();

					handlers.AddHandler<MenuFlyout, MenuFlyoutHandler>();
					handlers.AddHandler<MenuFlyoutItem, MenuFlyoutItemHandler>();
					handlers.AddHandler<MenuFlyoutSubItem, MenuFlyoutSubItemHandler>();
					handlers.AddHandler<MenuFlyoutSeparator, MenuFlyoutSeparatorHandler>();
				});
			});
		}
	}
}

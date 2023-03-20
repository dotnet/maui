using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Path)]
	public partial class PathTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<StackLayout, LayoutHandler>();
					handlers.AddHandler<Button, ButtonHandler>();
					handlers.AddHandler<Path, PathHandler>();
				});
			});
		}
	}
}
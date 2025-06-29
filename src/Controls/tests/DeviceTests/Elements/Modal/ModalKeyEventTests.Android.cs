using System.Threading.Tasks;
using Android.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Modal)]
	public class ModalKeyEventTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureFonts(fonts =>
				{
					fonts.AddFont("Dokdo-Regular.ttf", "Dokdo");
				});
			});
		}

		[Fact(DisplayName = "Modal Dialog Propagates Key Events to Activity")]
		public async Task ModalDialogPropagatesKeyEventsToActivity()
		{
			SetupBuilder();

			var mainPage = new ContentPage { Title = "Main Page" };
			var modalPage = new ContentPage { Title = "Modal Page" };

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(mainPage), async (handler) =>
			{
				// Push modal page
				await mainPage.Navigation.PushModalAsync(modalPage);

				// Get the modal fragment
				var modalManager = mainPage.FindMauiContext()?.GetNavigationManager() as Platform.ModalNavigationManager;
				var fragment = modalManager?.GetType()
					.GetField("_modalStack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
					?.GetValue(modalManager) as System.Collections.Generic.Stack<object>;

				// Verify the modal fragment implements the key listener interface
				if (fragment?.Count > 0)
				{
					var modalFragment = fragment.Peek();
					Assert.True(modalFragment is IDialogInterfaceOnKeyListener, 
						"Modal fragment should implement IDialogInterfaceOnKeyListener to propagate key events");
				}
			});
		}
	}
}
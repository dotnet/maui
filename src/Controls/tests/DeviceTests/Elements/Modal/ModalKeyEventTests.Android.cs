using System.Threading.Tasks;
using Android.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Controls.Hosting;
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

		[Fact(DisplayName = "Modal Dialog Navigation Works")]
		public async Task ModalDialogNavigationWorks()
		{
			SetupBuilder();

			var mainPage = new ContentPage { Title = "Main Page" };
			var modalPage = new ContentPage { Title = "Modal Page" };

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Microsoft.Maui.Controls.Window(mainPage), async (handler) =>
			{
				// Push modal page
				await mainPage.Navigation.PushModalAsync(modalPage);

				// Simple verification that modal was pushed
				Assert.Single(mainPage.Navigation.ModalStack);
				Assert.Equal(modalPage, mainPage.Navigation.ModalStack[0]);
				
				// Verify that the modal page is properly set up
				Assert.NotNull(modalPage.Handler);
				
				// Pop modal page
				await mainPage.Navigation.PopModalAsync();
				Assert.Empty(mainPage.Navigation.ModalStack);
			});
		}
	}
}
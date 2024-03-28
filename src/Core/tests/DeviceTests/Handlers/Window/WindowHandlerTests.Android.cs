using System.Threading.Tasks;
using AndroidX.AppCompat.App;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.Maui.DeviceTests.Stubs;
using Android.App;
using System;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowHandlerTests : CoreHandlerTestBase
	{

		[Fact]
		public async Task UsingTheSameWindowThrowsInvalidOperationException()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var app = (CoreApplicationStub)MauiContext.Services.GetRequiredService<IApplication>();

				var activity1 = new MauiAppCompatActivity();
				var activity2 = new MauiAppCompatActivity();
				
				activity1.CreatePlatformWindow(app, null);

				var window = app.Windows[0];
				app.SetSingleWindow(window);

				Assert.Throws<InvalidOperationException>(() =>
				{
					activity2.CreatePlatformWindow(app, null);
				});

			});
		}


		[Fact]
		public async Task TitleSetsOnWindow()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var activity = (AppCompatActivity)MauiProgramDefaults.DefaultContext;
				var testWindow = new Window();

				Assert.True(activity is not null, "Activity is Null");

				testWindow.Title = "Test Title";
				WindowExtensions.UpdateTitle(activity, testWindow);

				Assert.Equal("Test Title", activity.Title);
				testWindow.Title = null;

				WindowExtensions.UpdateTitle(activity, testWindow);
				Assert.Equal(activity.Title, ApplicationModel.AppInfo.Current.Name);
			});
		}
	}
}
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowTests
	{
		[Fact]
		public async Task WindowDestroyingPreservesWindowScopeOnAndroid()
		{
			// https://github.com/dotnet/maui/issues/33597
			SetupBuilder();

			var window = new Window(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(window.Page);

				var mauiContext = handler.MauiContext as MauiContext;
				Assert.NotNull(mauiContext);

				var windowScopeField = typeof(MauiContext).GetField("_windowScope", BindingFlags.NonPublic | BindingFlags.Instance);
				var setWindowScope = typeof(MauiContext).GetMethod("SetWindowScope", BindingFlags.NonPublic | BindingFlags.Instance);

				var newScope = mauiContext.Services.CreateScope();
				setWindowScope.Invoke(mauiContext, new[] { newScope });
				Assert.NotNull(windowScopeField.GetValue(mauiContext));

				((IWindow)window).Destroying();

				Assert.NotNull(windowScopeField.GetValue(mauiContext));
			});
		}

	}
}

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

#if ANDROID
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests : HandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
#if WINDOWS || ANDROID
					handlers.AddHandler(typeof(Controls.Shell), typeof(ShellHandler));
					handlers.AddHandler<Layout, LayoutHandler>();
					handlers.AddHandler<Image, ImageHandler>();
					handlers.AddHandler<Label, LabelHandler>();
#endif
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Toolbar, ToolbarHandler>();
#if WINDOWS
					handlers.AddHandler<ShellItem, ShellItemHandler>();
					handlers.AddHandler<ShellSection, ShellSectionHandler>();
					handlers.AddHandler<ShellContent, ShellContentHandler>();
#endif
				});
			});
		}

#if !IOS
		[Fact(DisplayName = "Empty Shell")]
		public async Task DetailsViewUpdates()
		{
			SetupBuilder();

			var shell = await InvokeOnMainThreadAsync<Shell>(() =>
			{
				return new Shell()
				{
					Items =
						{
							new ContentPage()
						}
				};
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				// TODO MAUI Fix this 
				await Task.Delay(100);
				Assert.NotNull(shell.Handler);
			});
		}


		[Theory]
		[ClassData(typeof(ShellBasicNavigationTestCases))]
		public async Task BasicShellNavigationStructurePermutations(ShellItem[] shellItems)
		{
			SetupBuilder();
			var shell = await InvokeOnMainThreadAsync<Shell>(() =>
			{
				var value = new Shell();
				foreach (var item in shellItems)
					value.Items.Add(item);

				return value;
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				// TODO MAUI Fix this 
				await Task.Delay(100);
				await shell.GoToAsync("//page2");
				await Task.Delay(100);
			});
		}
#endif

		protected Task<Shell> CreateShellAsync(Action<Shell> action)=>
			InvokeOnMainThreadAsync(() =>
			{
				var value = new Shell();
				action?.Invoke(value);
				return value;
			});
	}
}

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
	public partial class ModalTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ModalPageMarginCorrectAfterKeyboardOpens()
		{
			SetupBuilder();

			var navPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(navPage),
				async (handler) =>
				{
					VerticalStackLayout layout = new VerticalStackLayout();
					List<Entry> entries = new List<Entry>();
					ContentPage modalPage = new ContentPage()
					{
						Content = layout
					};

					for (int i = 0; i < 30; i++)
					{
						var entry = new Entry();
						entries.Add(entry);
						layout.Add(entry);
					}

					await navPage.CurrentPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(entries[0]);

				});
		}
	}
}

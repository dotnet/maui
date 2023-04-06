using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

#if IOS || MACCATALYST
using FlyoutViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.PhoneFlyoutPageRenderer;
#endif

namespace Microsoft.Maui.DeviceTests.TestCases
{
	public class ControlsPageTypesTestCases : IEnumerable<object[]>
	{
		private readonly List<object[]> _data = new()
		{
			new object[] { nameof(FlyoutPage) },
			new object[] { nameof(TabbedPage) },
			new object[] { nameof(ContentPage) },
			new object[] { nameof(Shell) },
			new object[] { nameof(NavigationPage) },
		};

		public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


		public static Page CreatePageType(string name, Page content)
		{
			switch (name)
			{
				case nameof(FlyoutPage):
					content.Title = content.Title ?? "Detail Title";
					return new FlyoutPage() { Flyout = new ContentPage() { Title = "title" }, Detail = content };
				case nameof(TabbedPage):
					return new TabbedPage() { Children = { content } };
				case nameof(ContentPage):
					return content;
				case nameof(Shell):
					return new Shell() { CurrentItem = (ContentPage)content };
				case nameof(NavigationPage):
					return new NavigationPage(content);
			}

			throw new Exception($"{name} not found");
		}

		public static void Setup(MauiAppBuilder builder)
		{
			builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.SetupShellHandlers();

				handlers.AddHandler(typeof(Controls.Label), typeof(LabelHandler));
				handlers.AddHandler(typeof(Controls.Toolbar), typeof(ToolbarHandler));
				handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));
#if IOS || MACCATALYST
				handlers.AddHandler(typeof(TabbedPage), typeof(TabbedRenderer));
				handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
#else
				handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
				handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
#endif
				handlers.AddHandler<Page, PageHandler>();
				handlers.AddHandler<Controls.Window, WindowHandlerStub>();
			});
		}
	}
}
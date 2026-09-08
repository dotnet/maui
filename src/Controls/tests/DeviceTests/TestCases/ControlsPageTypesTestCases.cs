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
	public enum ControlsPageTypesTestCase
	{
		ContentPage,
		FlyoutPage,
		TabbedPage,
		Shell,
		NavigationPage,
		FlyoutPageWithNavigationPage,
		TabbedPageWithNavigationPage,
		NavigationPageWithFlyoutPage,
		NavigationPageWithTabbedPage,
		NavigationPageWithFlyoutPageWithNavigationPage,
		NavigationPageWithTabbedPageWithNavigationPage
	}

	public class ControlsPageTypesTestCases : IEnumerable<object[]>
	{
		private readonly List<object[]> _data = new()
		{
			new object[] { ControlsPageTypesTestCase.FlyoutPage },
			new object[] { ControlsPageTypesTestCase.TabbedPage },
			new object[] { ControlsPageTypesTestCase.ContentPage },
			new object[] { ControlsPageTypesTestCase.Shell },
			new object[] { ControlsPageTypesTestCase.NavigationPage },
		};

		public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


		public static Page CreatePageType(ControlsPageTypesTestCase name, Page content)
		{
			switch (name)
			{
				case ControlsPageTypesTestCase.FlyoutPage:
					content.Title = content.Title ?? "Detail Title";
					return new FlyoutPage() { Flyout = new ContentPage() { Title = "title" }, Detail = content };
				case ControlsPageTypesTestCase.TabbedPage:
					return new TabbedPage() { Children = { content } };
				case ControlsPageTypesTestCase.ContentPage:
					return content;
				case ControlsPageTypesTestCase.Shell:
					return new Shell() { CurrentItem = (ContentPage)content };
				case ControlsPageTypesTestCase.NavigationPage:
					return new NavigationPage(content);
				case ControlsPageTypesTestCase.FlyoutPageWithNavigationPage:
					return new FlyoutPage()
					{
						Flyout = new ContentPage() { Title = "title" },
						Detail = new NavigationPage(content)
					};
				case ControlsPageTypesTestCase.TabbedPageWithNavigationPage:
					return new TabbedPage() { Children = { new NavigationPage(content) } };
				case ControlsPageTypesTestCase.NavigationPageWithFlyoutPage:
					return new NavigationPage(new FlyoutPage()
					{
						Flyout = new ContentPage() { Title = "title" },
						Detail = content
					});
				case ControlsPageTypesTestCase.NavigationPageWithTabbedPage:
					return new NavigationPage(new TabbedPage() { Children = { content } });
			}

			throw new Exception($"{name} not found");
		}

		public static Page CreatePageType(ControlsPageTypesTestCase name) => CreatePageType(name, new ContentPage());

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Pages;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	class Navigator : INavigation
	{
		readonly Microsoft.Maui.Controls.INavigation navigation;

		public Navigator(Microsoft.Maui.Controls.INavigation navigation)
		{
			this.navigation = navigation;
		}

		public Task NavigateTo(PageType page, object dataContext)
		{
			ContentPage p;
			switch (page)
			{
				case PageType.Home:
					p = new HomePage();
					break;
				case PageType.AssemblyTestList:
					p = new AssemblyTestListPage();
					break;
				case PageType.TestResult:
					p = new TestResultPage();
					break;
				case PageType.Credits:
					p = new CreditsPage();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			p.BindingContext = dataContext;

			return navigation.PushAsync(p);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Maui.Controls.Sample.ViewModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public class NavPage : NavigationPage
	{
		public NavPage(IServiceProvider services, MainPageViewModel viewModel) :
			base(new MainPage(services, viewModel))
		{
			ToolbarItems.Add(new ToolbarItem()
			{
				Text = "Nav Page"
			});

		}
	}
}

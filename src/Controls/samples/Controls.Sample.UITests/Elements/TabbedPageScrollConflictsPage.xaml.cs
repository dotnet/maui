using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TabbedPageScrollConflictsPage : TabbedPage
	{
		public TabbedPageScrollConflictsPage()
		{
			InitializeComponent();
		}

		void OnTabbedPageCurrentPageChanged(object sender, EventArgs e)
		{
			ScrollConflicsTabbedPage.Title = "Failed";
		}
	}
}
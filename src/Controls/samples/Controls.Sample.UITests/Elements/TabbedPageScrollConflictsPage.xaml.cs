using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{

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
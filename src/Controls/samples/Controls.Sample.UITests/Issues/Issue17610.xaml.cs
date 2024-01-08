using System;
using System.Collections;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 17610, "Cancelling Refresh With Slow Scroll Leaves Refresh Icon Visible", PlatformAffected.Android)]
	public partial class Issue17610 : ContentPage
	{
		public IEnumerable ItemSource {get;set;}

		public Issue17610()
		{
			InitializeComponent();
			ItemSource = 
				Enumerable.Range(0,100)
					.Select(x => new { Text = $"Item {x}", AutomationId = $"Item{x}"  })
					.ToList();		
					
			BindableLayout.SetItemsSource(vsl, ItemSource);
		}


		void Button_Clicked(object sender, EventArgs e)
		{
			refreshView.IsRefreshing = false;
		}
	}
}
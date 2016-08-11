using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41205, "UWP CreateDefault passes string instead of object")]
	public class Bugzilla41205 : ContentPage
	{
		public class ViewModel
		{
			public string Text { get { return "Pass"; } }
		}

		public class CustomListView : ListView
		{
			protected override Cell CreateDefault(object item)
			{
				if (item is ViewModel)
				{
					var newTextCell = new TextCell();
					newTextCell.SetBinding(TextCell.TextProperty, nameof(ViewModel.Text));
					return newTextCell;
				}
				return base.CreateDefault("Fail");
			}
		}

		public Bugzilla41205()
		{
			var listView = new CustomListView
			{
				ItemsSource = new []
				{
					new ViewModel(), 
					new ViewModel(),
				}
			};

			Content = listView;
		}

	}
}

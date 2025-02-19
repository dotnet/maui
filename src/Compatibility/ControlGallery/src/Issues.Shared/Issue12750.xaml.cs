using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.CustomAttributes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 12750,
		"[Bug] SwipeView in ListView on Android causes Overlapping Duplicates",
		PlatformAffected.Android)]
	public partial class Issue12750 : TestContentPage
	{
		ObservableCollection<string> _list;

		public Issue12750()
		{
			_list = new ObservableCollection<string>
			{
				"one",
				"two"
			};
#if APP
			InitializeComponent();

			Issue12750ListView.ItemsSource = _list;
#endif
		}

		protected override void Init()
		{
		}

#if APP
		void OnButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new Issue12750DetailPage(_list));
		}

		public class Issue12750DetailPage : ContentPage
		{
			ObservableCollection<string> _list;

			public Issue12750DetailPage(ObservableCollection<string> list)
			{
				_list = list;

				var layout = new StackLayout();

				var label = new Label
				{
					Text = "Add item:"
				};

				var entry = new Entry();

				var button = new Button
				{
					Text = "Add"
				};

				layout.Children.Add(label);
				layout.Children.Add(entry);
				layout.Children.Add(button);

				Content = layout;

				button.Clicked += (sender, args) =>
				{
					_list.Add(entry.Text);
					Navigation.PopModalAsync();
				};
			}
		}
#endif
	}
}
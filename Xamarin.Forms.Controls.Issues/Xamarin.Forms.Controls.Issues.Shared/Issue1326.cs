using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1326, "ListView word wrap in Label causing ViewCells to overlap", PlatformAffected.iOS)]
	public class Issue1326 : TestContentPage
	{
		private MyItemsViewModel _model = new MyItemsViewModel();

		protected override void Init()
		{
			DataTemplate MyItemsDataTemplate = new DataTemplate(() => new MyViewCell());

			ListView listView = new ListView
			{
				ItemTemplate = MyItemsDataTemplate,
				SeparatorVisibility = SeparatorVisibility.None,
				HasUnevenRows = true,
				IsPullToRefreshEnabled = false
			};
			listView.SetBinding(ListView.ItemsSourceProperty, new Binding(nameof(MyItemsViewModel.MyItems)));

			Content = new StackLayout { Children = { new Label { Text = "If the text in the cells below is overlapping, this test has failed." }, listView } };

			_model.MyItems.Add(new MyItem() { Description = "Record 1. OK" });
			_model.MyItems.Add(new MyItem() { Description = "Record 2. Abcde ab ab abcdefg.   x.  Xxxxxx Z.  Zzzzz" });
			_model.MyItems.Add(new MyItem() { Description = "Record 3. This one gets partially stomped on." });
			_model.MyItems.Add(new MyItem() { Description = "Record 4. OK" });

			Content.BindingContext = _model;
		}

		class MyItem
		{
			public String Description { get; set; }
		}

		class MyItemsViewModel
		{
			public List<MyItem> MyItems { get; set; } = new List<MyItem>();
		}

		class MyViewCell : ViewCell
		{
			Label label;

			public MyViewCell()
			{
				label = new Label
				{
					Margin = new Thickness(0, 3, 0, 3),
					LineBreakMode = LineBreakMode.WordWrap,
					HorizontalOptions = LayoutOptions.Start,
					VerticalTextAlignment = TextAlignment.Start,
					FontFamily = "HelveticaNeue-Light",
					FontSize = 16
				};
				label.SetBinding(Label.TextProperty, new Binding(nameof(MyItem.Description)));

				View = new StackLayout
				{
					Margin = new Thickness(15, 8, 10, 0),
					Children = { label },
				};
			}
		}
	}
}
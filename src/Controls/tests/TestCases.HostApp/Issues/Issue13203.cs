using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 13203, "[Bug] [iOS] CollectionView does not bind to items if `IsVisible=False`", PlatformAffected.iOS)]
public class Issue13203 : TestContentPage
{
	const string Success = "Success";

	protected override void Init()
	{
		var cv = new CollectionView
		{
			IsVisible = false,

			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, new Binding(nameof(Item.Text)));
				return label;
			})
		};

		var source = new List<Item> { new Item { Text = Success } };
		cv.ItemsSource = source;

		Content = cv;

		Appearing += (sender, args) => { cv.IsVisible = true; };
	}

	class Item
	{
		public string Text { get; set; }
	}
}

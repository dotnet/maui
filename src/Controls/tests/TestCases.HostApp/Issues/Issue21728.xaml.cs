using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21728, "CollectionView item alignment issue when a single item is present with a footer", PlatformAffected.iOS)]
public partial class Issue21728 : ContentPage
{
	public IList<TestItem> Items { get; set; }

	public Issue21728()
    {
        InitializeComponent();
		BindingContext = this;
		Items = new List<TestItem>();
		Items.Add(new TestItem() { Name = "Test Item 1" });
		collectionview.ItemsSource = Items;
	}
	
	public class TestItem
	{
		public string Name { get; set; }
	}
}
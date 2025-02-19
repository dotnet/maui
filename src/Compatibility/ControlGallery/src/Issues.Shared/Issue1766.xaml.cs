using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if APP
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1766, "[WP8] ListView with button fires .ItemTapped instead of .Clicked", PlatformAffected.WinPhone)]
	public partial class Issue1766 : ContentPage
	{
		public Issue1766()
		{
			InitializeComponent();
			MyItems.Add(new MyItem() { Reference = DateTime.Now.Ticks.ToString(), ShowButton = true });
			MyItems.Add(new MyItem() { Reference = DateTime.Now.Ticks.ToString(), ShowButton = false });
			MyItems.Add(new MyItem() { Reference = DateTime.Now.Ticks.ToString(), ShowButton = true });
			MyItems.Add(new MyItem() { Reference = DateTime.Now.Ticks.ToString(), ShowButton = false });

			var myListViewList = this.FindByName<ListView>("MyListViewList");

			foreach (var item in myListViewList.ItemTemplate.Values)
			{
				System.Diagnostics.Debug.WriteLine("item: {0}", item);
			}

			if (myListViewList != null)
			{
				myListViewList.ItemTapped += (sender, args) =>
				{
					DisplayAlert("Item Tapped", "Item Tapped", "Ok");
				};
			}

			BindingContext = this;
		}

		List<MyItem> _myItems;
		public List<MyItem> MyItems
		{
			get
			{
				if (_myItems == null)
				{
					_myItems = new List<MyItem>();
				}
				return _myItems;
			}
		}

		public void OnButtonClicked(object sender, EventArgs e)
		{
			DisplayAlert("Button Tapped", "Button Tapped", "Ok");
		}
	}

	public class MyItem
	{
		public string Reference { get; set; }
		public bool ShowButton { get; set; }
	}
#endif

}


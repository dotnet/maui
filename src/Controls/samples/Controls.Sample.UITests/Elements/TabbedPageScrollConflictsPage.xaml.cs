using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	class TabItem
	{
		public string Name { get; set; }
		public TabItem(string name) { Name = name; }
	}

	class TabItemList
	{
		public List<TabItem> TabItems { get; set; }

		public TabItemList()
		{
			TabItems = new List<TabItem>();

			for (int i = 0; i < 5; i++)
			{
				TabItems.Add(new TabItem($"Tab-{i}"));
			}
		}
	}

	public partial class TabbedPageScrollConflictsPage : TabbedPage
	{
		public TabbedPageScrollConflictsPage()
		{
			InitializeComponent();

			BindingContext = new TabItemList();
		}

		void OnTabbedPageCurrentPageChanged(object sender, EventArgs e)
		{
			ScrollConflicsTabbedPage.Title = "Failed";
		}
	}
}
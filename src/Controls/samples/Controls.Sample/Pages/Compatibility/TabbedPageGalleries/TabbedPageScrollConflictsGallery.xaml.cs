using System.Collections.Generic;

namespace Maui.Controls.Sample.Pages.TabbedPageGalleries
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

	public partial class TabbedPageScrollConflictsGallery
	{
		public TabbedPageScrollConflictsGallery()
		{
			InitializeComponent();

			BindingContext = new TabItemList();
		}
	}
}
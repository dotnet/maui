using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	internal static class NSTableViewExtensions
	{
		public static NSTableView AsListViewLook(this NSTableView self)
		{
			self.SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.SourceList;

			self.AllowsColumnReordering = false;
			self.AllowsColumnResizing = false;
			self.AllowsColumnSelection = false;

			//this is needed .. can we go around it ?
			self.AddColumn(new NSTableColumn("1"));
			//this line hides the header by default
			self.HeaderView = new CustomNSTableHeaderView();
			return self;
		}
	}
}
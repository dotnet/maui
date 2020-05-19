using System;
using System.Linq;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class TableViewScenario1 : PerformanceScenario
	{
		public TableViewScenario1()
		: base("[TableView] Empty")
		{
			View = new TableView();
		}
	}
}

using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PerformanceGallery.Scenarios
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

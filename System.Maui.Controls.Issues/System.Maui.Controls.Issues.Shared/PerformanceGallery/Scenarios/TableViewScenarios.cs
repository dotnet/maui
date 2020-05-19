using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
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

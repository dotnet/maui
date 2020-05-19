using System;
using System.Linq;
using System.Maui.Internals;
using System.Maui.Maps;

namespace System.Maui.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class MapScenario1 : PerformanceScenario
	{
		public MapScenario1()
		: base("[Map] Empty")
		{
			View = new Map();
		}
	}
}

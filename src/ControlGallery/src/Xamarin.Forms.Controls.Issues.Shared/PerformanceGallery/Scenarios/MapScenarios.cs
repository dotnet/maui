using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Maps;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PerformanceGallery.Scenarios
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

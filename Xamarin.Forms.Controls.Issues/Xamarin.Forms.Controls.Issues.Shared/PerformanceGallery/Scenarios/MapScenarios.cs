using System;
using System.Linq;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;

namespace Xamarin.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
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

﻿using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class BoxViewScenario1 : PerformanceScenario
	{
		public BoxViewScenario1()
		: base("[BoxView] Color set in ctor")
		{
			View = new BoxView { Color = Colors.Red };
		}
	}
}

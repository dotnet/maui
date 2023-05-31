using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class EditorScenario1 : PerformanceScenario
	{
		public EditorScenario1()
		: base("[Editor] Empty")
		{
			View = new Editor();
		}
	}
}

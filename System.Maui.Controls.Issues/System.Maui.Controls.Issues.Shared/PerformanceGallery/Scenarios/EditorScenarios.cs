using System;
using System.Linq;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.PerformanceGallery.Scenarios
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

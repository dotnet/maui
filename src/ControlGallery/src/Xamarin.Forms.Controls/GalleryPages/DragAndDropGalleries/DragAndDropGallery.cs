using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.DragAndDropGalleries
{
	[Preserve(AllMembers = true)]
	public class DragAndDropGallery : Shell
	{
		public DragAndDropGallery()
		{
			Items.Add(new EnablingAndDisablingGestureTests());
			Items.Add(new VariousDragAndDropPermutations());
			Items.Add(new DragAndDropBetweenLayouts());
			Items.Add(new DragAndDropEvents());
			Items.Add(new DragPaths());
		}
	}
}

using System.Collections.Generic;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.DragAndDropGalleries
{
	[Preserve(AllMembers = true)]
	public class DragAndDropGallery : Shell
	{
		public DragAndDropGallery()
		{
			Device.SetFlags(new List<string> { ExperimentalFlags.DragAndDropExperimental, ExperimentalFlags.ShellUWPExperimental });
			Items.Add(new EnablingAndDisablingGestureTests());
			Items.Add(new VariousDragAndDropPermutations());
		}
	}
}

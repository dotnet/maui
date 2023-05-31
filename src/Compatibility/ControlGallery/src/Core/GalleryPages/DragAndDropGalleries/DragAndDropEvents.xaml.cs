using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.DragAndDropGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DragAndDropEvents : ContentPage
	{
		public DragAndDropEvents()
		{
			InitializeComponent();
		}
		void AddEvent(string name)
		{
			events.Text += $"{name},";
		}

		void DragStarting(object sender, DragStartingEventArgs e)
		{
			AddEvent(nameof(DragStarting));
		}

		void DropCompleted(object sender, DropCompletedEventArgs e)
		{
			AddEvent(nameof(DropCompleted));
		}

		void DragLeave(object sender, DragEventArgs e)
		{
			AddEvent(nameof(DragLeave));
		}

		void DragOver(object sender, DragEventArgs e)
		{
			AddEvent(nameof(DragOver));
		}

		void Drop(object sender, DropEventArgs e)
		{
			AddEvent(nameof(Drop));
		}
	}
}
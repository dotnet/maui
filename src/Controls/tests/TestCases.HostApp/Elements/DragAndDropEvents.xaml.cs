namespace Maui.Controls.Sample
{
	public partial class DragAndDropEvents : ContentView
	{
		bool _emittedDragOver = false;
		public DragAndDropEvents()
		{
			InitializeComponent();
		}

		void DragStarting(object sender, DragStartingEventArgs e)
		{
			_emittedDragOver = false;
			dragStartEvent.Text = "DragStarting";
		}

		void DropCompleted(object sender, DropCompletedEventArgs e)
		{
			dragCompletedEvent.Text = "DropCompleted";
		}

		void DragLeave(object sender, DragEventArgs e)
		{
			dragLeaveEvent.Text = "DragLeave";
		}

		void DragOver(object sender, DragEventArgs e)
		{
			if (!_emittedDragOver) // This can generate a lot of noise, only add it once
			{
				dragOverEvent.Text = "DragOver";
				_emittedDragOver = true;
			}
		}

		void Drop(object sender, DropEventArgs e)
		{
			dropEvent.Text = "Drop";
		}
	}
}
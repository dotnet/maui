namespace Maui.Controls.Sample
{
	public partial class DragAndDropEvents : ContentView
	{
		bool _emittedDragOver = false;
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
			_emittedDragOver = false;
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
			if (!_emittedDragOver) // This can generate a lot of noise, only add it once
			{
				AddEvent(nameof(DragOver));
				_emittedDragOver = true;
			}
		}

		void Drop(object sender, DropEventArgs e)
		{
			AddEvent(nameof(Drop));
		}
	}
}
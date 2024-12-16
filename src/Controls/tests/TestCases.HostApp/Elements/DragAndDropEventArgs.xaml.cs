namespace Maui.Controls.Sample;

public partial class DragAndDropEventArgs : ContentView
{
	bool _emittedDragOver = false;
	public DragAndDropEventArgs()
	{
		InitializeComponent();
	}

	void DragStarting(object sender, DragStartingEventArgs e)
	{
		_emittedDragOver = false;
		if (e.PlatformArgs is PlatformDragStartingEventArgs platformArgs)
		{
#if IOS || MACCATALYST
			if (platformArgs.Sender is not null)
				dragStartEvent.Text += $"{"DragStarting:" + nameof(platformArgs.Sender)},";
			if (platformArgs.DragInteraction is not null)
				dragStartEvent.Text += $"{"DragStarting:" + nameof(platformArgs.DragInteraction)},";
			if (platformArgs.DragSession is not null)
				dragStartEvent.Text += $"{"DragStarting:" + nameof(platformArgs.DragSession)},";
#elif ANDROID
			if (platformArgs.Sender is not null)
				dragStartEvent.Text += $"{"DragStarting:" + nameof(platformArgs.Sender)},";
			if (platformArgs.MotionEvent is not null)
				dragStartEvent.Text += $"{"DragStarting:" + nameof(platformArgs.MotionEvent)},";
#elif WINDOWS
			if (platformArgs.Sender is not null)
				dragStartEvent.Text += $"{"DragStarting:" + nameof(platformArgs.Sender)},";
			if (platformArgs.DragStartingEventArgs is not null)
				dragStartEvent.Text += $"{"DragStarting:" + nameof(platformArgs.DragStartingEventArgs)},";
			dragStartEvent.Text += $"{"DragStarting:" + nameof(platformArgs.Handled)},";
#endif
		}
	}

	void DropCompleted(object sender, DropCompletedEventArgs e)
	{
		if (e.PlatformArgs is PlatformDropCompletedEventArgs platformArgs)
		{
#if IOS || MACCATALYST
			if (platformArgs.Sender is not null)
				dropCompletedEvent.Text += $"{"DropCompleted:" + nameof(platformArgs.Sender)},";
			if (platformArgs.DropInteraction is not null)
				dropCompletedEvent.Text += $"{"DropCompleted:" + nameof(platformArgs.DropInteraction)},";
			if (platformArgs.DropSession is not null)
				dropCompletedEvent.Text += $"{"DropCompleted:" + nameof(platformArgs.DropSession)},";
#elif ANDROID
			if (platformArgs.Sender is not null)
				dropCompletedEvent.Text += $"{"DropCompleted:" + nameof(platformArgs.Sender)},";
			if (platformArgs.DragEvent is not null)
				dropCompletedEvent.Text += $"{"DropCompleted:" + nameof(platformArgs.DragEvent)},";
#elif WINDOWS
			if (platformArgs.Sender is not null)
				dropCompletedEvent.Text += $"{"DropCompleted:" + nameof(platformArgs.Sender)},";
			if (platformArgs.DropCompletedEventArgs is not null)
				dropCompletedEvent.Text += $"{"DropCompleted:" + nameof(platformArgs.DropCompletedEventArgs)},";
#endif
		}
	}

	void DragLeave(object sender, DragEventArgs e)
	{
		if (e.PlatformArgs is PlatformDragEventArgs platformArgs)
		{
#if IOS || MACCATALYST
			if (platformArgs.Sender is not null)
				dragLeaveEvent.Text += $"{"DragLeave:" + nameof(platformArgs.Sender)},";
			if (platformArgs.DropInteraction is not null)
				dragLeaveEvent.Text += $"{"DragLeave:" + nameof(platformArgs.DropInteraction)},";
			if (platformArgs.DropSession is not null)
				dragLeaveEvent.Text += $"{"DragLeave:" + nameof(platformArgs.DropSession)},";
#elif ANDROID
			if (platformArgs.Sender is not null)
				dragLeaveEvent.Text += $"{"DragLeave:" + nameof(platformArgs.Sender)},";
			if (platformArgs.DragEvent is not null)
				dragLeaveEvent.Text += $"{"DragLeave:" + nameof(platformArgs.DragEvent)},";
#elif WINDOWS
			if (platformArgs.Sender is not null)
				dragLeaveEvent.Text += $"{"DragLeave:" + nameof(platformArgs.Sender)},";
			if (platformArgs.DragEventArgs is not null)
				dragLeaveEvent.Text += $"{"DragLeave:" + nameof(platformArgs.DragEventArgs)},";
			dragLeaveEvent.Text += $"{"DragLeave:" + nameof(platformArgs.Handled)},";
#endif
		}
	}

	void DragOver(object sender, DragEventArgs e)
	{
		if (!_emittedDragOver) // This can generate a lot of noise, only add it once
		{
			if (e.PlatformArgs is PlatformDragEventArgs platformArgs)
			{
#if IOS || MACCATALYST
				if (platformArgs.Sender is not null)
					dragOverEvent.Text += $"{"DragOver:" + nameof(platformArgs.Sender)},";
				if (platformArgs.DropInteraction is not null)
					dragOverEvent.Text += $"{"DragOver:" + nameof(platformArgs.DropInteraction)},";
				if (platformArgs.DropSession is not null)
					dragOverEvent.Text += $"{"DragOver:" + nameof(platformArgs.DropSession)},";
#elif ANDROID
				if (platformArgs.Sender is not null)
					dragOverEvent.Text += $"{"DragOver:" + nameof(platformArgs.Sender)},";
				if (platformArgs.DragEvent is not null)
					dragOverEvent.Text += $"{"DragOver:" + nameof(platformArgs.DragEvent)},";
#elif WINDOWS
				if (platformArgs.Sender is not null)
					dragOverEvent.Text += $"{"DragOver:" + nameof(platformArgs.Sender)},";
				if (platformArgs.DragEventArgs is not null)
					dragOverEvent.Text += $"{"DragOver:" + nameof(platformArgs.DragEventArgs)},";
#endif
			}
			_emittedDragOver = true;
		}
	}

	void Drop(object sender, DropEventArgs e)
	{
		if (e.PlatformArgs is PlatformDropEventArgs platformArgs)
		{
#if IOS || MACCATALYST
			if (platformArgs.Sender is not null)
				dropEvent.Text += $"{"Drop:" + nameof(platformArgs.Sender)},";
			if (platformArgs.DropInteraction is not null)
				dropEvent.Text += $"{"Drop:" + nameof(platformArgs.DropInteraction)},";
			if (platformArgs.DropSession is not null)
				dropEvent.Text += $"{"Drop:" + nameof(platformArgs.DropSession)},";
#elif ANDROID
			if (platformArgs.Sender is not null)
				dropEvent.Text += $"{"Drop:" + nameof(platformArgs.Sender)},";
			if (platformArgs.DragEvent is not null)
				dropEvent.Text += $"{"Drop:" + nameof(platformArgs.DragEvent)},";
#elif WINDOWS
			if (platformArgs.Sender is not null)
				dropEvent.Text += $"{"Drop:" + nameof(platformArgs.Sender)},";
			if (platformArgs.DragEventArgs is not null)
				dropEvent.Text += $"{"Drop:" + nameof(platformArgs.DragEventArgs)},";
#endif
		}
	}
}

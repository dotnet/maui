namespace Maui.Controls.Sample;

public partial class DragAndDropEventArgs : ContentView
{
	bool _emittedDragOver = false;
	public DragAndDropEventArgs()
	{
		InitializeComponent();
	}

	void AddEvent(string name, Label? label = null)
	{
		events.Text += $"{name},";
		if (label is not null)
			label.Text += $"{name},";
	}

	void DragStarting(object sender, DragStartingEventArgs e)
	{
		_emittedDragOver = false;
		if (e.PlatformArgs is PlatformDragStartingEventArgs platformArgs)
		{
#if IOS || MACCATALYST
			if (platformArgs.Sender is not null)
				AddEvent("DragStarting:" + nameof(platformArgs.Sender), dragStartEvent);
			if (platformArgs.DragInteraction is not null)
				AddEvent("DragStarting:" + nameof(platformArgs.DragInteraction), dragStartEvent);
			if (platformArgs.DragSession is not null)
				AddEvent("DragStarting:" + nameof(platformArgs.DragSession), dragStartEvent);
#elif ANDROID
			if (platformArgs.Sender is not null)
				AddEvent("DragStarting:" + nameof(platformArgs.Sender), dragStartEvent);
			if (platformArgs.MotionEvent is not null)
				AddEvent("DragStarting:" + nameof(platformArgs.MotionEvent), dragStartEvent);
#elif WINDOWS
			if (platformArgs.Sender is not null)
				AddEvent("DragStarting:" + nameof(platformArgs.Sender), dragStartEvent);
			if (platformArgs.DragStartingEventArgs is not null)
				AddEvent("DragStarting:" + nameof(platformArgs.DragStartingEventArgs), dragStartEvent);
			AddEvent("DragStarting:" + nameof(platformArgs.Handled), dragStartEvent);
#endif
		}
	}

	void DropCompleted(object sender, DropCompletedEventArgs e)
	{
		if (e.PlatformArgs is PlatformDropCompletedEventArgs platformArgs)
		{
#if IOS || MACCATALYST
			if (platformArgs.Sender is not null)
				AddEvent("DropCompleted:" + nameof(platformArgs.Sender), dropCompletedEvent);
			if (platformArgs.DropInteraction is not null)
				AddEvent("DropCompleted:" + nameof(platformArgs.DropInteraction), dropCompletedEvent);
			if (platformArgs.DropSession is not null)
				AddEvent("DropCompleted:" + nameof(platformArgs.DropSession), dropCompletedEvent);
#elif ANDROID
			if (platformArgs.Sender is not null)
				AddEvent("DropCompleted:" + nameof(platformArgs.Sender), dropCompletedEvent);
			if (platformArgs.DragEvent is not null)
				AddEvent("DropCompleted:" + nameof(platformArgs.DragEvent), dropCompletedEvent);
#elif WINDOWS
			if (platformArgs.Sender is not null)
				AddEvent("DropCompleted:" + nameof(platformArgs.Sender), dropCompletedEvent);
			if (platformArgs.DropCompletedEventArgs is not null)
				AddEvent("DropCompleted:" + nameof(platformArgs.DropCompletedEventArgs), dropCompletedEvent);
#endif
		}
	}

	void DragLeave(object sender, DragEventArgs e)
	{
		if (e.PlatformArgs is PlatformDragEventArgs platformArgs)
		{
#if IOS || MACCATALYST
			if (platformArgs.Sender is not null)
				AddEvent("DragLeave:" + nameof(platformArgs.Sender));
			if (platformArgs.DropInteraction is not null)
				AddEvent("DragLeave:" + nameof(platformArgs.DropInteraction));
			if (platformArgs.DropSession is not null)
				AddEvent("DragLeave:" + nameof(platformArgs.DropSession));
#elif ANDROID
			if (platformArgs.Sender is not null)
				AddEvent("DragLeave:" + nameof(platformArgs.Sender));
			if (platformArgs.DragEvent is not null)
				AddEvent("DragLeave:" + nameof(platformArgs.DragEvent));
#elif WINDOWS
			if (platformArgs.Sender is not null)
				AddEvent("DragLeave:" + nameof(platformArgs.Sender));
			if (platformArgs.DragEventArgs is not null)
				AddEvent("DragLeave:" + nameof(platformArgs.DragEventArgs));
			AddEvent("DragLeave:" + nameof(platformArgs.Handled));
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
					AddEvent("DragOver:" + nameof(platformArgs.Sender), dragOverEvent);
				if (platformArgs.DropInteraction is not null)
					AddEvent("DragOver:" + nameof(platformArgs.DropInteraction), dragOverEvent);
				if (platformArgs.DropSession is not null)
					AddEvent("DragOver:" + nameof(platformArgs.DropSession), dragOverEvent);
#elif ANDROID
				if (platformArgs.Sender is not null)
					AddEvent("DragOver:" + nameof(platformArgs.Sender), dragOverEvent);
				if (platformArgs.DragEvent is not null)
					AddEvent("DragOver:" + nameof(platformArgs.DragEvent), dragOverEvent);
#elif WINDOWS
				if (platformArgs.Sender is not null)
					AddEvent("DragOver:" + nameof(platformArgs.Sender), dragOverEvent);
				if (platformArgs.DragEventArgs is not null)
					AddEvent("DragOver:" + nameof(platformArgs.DragEventArgs), dragOverEvent);
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
				AddEvent("Drop:" + nameof(platformArgs.Sender), dropEvent);
			if (platformArgs.DropInteraction is not null)
				AddEvent("Drop:" + nameof(platformArgs.DropInteraction), dropEvent);
			if (platformArgs.DropSession is not null)
				AddEvent("Drop:" + nameof(platformArgs.DropSession), dropEvent);
#elif ANDROID
			if (platformArgs.Sender is not null)
				AddEvent("Drop:" + nameof(platformArgs.Sender), dropEvent);
			if (platformArgs.DragEvent is not null)
				AddEvent("Drop:" + nameof(platformArgs.DragEvent), dropEvent);
#elif WINDOWS
			if (platformArgs.Sender is not null)
				AddEvent("Drop:" + nameof(platformArgs.Sender), dropEvent);
			if (platformArgs.DragEventArgs is not null)
				AddEvent("Drop:" + nameof(platformArgs.DragEventArgs), dropEvent);
#endif
		}
	}
}

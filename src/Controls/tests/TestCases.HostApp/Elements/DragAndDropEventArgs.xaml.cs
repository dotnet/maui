using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class DragAndDropEventArgs : ContentView
{
	bool _emittedDragOver = false;
	public DragAndDropEventArgs()
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
		if (e.PlatformArgs is PlatformDragStartingEventArgs platformArgs)
		{
#if IOS || MACCATALYST
			if (platformArgs.Sender is not null)
				AddEvent("DragStarting:" + nameof(platformArgs.Sender));
			if (platformArgs.DragInteraction is not null)
				AddEvent("DragStarting:" + nameof(platformArgs.DragInteraction));
			if (platformArgs.DragSession is not null)
				AddEvent("DragStarting:" + nameof(platformArgs.DragSession));
#elif ANDROID
			if (platformArgs.Sender is not null)
				AddEvent("DragStarting:" + nameof(platformArgs.Sender));
			if (platformArgs.MotionEvent is not null)
				AddEvent("DragStarting:" + nameof(platformArgs.MotionEvent));
#elif WINDOWS
			if (platformArgs.Sender is not null)
				AddEvent("DragStarting:" + nameof(platformArgs.Sender));
			if (platformArgs.DragStartingEventArgs is not null)
				AddEvent("DragStarting:" + nameof(platformArgs.DragStartingEventArgs));
			AddEvent("DragStarting:" + nameof(platformArgs.Handled));
#endif
		}
	}

	void DropCompleted(object sender, DropCompletedEventArgs e)
	{
		if (e.PlatformArgs is PlatformDropCompletedEventArgs platformArgs)
		{
#if IOS || MACCATALYST
			if (platformArgs.Sender is not null)
				AddEvent("DropCompleted:" + nameof(platformArgs.Sender));
			if (platformArgs.DropInteraction is not null)
				AddEvent("DropCompleted:" + nameof(platformArgs.DropInteraction));
			if (platformArgs.DropSession is not null)
				AddEvent("DropCompleted:" + nameof(platformArgs.DropSession));
#elif ANDROID
			if (platformArgs.Sender is not null)
				AddEvent("DropCompleted:" + nameof(platformArgs.Sender));
			if (platformArgs.DragEvent is not null)
				AddEvent("DropCompleted:" + nameof(platformArgs.DragEvent));
#elif WINDOWS
			if (platformArgs.Sender is not null)
				AddEvent("DropCompleted:" + nameof(platformArgs.Sender));
			if (platformArgs.DropCompletedEventArgs is not null)
				AddEvent("DropCompleted:" + nameof(platformArgs.DropCompletedEventArgs));
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
					AddEvent("DragOver:" + nameof(platformArgs.Sender));
				if (platformArgs.DropInteraction is not null)
					AddEvent("DragOver:" + nameof(platformArgs.DropInteraction));
				if (platformArgs.DropSession is not null)
					AddEvent("DragOver:" + nameof(platformArgs.DropSession));
#elif ANDROID
				if (platformArgs.Sender is not null)
					AddEvent("DragOver:" + nameof(platformArgs.Sender));
				if (platformArgs.DragEvent is not null)
					AddEvent("DragOver:" + nameof(platformArgs.DragEvent));
#elif WINDOWS
				if (platformArgs.Sender is not null)
					AddEvent("DragOver:" + nameof(platformArgs.Sender));
				if (platformArgs.DragEventArgs is not null)
					AddEvent("DragOver:" + nameof(platformArgs.DragEventArgs));
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
				AddEvent("Drop:" + nameof(platformArgs.Sender));
			if (platformArgs.DropInteraction is not null)
				AddEvent("Drop:" + nameof(platformArgs.DropInteraction));
			if (platformArgs.DropSession is not null)
				AddEvent("Drop:" + nameof(platformArgs.DropSession));
#elif ANDROID
			if (platformArgs.Sender is not null)
				AddEvent("Drop:" + nameof(platformArgs.Sender));
			if (platformArgs.DragEvent is not null)
				AddEvent("Drop:" + nameof(platformArgs.DragEvent));
#elif WINDOWS
			if (platformArgs.Sender is not null)
				AddEvent("Drop:" + nameof(platformArgs.Sender));
			if (platformArgs.DragEventArgs is not null)
				AddEvent("Drop:" + nameof(platformArgs.DragEventArgs));
#endif
		}
	}
}

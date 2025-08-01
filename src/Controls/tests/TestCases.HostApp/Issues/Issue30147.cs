#if IOS
using CoreGraphics;
using Microsoft.Maui.Handlers;
using UIKit;
#endif

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 30147, "MauiScrollView resets ContentOffset on first layout pass", PlatformAffected.iOS)]
public class Issue30147 : ContentPage
{
	Issue30147CustomScrollView myScroll;
	Label offsetLabel;

	public Issue30147()
	{
		Title = "Issue 30147";

		// Create the Label to display scroll offset
		offsetLabel = new Label
		{
			Text = "0",
			AutomationId = "OffsetLabel",
			FontSize = 18
		};

		// Create Header with offset display
		var headerLayout = new HorizontalStackLayout
		{
			Padding = new Thickness(20),
			BackgroundColor = Colors.LightGray,
			Children =
			{
				new Label
				{
					Text = "Offset: X = ",
					FontSize = 18
				},
				offsetLabel
			}
		};

		// Create the CustomScrollView
		myScroll = new Issue30147CustomScrollView
		{
			Orientation = ScrollOrientation.Horizontal
		};

		// Add the event handler for offset changes
		myScroll.OffsetChanged += (s, e) =>
		{
			offsetLabel.Text = e.X.ToString();
		};

		// Create the content for the scroll view
		var scrollContent = new StackLayout();
		scrollContent.Add(new BoxView { Color = Colors.Red, HeightRequest = 300, WidthRequest = 2000 });

		myScroll.Content = scrollContent;

		// Create the Grid layout
		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		grid.Add(headerLayout, 0, 0);
		grid.Add(myScroll, 0, 1);

		// Set the page content
		Content = grid;
	}
}

public class Issue30147ScrollOffsetChangedEventArgs : EventArgs
{
	public double X { get; }
	public double Y { get; }

	public Issue30147ScrollOffsetChangedEventArgs(double x, double y)
	{
		X = x;
		Y = y;
	}
}

// Custom ScrollView class for tracking offset changes
public class Issue30147CustomScrollView : ScrollView
{
	public event EventHandler<Issue30147ScrollOffsetChangedEventArgs> OffsetChanged;

	// Raise the event from platform-specific code
	internal void RaiseOffsetChanged(double x, double y)
	{
		OffsetChanged?.Invoke(this, new Issue30147ScrollOffsetChangedEventArgs(x, y));
	}
}

#if IOS
public class Issue30147CustomMauiScrollView : Microsoft.Maui.Platform.MauiScrollView
{
	CGPoint _previousOffset = new(-1, -1);
	Issue30147CustomScrollView _virtualView;

	public Issue30147CustomMauiScrollView(Issue30147CustomScrollView virtualView)
	{
		_virtualView = virtualView;
	}

	public override void SetContentOffset(CGPoint contentOffset, bool animated)
	{
		base.SetContentOffset(contentOffset, animated);
		NotifyOffsetChanged(contentOffset);
	}

	public override CGPoint ContentOffset
	{
		get => base.ContentOffset;
		set
		{
			base.ContentOffset = value;
			NotifyOffsetChanged(value);
		}
	}

	void NotifyOffsetChanged(CGPoint offset)
	{
		if (_previousOffset.X != offset.X || _previousOffset.Y != offset.Y)
		{
			_previousOffset = offset;
			_virtualView?.RaiseOffsetChanged(offset.X, offset.Y);
		}
	}
}

public class Issue30147CustomScrollViewHandler : ScrollViewHandler
{
	bool _initialOffsetApplied = false;

	protected override UIScrollView CreatePlatformView()
	{
		if (VirtualView is Issue30147CustomScrollView customScrollView)
		{
			return new Issue30147CustomMauiScrollView(customScrollView);
		}

		return base.CreatePlatformView();
	}

	public override void PlatformArrange(Rect frame)
	{
		base.PlatformArrange(frame);

		if (!_initialOffsetApplied)
		{
			PlatformView.ContentOffset = new CGPoint(500, 0);
			_initialOffsetApplied = true;
		}
	}
}
#endif
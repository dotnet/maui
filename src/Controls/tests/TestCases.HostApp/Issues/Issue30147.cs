using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

#if IOS
using UIKit;
using CoreGraphics;
using Microsoft.Maui.Handlers;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30147, "MauiScrollView resets ContentOffset on first layout pass", PlatformAffected.UWP)]
public class Issue30147 : ContentPage
{
    CustomScrollView myScroll;
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
        myScroll = new CustomScrollView
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
    
    // The custom ScrollView class for tracking offset changes
    public class CustomScrollView : ScrollView
    {
        public event EventHandler<ScrollOffsetChangedEventArgs> OffsetChanged;

        // Raise the event from platform-specific code
        internal void RaiseOffsetChanged(double x, double y)
        {
            OffsetChanged?.Invoke(this, new ScrollOffsetChangedEventArgs(x, y));
        }
    }

    public class ScrollOffsetChangedEventArgs : EventArgs
    {
        public double X { get; }
        public double Y { get; }

        public ScrollOffsetChangedEventArgs(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}

#if IOS
public class CustomMauiScrollView : Microsoft.Maui.Platform.MauiScrollView
{
    CGPoint _previousOffset = new(-1, -1);
    Issue30147.CustomScrollView _virtualView;

    public CustomMauiScrollView(Issue30147.CustomScrollView virtualView)
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

public class CustomScrollViewHandler : ScrollViewHandler
{
    bool _initialOffsetApplied = false;
    
    protected override UIScrollView CreatePlatformView()
    {
        if (VirtualView is Issue30147.CustomScrollView customScrollView)
        {
            return new CustomMauiScrollView(customScrollView);
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
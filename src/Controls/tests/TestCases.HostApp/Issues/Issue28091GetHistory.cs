using System.Text;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Performance;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, "28091GetHistory", "Add Layout Performance Profiler (GetHistory)", PlatformAffected.All)]
public class Issue28091GetHistory : ContentPage
{
	readonly Rectangle _rectangle; 
	readonly Label _historyLabel; 
	
	public Issue28091GetHistory()
    {
        // Rectangle inside a Border
        _rectangle = new Rectangle
        {
	        AutomationId = "ResizableRectangle",
	        Fill = Colors.Blue,
	        WidthRequest = 100,
	        HeightRequest = 100
        };

        // History Label
        _historyLabel = new Label
        {
	        AutomationId = "HistoryLabel",
            Text = "Performance metrics will appear here",
            FontSize = 16,
            HorizontalOptions = LayoutOptions.Center,
        };

        // Buttons
        var increaseWidthButton = new Button
        {
	        AutomationId = "IncreaseWidthButton",
            Text = "Increase Width",
            HorizontalOptions = LayoutOptions.Center,
        };
        increaseWidthButton.Clicked += OnIncreaseWidthClicked;

        var decreaseWidthButton = new Button
        {
	        AutomationId = "DecreaseWidthButton",
            Text = "Decrease Width",
            HorizontalOptions = LayoutOptions.Center,
        };
        decreaseWidthButton.Clicked += OnDecreaseWidthClicked;

        var increaseHeightButton = new Button
        {
            Text = "Increase Height",
            HorizontalOptions = LayoutOptions.Center,
            AutomationId = "IncreaseHeightButton"
        };
        increaseHeightButton.Clicked += OnIncreaseHeightClicked;

        var decreaseHeightButton = new Button
        {
            Text = "Decrease Height",
            HorizontalOptions = LayoutOptions.Center,
            AutomationId = "DecreaseHeightButton"
        };
        decreaseHeightButton.Clicked += OnDecreaseHeightClicked;

        var showHistoryButton = new Button
        {
            Text = "Show History",
            HorizontalOptions = LayoutOptions.Center,
            AutomationId = "ShowHistoryButton"
        };
        showHistoryButton.Clicked += OnShowHistoryClicked;

        // Layout
        Content = new VerticalStackLayout
        {
            Spacing = 10,
            Padding = new Thickness(20),
            Children =
            {
                _rectangle,
                increaseWidthButton,
                decreaseWidthButton,
                increaseHeightButton,
                decreaseHeightButton,
                showHistoryButton,
                _historyLabel
            }
        };
    }

    void OnIncreaseWidthClicked(object sender, EventArgs e)
    {
        using var tracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Rectangle");
        _rectangle.WidthRequest += 10;
    }

    void OnDecreaseWidthClicked(object sender, EventArgs e)
    {
        using var tracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Rectangle");
        if (_rectangle.WidthRequest >= 20)
        {
            _rectangle.WidthRequest -= 10;
        }
    }

    void OnIncreaseHeightClicked(object sender, EventArgs e)
    {
        using var tracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Rectangle");
        _rectangle.HeightRequest += 10;
    }

    void OnDecreaseHeightClicked(object sender, EventArgs e)
    {
        using var tracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Rectangle");
        if (_rectangle.HeightRequest >= 20)
        {
            _rectangle.HeightRequest -= 10;
        }
    }

    void OnShowHistoryClicked(object sender, EventArgs e)
    {
        var history = PerformanceProfiler.GetHistory("Rectangle");
        var layoutHistory = new List<LayoutUpdate>(history.Layout ?? Array.Empty<LayoutUpdate>());
        var sb = new StringBuilder();
       
        foreach (var update in layoutHistory)
        {
            sb.AppendLine($"Pass: {update.PassType}, Element: {update.Element}, Duration: {update.TotalTime:F2}ms, Time: {update.TimestampUtc}");
        }
        
        _historyLabel.Text = sb.ToString();
    }
}
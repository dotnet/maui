using Microsoft.Maui.Controls.Shapes;
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33241, "StackLayout fails to render content while applying Clip, and the layout is placed inside a Border with Background", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue33241 : ContentPage
{
	public Issue33241()
	{
		var layout =
    	new Border
    	{
        	Background = Colors.SkyBlue,
        	Padding = 10,
        	Content =
            new Issue33241_CustomView
            {
                Background = Colors.Red,
				AutomationId = "CustomView",
                Padding = 10
            }
     	};
		Content = layout;
	}
}

public class Issue33241_CustomView : StackLayout
{
	protected override void OnSizeAllocated(double width, double height)
	{
		this.Clip = new RoundRectangleGeometry { Rect = new Rect(0, 0, width, height) };
		base.OnSizeAllocated(width, height);
	}
}
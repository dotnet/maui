using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.None, 0, "Border and shadow drawing verifications", PlatformAffected.All)]

public partial class BorderAndShadowDrawingVerifications : ContentPage
{
	public BorderAndShadowDrawingVerifications()
	{
		InitializeComponent();
	}

	private void ChangeSizeClicked(object sender, EventArgs e)
	{
		var button = TheButton;
		button.CancelAnimations();
		var targetHeight = button.HeightRequest == 200.0 ? 400.0 : 200.0;
		button.Animate("Height", new Animation(v => button.HeightRequest = v, button.Height, targetHeight, Easing.Linear));
	}
	
	private void ChangeShadowClicked(object sender, EventArgs e)
	{
		var border = (Border)TheButton.Parent;
		border.Shadow.Radius += 16;
	}

	private void ChangeShapeClicked(object sender, EventArgs e)
	{
		var border = (Border)TheButton.Parent;
		border.StrokeShape = new Ellipse();
	}

	private void ChangeClipClicked(object sender, EventArgs e)
	{
		var border = (Border)TheButton.Parent;
		border.Clip = new RoundRectangleGeometry(40, new Rect(0, 0, 80, 80));
	}
	
	private void ShowHideClicked(object sender, EventArgs e)
	{
		var button = TheOtherButton;
		button.IsVisible = !button.IsVisible;
	}
}

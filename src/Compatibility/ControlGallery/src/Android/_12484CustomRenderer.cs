using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Android;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using AView = Android.Views.View;

[assembly: ExportRenderer(typeof(Issue12484CustomView), typeof(_12484CustomRenderer))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Android
{
	public class _12484CustomRenderer : ViewRenderer<Issue12484CustomView, AView>
	{
		public _12484CustomRenderer(global::Android.Content.Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Issue12484CustomView> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement.Children[0] is Issue12484CustomView.Issue12484Template t &&
				t.Content is StackLayout g)
			{
				var label = new Label
				{
					AutomationId = "Success",
					Text = "Success",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};

				g.Children.Add(label);
			}
		}
	}
}
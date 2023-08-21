//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7111, "[Bug] [WPF] Label with a predefined FontSize value throws an exception", PlatformAffected.WPF)]
	public class Issue7111 : TestContentPage
	{

		protected override void Init()
		{
			var stack = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};

			var labelBody = new Label
			{
				Text = "If you see this, things didn't crash and it worked",
				FontSize = Device.GetNamedSize(NamedSize.Body, typeof(Label))
			};

			var labelCaption = new Label
			{
				Text = "If you see this, things didn't crash and it worked",
				FontSize = Device.GetNamedSize(NamedSize.Caption, typeof(Label))
			};

			var labelHeader = new Label
			{
				Text = "If you see this, things didn't crash and it worked",
				FontSize = Device.GetNamedSize(NamedSize.Header, typeof(Label))
			};

			var labelSubtitle = new Label
			{
				Text = "If you see this, things didn't crash and it worked",
				FontSize = Device.GetNamedSize(NamedSize.Subtitle, typeof(Label))
			};

			var labelTitle = new Label
			{
				Text = "If you see this, things didn't crash and it worked",
				FontSize = Device.GetNamedSize(NamedSize.Title, typeof(Label))
			};

			stack.Children.Add(labelBody);
			stack.Children.Add(labelCaption);
			stack.Children.Add(labelHeader);
			stack.Children.Add(labelSubtitle);
			stack.Children.Add(labelTitle);

			Content = stack;
		}
	}
}
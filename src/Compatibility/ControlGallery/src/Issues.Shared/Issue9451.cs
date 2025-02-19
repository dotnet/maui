using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 9451,
		"[Bug] RelativeLayout Constraint can not go back to zero", PlatformAffected.All)]
	public class Issue9451 : TestContentPage
	{
		public StackLayout StackLayout { get; set; }
		public Button TriggerButton { get; set; }

		protected override void Init()
		{
			var relativeLayout = new Compatibility.RelativeLayout() { WidthRequest = 400, HeightRequest = 400 };
			StackLayout = new StackLayout() { BackgroundColor = Colors.Red };

			TriggerButton = new Button() { Text = "Set View Width To Zero" };

			StackLayout.Children.Add(TriggerButton);

			relativeLayout.Children.Add(StackLayout,
				Compatibility.Constraint.Constant(0),
				Compatibility.Constraint.Constant(0),
				Compatibility.Constraint.RelativeToParent(x => x.Width / 2),
				Compatibility.Constraint.RelativeToParent(y => y.Height));

			Content = relativeLayout;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			TriggerButton.Clicked += Button_Clicked;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			TriggerButton.Clicked -= Button_Clicked;
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			Compatibility.RelativeLayout.SetWidthConstraint(StackLayout, Compatibility.Constraint.Constant(0.0));
		}
	}
}

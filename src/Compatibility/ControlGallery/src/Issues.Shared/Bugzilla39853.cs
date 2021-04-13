using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39853, "BorderRadius ignored on UWP", PlatformAffected.UWP)]
	public class Bugzilla39853 : TestContentPage
	{
		public class RoundedButton : Microsoft.Maui.Controls.Button
		{
			public RoundedButton(int radius)
			{
				base.CornerRadius = radius;
				base.WidthRequest = 2 * radius;
				base.HeightRequest = 2 * radius;
				HorizontalOptions = LayoutOptions.Center;
				VerticalOptions = LayoutOptions.Center;
				BackgroundColor = Colors.Aqua;
				BorderColor = Colors.White;
				TextColor = Colors.Purple;
				Text = "YAY";
				//Image = new FileImageSource { File = "crimson.jpg" };
			}

			public new int CornerRadius
			{
				get
				{
					return base.CornerRadius;
				}

				set
				{
					base.WidthRequest = 2 * value;
					base.HeightRequest = 2 * value;
					base.CornerRadius = value;
				}
			}

			public new double WidthRequest
			{
				get
				{
					return base.WidthRequest;
				}

				set
				{
					base.WidthRequest = value;
					base.HeightRequest = value;
					base.CornerRadius = ((int)value) / 2;
				}
			}

			public new double HeightRequest
			{
				get
				{
					return base.HeightRequest;
				}

				set
				{
					base.WidthRequest = value;
					base.HeightRequest = value;
					base.CornerRadius = ((int)value) / 2;
				}
			}
		}

		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label
			{
				Text = "The button below should be round. "
												+ "If it has any right angles, the test has failed."
			};

			layout.Children.Add(instructions);
			layout.Children.Add(new RoundedButton(100));

			Content = layout;
		}
	}
}

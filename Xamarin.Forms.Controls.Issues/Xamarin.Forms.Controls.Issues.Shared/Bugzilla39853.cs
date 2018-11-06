using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39853, "BorderRadius ignored on UWP", PlatformAffected.UWP)]
	public class Bugzilla39853 : TestContentPage
	{
		public class RoundedButton : Xamarin.Forms.Button
		{
			public RoundedButton(int radius)
			{
#pragma warning disable 0618
				base.BorderRadius = radius;
#pragma warning restore
				base.WidthRequest = 2 * radius;
				base.HeightRequest = 2 * radius;
				HorizontalOptions = LayoutOptions.Center;
				VerticalOptions = LayoutOptions.Center;
				BackgroundColor = Color.Aqua;
				BorderColor = Color.White;
				TextColor = Color.Purple;
				Text = "YAY";
				//Image = new FileImageSource { File = "crimson.jpg" };
			}

			public new int BorderRadius
			{
				get
				{
#pragma warning disable 0618
					return base.BorderRadius;
#pragma warning restore
				}

				set
				{
					base.WidthRequest = 2 * value;
					base.HeightRequest = 2 * value;
#pragma warning disable 0618
					base.BorderRadius = value;
#pragma warning restore
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
#pragma warning disable 0618
					base.BorderRadius = ((int)value) / 2;
#pragma warning restore
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
#pragma warning disable 0618
					base.BorderRadius = ((int)value) / 2;
#pragma warning restore
				}
			}
		}

		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label { Text = "The button below should be round. " 
												+ "If it has any right angles, the test has failed."};

			layout.Children.Add(instructions);
			layout.Children.Add(new RoundedButton(100));

			Content = layout;
		}
	}
}

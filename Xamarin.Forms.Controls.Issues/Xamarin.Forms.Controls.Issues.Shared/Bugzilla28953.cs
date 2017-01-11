using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System;
using System.Threading.Tasks;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 28953, "Device.StartTimer (still) behaves differently on different platforms", PlatformAffected.All)]
	public class Bugzilla28953 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		int count = 0, count2 = 0;
		Label label2, label3;
		bool shouldStop, shouldStop2;

		protected override void Init()
		{
			var stackLayout = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				VerticalOptions = LayoutOptions.Center,
				Spacing = 20
			};

			var label1 = new Label
			{
				Text = "Click Start to start counting with a timer. Click Stop to reset. Both timers update text in UI thread."
			};
			stackLayout.Children.Add(label1);

			label2 = new Label
			{
				Text = count.ToString(),
				HorizontalTextAlignment = TextAlignment.Center,
			};
			stackLayout.Children.Add(label2);

			label3 = new Label
			{
				Text = count2.ToString(),
				HorizontalTextAlignment = TextAlignment.Center,
			};
			stackLayout.Children.Add(label3);

			var button = new Button
			{
				Text = "Start"
			};
			button.Clicked += Button_Clicked;
			stackLayout.Children.Add(button);

			var button2 = new Button
			{
				Text = "Start (in background thread)"
			};
			button2.Clicked += Button_Clicked2;
			stackLayout.Children.Add(button2);

			Content = stackLayout;
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			if (button.Text == "Start")
			{
				(sender as Button).Text = "Stop";
				shouldStop = false;

				Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
				{
					label2.Text = count.ToString();
					count++;
					return !shouldStop;
				});
			}
			else
			{
				button.Text = "Start";
				shouldStop = true;
				count = 0;
			}
		}

		private void Button_Clicked2(object sender, EventArgs e)
		{
			var button = sender as Button;
			if (button.Text == "Start (in background thread)")
			{
				(sender as Button).Text = "Stop (in background thread)";
				shouldStop2 = false;

				Task.Run(() =>
				{
					Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
					{
						label3.Text = count2.ToString();
						count2++;
						return !shouldStop2;
					});
				});
			}
			else
			{
				button.Text = "Start (in background thread)";
				shouldStop2 = true;
				count2 = 0;
			}
		}
	}
}
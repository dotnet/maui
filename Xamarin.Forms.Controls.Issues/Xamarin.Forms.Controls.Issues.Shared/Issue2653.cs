using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2653, "[UWP] Grid insert z-order on UWP broken in Forms 3",
		PlatformAffected.UWP)]
	public class Issue2653 : TestContentPage
	{
		BoxView bv = null;
		Grid layout = null;
		const string ButtonText = "Insert Box View";
		const string MoveUp = "Move Box View Up";
		const string MoveDown = "Move Box View Down";
		const string BoxViewIsOverlappingButton = "Box View Is Overlapping";
		const string Success = "BoxView Not Overlapping";
		string instructions = $"Click {ButtonText}. If Box View shows up over me test has failed.";
		const string TestForButtonClicked = "Test For Clicked";


		protected override void Init()
		{
			layout = new Grid { BackgroundColor = Color.Red, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand };

			layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
			layout.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star });
			layout.Children.Add(new Button()
			{
				Text = ButtonText,
				BackgroundColor = Color.Green,
				Margin = 10,
				TextColor = Color.White,
				Command = new Command(() =>
				{
					if (!AddBoxView())
					{
						layout.Children.Remove(bv);
					}
				})
			});

			this.On<iOS>().SetUseSafeArea(true);

			var labelInstructions = new Label { Text = instructions };
			
			Content = new StackLayout()
			{
				Children =
				{
					labelInstructions,
					new Button(){ Text = MoveUp, AutomationId = MoveUp, Command = new Command(() =>
					{
						AddBoxView();
						layout.RaiseChild(bv);
					}),  HeightRequest = 45},
					new Button(){ Text = MoveDown, AutomationId = MoveDown, Command = new Command(() =>
					{
						AddBoxView();
						layout.LowerChild(bv);
					}),  HeightRequest = 45},
					layout,
					new Button(){ Text = TestForButtonClicked, Command = new Command(() =>
					{
						if(!layout.Children.Contains(bv))
						{
							labelInstructions.Text = Success;
						}
						else
						{
							labelInstructions.Text = BoxViewIsOverlappingButton;
						}
					}),  HeightRequest = 45}
				}
			};
		}

		bool AddBoxView()
		{
			if (bv != null && layout.Children.Contains(bv))
				return false;

			bv = new BoxView
			{
				Color = Color.Purple,
				WidthRequest = 3000,
				HeightRequest = 3000,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			layout.Children.Insert(0, bv);
			return true;
		}

#if UITEST
		[Test]
		public void ZIndexWhenInsertingChildren()
		{
			RunningApp.WaitForElement(x => x.Marked(ButtonText));
			RunningApp.Tap(x => x.Marked(ButtonText));
			RunningApp.Tap(x => x.Marked(ButtonText));
			RunningApp.Tap(x => x.Marked(TestForButtonClicked));
			RunningApp.WaitForElement(x => x.Marked(Success));
		}


		[Test]
		public void MoveUpAndMoveDown()
		{
			RunningApp.WaitForElement(x => x.Marked(MoveUp));
			RunningApp.Tap(x => x.Marked(ButtonText));
			RunningApp.Tap(x => x.Marked(TestForButtonClicked));
			RunningApp.WaitForElement(x => x.Marked(BoxViewIsOverlappingButton));

			RunningApp.Tap(x => x.Marked(MoveUp));
			RunningApp.Tap(x => x.Marked(MoveDown));
			RunningApp.Tap(x => x.Marked(ButtonText));
			RunningApp.Tap(x => x.Marked(TestForButtonClicked));
			RunningApp.WaitForElement(x => x.Marked(Success));
		}
#endif
	}
}

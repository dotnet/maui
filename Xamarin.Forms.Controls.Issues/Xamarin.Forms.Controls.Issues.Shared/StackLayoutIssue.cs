using System.Diagnostics;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.None, 0, "StackLayout issue", PlatformAffected.All, NavigationBehavior.PushModalAsync)]
	public class StackLayoutIssue : TestContentPage
	{
		protected override void Init ()
		{
			var logo = new Image {
				Source = "cover1.jpg",
				WidthRequest = 20,
				HeightRequest = 20,
				VerticalOptions = LayoutOptions.FillAndExpand,
			};

			var winPrizeLabel = new Label {
				Text = "Win a Xamarin Prize",
#pragma warning disable 618
				XAlign = TextAlignment.Center,
#pragma warning restore 618

#pragma warning disable 618
				YAlign = TextAlignment.Center,
#pragma warning restore 618
				VerticalOptions = LayoutOptions.FillAndExpand
			};

#pragma warning disable 618
			Device.OnPlatform (iOS: () => winPrizeLabel.Font = Font.OfSize ("HelveticaNeue-UltraLight", NamedSize.Large));
#pragma warning restore 618

			StackLayout form = MakeForm ();

			var spinButton = new Button {
				Text = "Spin"
			};

			var mainLayout = new StackLayout {
				Children = {
					logo,
					winPrizeLabel,
					form,
					spinButton
				}
			};

			Content = mainLayout;
			Padding = new Thickness (50);
		}

#if UITEST
		[Test]
		public void StackLayoutIssueTestsAllElementsPresent ()
		{
			// TODO: Fix ME

			//var images = App.Query (PlatformQueries.Images);
			//Assert.AreEqual (2, images.Length);

			//App.WaitForElement (q => q.Marked ("Win a Xamarin Prize"));
			//App.WaitForElement (PlatformQueries.EntryWithPlaceholder ("Full Name"));
			//App.WaitForElement (PlatformQueries.EntryWithPlaceholder ("Email"));
			//App.WaitForElement (PlatformQueries.EntryWithPlaceholder ("Company"));
			//App.WaitForElement (q => q.Marked ("Completed Azure Mobile Services Challenge?"));

			//var switches = App.Query (q => q.Raw ("Switch"));
			//Assert.AreEqual (1, switches.Length);

			//App.WaitForElement (q => q.Button ("Spin"));
			//App.Screenshot ("All elements present");

			Assert.Inconclusive ("Fix Test");
		}
#endif

		StackLayout MakeForm ()
		{
			var nameEntry = new Entry {
				Placeholder = "Full Name"
			};
			var emailEntry = new Entry {
				Placeholder = "Email"
			};

			var companyEntry = new Entry {
				Placeholder = "Company"
			};

			var switchContainer = new StackLayout {
				Orientation = StackOrientation.Horizontal
			};

			var switchLabel = new Label {
				Text = "Completed Azure Mobile Services Challenge?"
			};
			var switchElement = new Switch ();

			switchContainer.Children.Add (switchLabel);
			switchContainer.Children.Add (switchElement);

			var entryContainer = new StackLayout {
				Children = {
					nameEntry,
					emailEntry,
					companyEntry,
					switchContainer
				},
				MinimumWidthRequest = 50
			};

			var qrButton = new Image {
				Source = "cover1.jpg",
				WidthRequest = 100,
				HeightRequest = 100
			};

			var result = new StackLayout {
				Orientation = StackOrientation.Horizontal
			};

			result.Children.Add (entryContainer);
			result.Children.Add (qrButton);

			result.SizeChanged += (sender, args) => {
				Debug.WriteLine (result.Bounds);
			};

			return result;
		}
	}
}

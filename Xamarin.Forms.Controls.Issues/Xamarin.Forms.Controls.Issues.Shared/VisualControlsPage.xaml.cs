
using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	public partial class VisualControlsPage : TestShell
	{
		bool isVisible = false;
		double percentage = 0.0;

		public VisualControlsPage()
		{
#if APP
			InitializeComponent();
#endif
		}
		protected override void Init()
		{
			BindingContext = this;
		}

		public double PercentageCounter
		{
			get { return percentage; }
			set
			{
				percentage = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(Counter));
			}
		}

		public double Counter => percentage * 10;

		protected override void OnAppearing()
		{
			isVisible = true;

			base.OnAppearing();

			Device.StartTimer(TimeSpan.FromSeconds(1), () =>
			{
				var progress = PercentageCounter + 0.1;
				if (progress > 1)
					progress = 0;

				PercentageCounter = progress;

				return isVisible;
			});
		}

		protected override void OnDisappearing()
		{
			isVisible = false;

			base.OnDisappearing();
		}




		[Preserve(AllMembers = true)]
		[Issue(IssueTracker.Github, 4435, "Visual Gallery Loads",
			PlatformAffected.iOS | PlatformAffected.Android)]
#if UITEST
		[NUnit.Framework.Category(UITestCategories.Visual)]
#endif
		public class Issue4435 : VisualControlsPage
		{
			protected override void Init()
			{
			}

#if UITEST && !__WINDOWS__
			[Test]
			public void LoadingVisualGalleryPageDoesNotCrash()
			{
				RunningApp.WaitForElement("Activity Indicators");
			}

			[Test]
			[NUnit.Framework.Category(UITestCategories.ManualReview)]
			public void DisabledButtonTest()
			{
				TapInFlyout("Disabled Button Test");
				RunningApp.WaitForElement("If either button looks odd this test has failed.");
				RunningApp.Screenshot("If either button looks off (wrong shadow, border drawn inside button) the test has failed.");
			}
#endif
		}
	}
}

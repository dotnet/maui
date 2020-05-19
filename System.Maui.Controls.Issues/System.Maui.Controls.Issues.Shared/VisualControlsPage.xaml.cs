
using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	public partial class VisualControlsPage : ContentPage
	{
		bool isVisible = false;
		double percentage = 0.0;

		public VisualControlsPage()
		{
#if APP
			InitializeComponent();
#endif

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
	}



	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4435, "Visual Gallery Loads",
		PlatformAffected.iOS | PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Visual)]
#endif
	public class Issue4435 : TestNavigationPage
	{
		const string Success = "Success";
		Label successLabel;
		protected override void Init()
		{
			var vg = new VisualControlsPage();
			successLabel = new Label();
			vg.Appearing += Vg_Appearing;
			PushAsync(new ContentPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						successLabel
					}
				}
			});

			PushAsync(vg);
		}

		private void Vg_Appearing(object sender, EventArgs e)
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				PopAsync();
				successLabel.Text = Success;
			});
		}

#if UITEST && !__WINDOWS__
		[Test]
		public void LoadingVisualGalleryPageDoesNotCrash()
		{
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}

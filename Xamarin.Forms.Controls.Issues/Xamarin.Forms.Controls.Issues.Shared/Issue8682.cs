using System;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8682, "[Bug] [UWP] NullReferenceException when call SavePropertiesAsync method off the main thread", PlatformAffected.UWP)]
	public class Issue8682 : TestContentPage
	{
		const string Run = "Save properties";
		const string Success = "Success";

		Label _result;

		protected override void Init()
		{
			var instructions = new Label
			{
				Text = $"Tap the button marked {Run}. If the Label below reads {Success} then the test has passed."
			};

			_result = new Label();

			var testButton = new Button
			{
				Text = Run
			};
			testButton.Clicked += OnTestButtonClicked;

			var layout = new StackLayout();
			layout.Children.Add(instructions);
			layout.Children.Add(_result);
			layout.Children.Add(testButton);

			Content = layout;
		}

		async void OnTestButtonClicked(object sender, EventArgs e)
		{
			_result.Text = await SavePropertiesAsyncOffMainThread();
		}

		async Task<string> SavePropertiesAsyncOffMainThread()
		{
			return await Task.Run(async () => 
			{
				try
				{
					await Application.Current.SavePropertiesAsync();

					return Success;
				} 
				catch (Exception e)
				{
					return $"Test failed: {e.Message}.";
				}
			});
		}

#if UITEST
		[Test]
		public void SavePropertiesAsyncOffMainThreadDoesNotCrash() 
		{
			RunningApp.WaitForElement(Run);
			RunningApp.Tap(Run);
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}

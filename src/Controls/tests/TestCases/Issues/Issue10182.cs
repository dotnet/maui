using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10182, "[Bug] Exception Ancestor must be provided for all pushes except first", PlatformAffected.Android, NavigationBehavior.SetApplicationRoot)]
	public class Issue10182 : TestContentPage
	{
		public Issue10182()
		{

		}

		protected override void Init()
		{
			Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "Starting Activity to Test Changing Page on Resume. If success label shows up test has passed."
					}
				}
			};

#if !UITEST
#pragma warning disable CS0612 // Type or member is obsolete
			Device.BeginInvokeOnMainThread(() =>
			{
				DependencyService.Get<IMultiWindowService>().OpenWindow(this.GetType());
			});
#pragma warning restore CS0612 // Type or member is obsolete
#endif

		}

		public class Issue10182SuccessPage : ContentPage
		{
			public Issue10182SuccessPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "Success.",
							AutomationId = "Success"
						}
					}
				};
			}
		}
	}
}

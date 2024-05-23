using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
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
			Device.BeginInvokeOnMainThread(() =>
			{
				DependencyService.Get<IMultiWindowService>().OpenWindow(this.GetType());
			});
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

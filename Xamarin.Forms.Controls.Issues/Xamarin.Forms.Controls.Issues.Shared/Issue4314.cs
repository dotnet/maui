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
	[Category(UITestCategories.ContextActions)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4314, "When ListView items is removed and it is empty, Xamarin Forms crash", PlatformAffected.iOS)]
	public class Issue4314 : TestNavigationPage // or TestMasterDetailPage, etc ...
	{
		const string Success = "Success";
#if !UITEST
		MessagesViewModel viewModel;
		protected override void Init()
		{
			var page = new ContextActionsGallery(false, true, 2) { Title = "Swipe and delete both" };
			viewModel = page.BindingContext as MessagesViewModel;
			viewModel.Messages.CollectionChanged += (s, e) =>
			{
				if (viewModel.Messages.Count == 0)
				{
					Navigation.PushAsync(new ContentPage { Title = "Success", Content = new Label { Text = Success } });
				}
			};
			Navigation.PushAsync(page);
		}
#else
		protected override void Init()
		{
		}
#endif
#if UITEST && __IOS__
		[Test]
		public void Issue4341Test() 
		{
			RunningApp.WaitForElement(c=> c.Marked("Email"));
			RunningApp.ActivateContextMenu("Subject Line 0");
			RunningApp.WaitForElement("Delete");
			RunningApp.Tap("Delete");
			RunningApp.ActivateContextMenu("Subject Line 1");
			RunningApp.Tap("Delete");
			RunningApp.WaitForElement(c=> c.Marked(Success));
			RunningApp.Back();
			RunningApp.WaitForElement(c => c.Marked("Email"));
			RunningApp.SwipeRightToLeft();
		}
#endif
	}
}
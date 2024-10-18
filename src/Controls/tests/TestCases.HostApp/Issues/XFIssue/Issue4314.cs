//using Microsoft.Maui.Controls.CustomAttributes;
//using Microsoft.Maui.Controls.Internals;

//namespace Maui.Controls.Sample.Issues;

//[Issue(IssueTracker.Github, 4314, "When ListView items is removed and it is empty, Xamarin Forms crash", PlatformAffected.iOS)]
//public class Issue4314 : TestNavigationPage
//{
//	const string Success = "Success";

//	MessagesViewModel viewModel;
//	protected override void Init()
//	{
//		var page = new ContextActionsGallery(false, true, 2) { Title = "Swipe and delete both" };
//		viewModel = page.BindingContext as MessagesViewModel;
//		viewModel.Messages.CollectionChanged += (s, e) =>
//		{
//			if (viewModel.Messages.Count == 0)
//			{
//				Navigation.PushAsync(new ContentPage { Title = "Success", Content = new Label { Text = Success } });
//			}
//		};
//		Navigation.PushAsync(page);
//	}
//}
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44338, "Tapping off of a cell with an open context action causes a crash in iOS 10", PlatformAffected.iOS)]
	public class Bugzilla44338 : TestContentPage
	{
		protected override void Init()
		{
			string[] items = new string[] { "A", "B", "C" };
			Content = new ListView
			{
				ItemsSource = items,
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, ".");
					var view = new ViewCell
					{
						View = new StackLayout
						{
							Children =
							{
								label
							}
						}
					};
					view.ContextActions.Add(new MenuItem
					{
						Text = "Action",
						Command = new Command(() => DisplayAlert("Alert", "Context Action Pressed", "Close"))
					});
					return view;
				})	
			};
		}
	}
}
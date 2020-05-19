using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;

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
		string[] _items;
		public string[] Items
		{
			get
			{
				if (_items == null)
				{
					_items = new string[] { "A", "B", "C" };
				}

				return _items;
			}
		}

		protected override void Init()
		{
			Content = new ListView
			{
				ItemsSource = Items,
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

#if UITEST && __IOS__
		[Test]
		public void Bugzilla44338Test()
		{
			RunningApp.SwipeRightToLeft(Items.First());
			RunningApp.Tap(Items.Last());
		}
#endif

#if UITEST && __ANDROID__
		[Test]
		public void Bugzilla44338Test()
		{
			RunningApp.TouchAndHold(Items.First());
			RunningApp.Tap(Items.Last());
		}
#endif
	}
}
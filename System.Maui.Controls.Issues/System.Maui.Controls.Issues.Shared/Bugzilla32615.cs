using System;

using Xamarin.Forms.CustomAttributes;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 32615, "OnAppearing is not called on previous page when modal page is popped")]
	public class Bugzilla32615 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		int _counter;
		Label _textField;
		protected override void Init ()
		{
			var btnModal = new Button { AutomationId = "btnModal",  Text = "open",  HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
			btnModal.Clicked += async (sender, e) => await Navigation.PushModalAsync (new Bugzilla32615Page2 ());
			_textField = new Label { AutomationId = "lblCount"};
			var layout = new StackLayout ();
			layout.Children.Add (btnModal);
			layout.Children.Add (_textField);
			// Initialize ui here instead of ctor
			Content = layout;
		}

		protected override void OnAppearing()
		{
			_textField.Text = _counter++.ToString();
		}

		class Bugzilla32615Page2 : ContentPage
		{
			public Bugzilla32615Page2 ()
			{
				var btnPop = new Button { AutomationId = "btnPop",  Text = "pop",  HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
				btnPop.Clicked += async (sender, e) => await Navigation.PopModalAsync ();
				Content = btnPop;
			}

			protected override void OnDisappearing ()
			{
				System.Diagnostics.Debug.WriteLine ("Disappearing Modal");
				base.OnDisappearing ();
			}

			protected override void OnAppearing ()
			{
				System.Diagnostics.Debug.WriteLine ("Appearing Modal");
				base.OnAppearing ();
			}
		}

#if UITEST
		[Test]
		public async Task Bugzilla32615Test ()
		{
			RunningApp.Tap (q => q.Marked ("btnModal"));
			RunningApp.Tap (q => q.Marked ("btnPop"));
			await Task.Delay (1000);
			RunningApp.WaitForElement ("1");
		}
#endif
	}
}

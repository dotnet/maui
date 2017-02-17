using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Threading.Tasks;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

// Apply the default category of "Issues" to all of the tests in this assembly
// We use this as a catch-all for tests which haven't been individually categorized
#if UITEST
[assembly: NUnit.Framework.Category("Issues")]
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44940, "[WinRT/UWP] ScrollView.ScrollToAsync does not return from await", PlatformAffected.WinRT)]
	public class Bugzilla44940 : TestContentPage
	{
		Label _statusLabel;
		Entry _firstEntry;
		Entry _secondEntry;
		StackLayout _verticalStackLayout;
		ScrollView _scrollView;

		protected override void Init()
		{
			_statusLabel = new Label
			{
				Text = "With focus on first Entry, hit Return key",
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				LineBreakMode = LineBreakMode.WordWrap
			};

			_firstEntry = new Entry
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand,
			};

			_secondEntry = new Entry
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand,
			};

			_firstEntry.Completed += FirstEntryCompleted;

			_verticalStackLayout = new StackLayout
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Padding = new Thickness(0, 0, 0, 0),
				Margin = new Thickness(0, 0, 0, 0),
				Spacing = 5,
				Children =
				{
					_statusLabel,
					_firstEntry,
					_secondEntry
				}
			};

			_scrollView = new ScrollView
			{
				Orientation = ScrollOrientation.Vertical,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Margin = new Thickness(10, 5, 10, 0),
				Padding = new Thickness(0, 0, 0, 0),
				Content = _verticalStackLayout
			};

			Content = _scrollView;
			
			Device.BeginInvokeOnMainThread(async () =>
			{
				await Task.Delay(100);
				_firstEntry.Focus();
			});
		}

		async void FirstEntryCompleted(object sender, System.EventArgs e)
		{
			_firstEntry?.Unfocus();
			_statusLabel.Text = "Attempting scroll. Return from await pending...";
			await _scrollView.ScrollToAsync(_secondEntry, ScrollToPosition.MakeVisible, false);
			_statusLabel.Text = "This should be visible on WinRT/UWP";
			_secondEntry?.Focus();
		}
	}
}
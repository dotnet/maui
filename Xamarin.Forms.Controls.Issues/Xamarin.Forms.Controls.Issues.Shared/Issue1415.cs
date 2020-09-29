using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1415,
		"HorizontalTextAlignment=\"Center\" loses alignment when scrolled",
		PlatformAffected.Android)]
	public class Issue1415 : TestContentPage
	{
		[Preserve(AllMembers = true)]
		public class _1415ViewModel
		{
			public ICommand AddMoreCommand { get; set; }
			public ObservableCollection<_1415Item> ListViewItems { get; set; }

			public _1415ViewModel()
			{
				ListViewItems = new ObservableCollection<_1415Item>();
				for (int i = 0; i < 500; i++)
				{
					ListViewItems.Add(new _1415Item() { PropA = $"A {i}" });
				}
			}
		}

		[Preserve(AllMembers = true)]
		public class _1415Item
		{
			public string PropA { get; set; }
		}

		protected override void Init()
		{
			BindingContext = new _1415ViewModel();

			var layout = new StackLayout();

			var instructions =
				"Scroll up and down for a while. The text in the ListView items should remain centered. "
				+ "If any of the text is not centered, this test has failed.";

			var lv = new ListView();
			lv.SetBinding(ListView.ItemsSourceProperty, "ListViewItems");

			lv.ItemTemplate = new DataTemplate(() =>
			{
				var frame = new Frame();

				var sl = new StackLayout();

				for (int n = 0; n < 4; n++)
				{
					var label = new Label { HorizontalTextAlignment = TextAlignment.Center };
					label.SetBinding(Label.TextProperty, "PropA");
					sl.Children.Add(label);
				}

				frame.Content = sl;
				var cell = new ViewCell { View = frame };
				return cell;
			});

			layout.Children.Add(new Label { Text = instructions });
			layout.Children.Add(lv);

			Content = layout;
		}
	}
}
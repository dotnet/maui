using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8345, "[Bug] CollectionView ItemSpacing does not affect the Item spacing on iOS",
		PlatformAffected.iOS)]
	public class Issue8345 : TestContentPage
	{
		protected override void Init()
		{
			var groups = new List<_8345Group>
			{
				new _8345Group() { HeaderText = "Group 1" },
				new _8345Group() { HeaderText = "Group 2" },
				new _8345Group() { HeaderText = "Group 3" }
			};

			var cv = new CollectionView
			{
				IsGrouped = true,
				ItemsSource = groups,
				GroupHeaderTemplate = new DataTemplate(() =>
				{
					var label = new Label
					{
						BackgroundColor = Color.Red
					};
					label.SetBinding(Label.TextProperty, new Binding("HeaderText"));
					return label;
				})
			};

			cv.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 20 };

			var instructions = new Label
			{
				Text = "The CollectionView group headers below should have space between " +
				"them; if they are right next to each other, this test has failed."
			};

			var layout = new StackLayout();
			layout.Children.Add(instructions);
			layout.Children.Add(cv);
			Content = layout;
		}
	}

	[Preserve(AllMembers = true)]
	public class _8345Group : List<_8345Item>
	{
		public string HeaderText { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class _8345Item { }
}

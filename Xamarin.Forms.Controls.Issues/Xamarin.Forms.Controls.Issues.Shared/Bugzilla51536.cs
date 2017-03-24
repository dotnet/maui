using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 51536, "[iOS] Xamarin.Forms ListView Row Height Does Not Adapt")]
	public class Bugzilla51536 : TestContentPage
	{
		protected override void Init()
		{
			const string InstructionsLong = "On iOS, all the list items below will have different height defined by this text, the text " +
										"should be wrapped and take all cell space. If this text is not wrapped and there is a lot of " +
										"whitespace in the cell then this test has failed. This error was happening to ListView with RecycleElement mode " +
										"or when cell has context actions.";

			const string InstructionsShort = "On iOS, all the list items below will have different height defined by this text.";

			var listItems = Enumerable.Range(1, 100).Select(i => new ItemViewModel
																 {
																	 Name = "Item" + i,
																	 Description = i % 2 == 0 ? (InstructionsLong + i) : (InstructionsShort + i)
																 }).ToArray();

			var listView = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				ItemTemplate = new DataTemplate(typeof(ItemViewCell)),
				HasUnevenRows = true,
				ItemsSource = listItems
			};

			Content = listView;
		}

		[Preserve(AllMembers = true)]
		public sealed class ItemViewModel
		{
			public string Name { get; set; }
			public string Description { get; set; }
		}

		[Preserve(AllMembers = true)]
		public sealed class ItemViewCell : ViewCell
		{
			public Label Label1 { get; set; }
			public Label Label2 { get; set; }

			public ItemViewCell()
			{
				var stackLayout = new StackLayout
				{
					Orientation = StackOrientation.Vertical,
					HorizontalOptions = LayoutOptions.StartAndExpand,
					VerticalOptions = LayoutOptions.StartAndExpand
				};

				Label1 = new Label();
				Label2 = new Label { LineBreakMode = LineBreakMode.WordWrap };

				stackLayout.Children.Add(Label1);
				stackLayout.Children.Add(Label2);

				View = stackLayout;
			}

			protected override void OnBindingContextChanged()
			{
				base.OnBindingContextChanged();

				var item = BindingContext as ItemViewModel;

				if (item != null)
				{
					Label1.Text = item.Name;
					Label2.Text = item.Description;
				}
			}
		}
	}
}
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2642, "ControlTemplate resizing issue", PlatformAffected.WPF)]
	public class GitHub2642 : TestContentPage
	{
		public class PresenterWrapper : ContentView
		{
			public PresenterWrapper()
			{
				Content = new ContentPresenter();
			}
		}

		protected override void Init()
		{
			this.ControlTemplate = new ControlTemplate(typeof(PresenterWrapper));

			var grid = new Grid()
			{
				RowDefinitions = new RowDefinitionCollection
					{
						new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) },
						new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
					}
			};

			grid.AddChild(new Label()
			{
				Text = "Header",
				LineBreakMode = LineBreakMode.WordWrap,
				FontSize = 24

			}, 0, 0);

			grid.AddChild(new Label()
			{
				Text = "Lorem ipsum dolor sit amet, sed at etiam graecis. Amet dicta utroque in ius, error vituperatoribus vel ex. " +
					"Cu duo veri aperiam honestatis. Quo sint movet ullamcorper cu, vero vidisse argumentum ne nec, in munere eirmod eum. " +
					"Persius similique reformidans ex mei, cu quo quot nihil mediocrem.",
				LineBreakMode = LineBreakMode.WordWrap

			}, 0, 1);

			Content = grid;
		}
	}
}
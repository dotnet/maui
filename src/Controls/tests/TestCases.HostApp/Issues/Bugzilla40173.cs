using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Button = Microsoft.Maui.Controls.Button;
using ListView = Microsoft.Maui.Controls.ListView;
using ViewCell = Microsoft.Maui.Controls.ViewCell;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 40173, "Android BoxView/Frame not clickthrough in ListView")]
public class Bugzilla40173 : TestContentPage
{
	const string CantTouchButtonId = "CantTouchButtonId";
	const string CanTouchButtonId = "CanTouchButtonId";
	const string ListTapTarget = "ListTapTarget";
	const string CantTouchFailText = "Failed";
	const string CanTouchSuccessText = "ButtonTapped";
	const string ListTapSuccessText = "ItemTapped";

	protected override void Init()
	{
		var outputLabel = new Label() { AutomationId = "outputlabel" };
		var testButton = new Button
		{
			Text = "Can't Touch This",
			AutomationId = CantTouchButtonId
		};

		testButton.Clicked += (sender, args) => outputLabel.Text = CantTouchFailText;

		var boxView = new BoxView
		{
			AutomationId = "nontransparentBoxView",
			Color = Colors.Pink.MultiplyAlpha(0.5f)
		};

		// Bump up the elevation on Android so the Button is covered (FastRenderers)
		boxView.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetElevation(10f);

		var testGrid = new Grid
		{
			AutomationId = "testgrid",
			Children =
			{
				testButton,
				boxView
			}
		};

		// BoxView over Button prevents Button click
		var testButtonOk = new Button
		{
			Text = "Can Touch This",
			AutomationId = CanTouchButtonId
		};

		testButtonOk.Clicked += (sender, args) =>
		{
			outputLabel.Text = CanTouchSuccessText;
		};

		var testGridOk = new Grid
		{
			AutomationId = "testgridOK",
			Children =
			{
				testButtonOk,
				new BoxView
				{
					AutomationId = "transparentBoxView",
					Color = Colors.Pink.MultiplyAlpha(0.5f),
					InputTransparent = true
				}
			}
		};

		var testListView = new ListView();
		var items = new[] { "Foo" };
		testListView.ItemsSource = items;
		testListView.ItemTemplate = new DataTemplate(() =>
		{
			var result = new ViewCell
			{
				View = new Grid
				{
					Children =
					{
						new BoxView
						{
							AutomationId = ListTapTarget,
							Color = Colors.Pink.MultiplyAlpha(0.5f)
						}
					}
				}
			};

			return result;
		});

		testListView.ItemSelected += (sender, args) => outputLabel.Text = ListTapSuccessText;

		Content = new StackLayout
		{
			AutomationId = "Container Stack Layout",
			Children = { outputLabel, testGrid, testGridOk, testListView }
		};
	}
}
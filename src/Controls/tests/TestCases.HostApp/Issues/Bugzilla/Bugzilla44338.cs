using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues;

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
				label.SetBinding(Label.AutomationIdProperty, ".");
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
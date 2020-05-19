using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1386,
		"[iOS] EntryCell within TableView using wrong keyboard",
		PlatformAffected.iOS)]
	public class Issue1386 : TestContentPage
	{
		bool _state;
		TableSection _selectionOne;
		TableSection _selectionTwo;

		protected override void Init()
		{
			var root = new TableRoot();

			var section1 = new TableSection("1")
			{
				new TextCell
				{
					Text = "CHANGE THE SECOND CELL",
					Command = new Command(() =>
					{
						root.Remove(_selectionOne);
						root.Remove(_selectionTwo);

						if (!_state)
						{
							root.Insert(1, _selectionOne);
						}
						else
						{
							root.Insert(1, _selectionTwo);
						}

						_state = !_state;
					})
				}
			};

			root.Add(section1);

			_selectionOne = new TableSection("2")
			{
				new EntryCell
				{
					Label = "Numeric Keyboard",
					Placeholder = "Tap here",
					Keyboard = Keyboard.Numeric
				}
			};

			_selectionTwo = new TableSection("2")
			{
				new EntryCell
				{
					Label = "Plain Keyboard",
					Placeholder = "Tap here",
					Keyboard = Keyboard.Plain,
				}
			};

			Content = new StackLayout
			{
				Padding = new Thickness(0, 50),
				Children = {
					new Label
					{
						Margin = new Thickness (15, 0),
						Text = "1) Tap 'CHANGE THE SECOND CELL' and make sure, that the second cell has numeric keyboard" +
							"\n2) Tap 'CHANGE THE SECOND CELL' again and make sure, that the second cel has plain keyboard"
					},
					new TableView
					{
						Intent = TableIntent.Form,
						Root = root
					}
				}
			};
		}
	}
}

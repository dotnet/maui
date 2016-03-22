using System;

using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 2191, "ToolBarItem not showing as disabled when CanExecute is set to false", PlatformAffected.Android)]
	public class Issue2191 : ContentPage
	{
		public Issue2191 ()
		{
			var stackPanel = new StackLayout { VerticalOptions = LayoutOptions.End };
			var button = new Button { Text = "Disable" };
			var button1 = new Button { Text = "Enable" };
			button1.Clicked+= (sender, e) => {
				_dummyResult = true;
				PunchSubmitCommand.ChangeCanExecute();
			};
			var tbItem = new ToolbarItem { Icon = "menuIcon.png" };
			var tbItem2 = new ToolbarItem { Icon = "menuIcon.png", Text="submit" };
			button.SetBinding (Button.CommandProperty, new Binding ("PunchSubmitCommand"));
			tbItem.SetBinding (MenuItem.CommandProperty, new Binding ("PunchSubmitCommand"));
			tbItem2.SetBinding (MenuItem.CommandProperty, new Binding ("PunchSubmitCommand"));
			button.BindingContext = tbItem.BindingContext = tbItem2.BindingContext = this;
			ToolbarItems.Add(tbItem);
			var toolbar = new Toolbar { BackgroundColor = Color.Red };
			toolbar.Add (tbItem2);
			stackPanel.Children.Add (toolbar);
			stackPanel.Children.Add (button);
			stackPanel.Children.Add (button1);
			Content = stackPanel;
		}

		bool _dummyResult = true;

		Command _punchSubmitCommand;
		public Command PunchSubmitCommand
		{
			get
			{
				return _punchSubmitCommand ?? (_punchSubmitCommand = new Command(() => {
					_dummyResult = !_dummyResult;
					PunchSubmitCommand.ChangeCanExecute();
				},
					() => _dummyResult));
			}
		}
	}
}

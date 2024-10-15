using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 41153, "jobject must not be IntPtr.Zero with TabbedPage and ToolbarItems")]
public class Bugzilla41153 : TestTabbedPage
{
	MyViewModel _Vm = new MyViewModel();
	const string Tab1 = "Tab 1";
	const string Tab1Content = "On Tab 1";
	const string Tab2 = "Tab 2";
	const string Tab3 = "Tab 3";
	const string Tab3Content = "On Tab 3";
	const string ToolbarItemText = "Toolbar Item";
	const string Success = "Success";


	class MyViewModel : INotifyPropertyChanged
	{
		string _toolBarItemText;
		public string ToolbarItemText
		{
			get
			{
				return _toolBarItemText;
			}
			set
			{
				_toolBarItemText = value;
				OnPropertyChanged();
			}
		}

		ICommand _toolBarItemCommand;
		public ICommand ToolbarItemCommand
		{
			get
			{
				if (_toolBarItemCommand == null)
				{
					_toolBarItemCommand = new Command(() =>
					{
						ToolbarItemText = Success;
					});
				}

				return _toolBarItemCommand;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	protected override void Init()
	{
		var page1 = new ContentPage { Content = new Label { Text = Tab1Content, AutomationId = Tab1Content }, BindingContext = _Vm };
		var toolBarItem = new ToolbarItem() { AutomationId = ToolbarItemText };
		toolBarItem.SetBinding(ToolbarItem.CommandProperty, nameof(MyViewModel.ToolbarItemCommand));
		toolBarItem.SetBinding(ToolbarItem.TextProperty, nameof(MyViewModel.ToolbarItemText));
		page1.ToolbarItems.Add(toolBarItem);
		var page2 = new ContentPage();
		var page3 = new ContentPage { Content = new Label { Text = Tab3Content, AutomationId = Tab3Content } };
		Children.Add(new NavigationPage(page1) { Title = Tab1, AutomationId = Tab1 });
		Children.Add(new NavigationPage(page2) { Title = Tab2, AutomationId = Tab2 });
		Children.Add(new NavigationPage(page3) { Title = Tab3, AutomationId = Tab3 });
		_Vm.ToolbarItemText = ToolbarItemText;
	}
}
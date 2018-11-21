using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
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

		[Preserve(AllMembers = true)]
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
			var page1 = new ContentPage { Content = new Label { Text = Tab1Content }, BindingContext = _Vm };
			var toolBarItem = new ToolbarItem();
			toolBarItem.SetBinding(ToolbarItem.CommandProperty, nameof(MyViewModel.ToolbarItemCommand));
			toolBarItem.SetBinding(ToolbarItem.TextProperty, nameof(MyViewModel.ToolbarItemText));
			page1.ToolbarItems.Add(toolBarItem);
			var page2 = new ContentPage();
			var page3 = new ContentPage { Content = new Label { Text = Tab3Content } };
			Children.Add(new NavigationPage(page1) { Title = Tab1 });
			Children.Add(new NavigationPage(page2) { Title = Tab2 });
			Children.Add(new NavigationPage(page3) { Title = Tab3 });
			_Vm.ToolbarItemText = ToolbarItemText;
		}

#if UITEST
		[Test]
		public void Bugzilla41153Test()
		{
			RunningApp.WaitForElement(q => q.Marked(Tab1Content));
			RunningApp.Tap(q => q.Marked(Tab2));
			RunningApp.Tap(q => q.Marked(Tab3));
			RunningApp.WaitForElement(q => q.Marked(Tab3Content));
			RunningApp.Tap(q => q.Marked(Tab1));
			RunningApp.WaitForElement(q => q.Marked(Tab1Content));
			RunningApp.Tap(q => q.Marked(ToolbarItemText));
			RunningApp.WaitForElement(q => q.Marked(Success));
		}
#endif
	}
}
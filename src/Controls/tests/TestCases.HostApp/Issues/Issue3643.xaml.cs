using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 3643, "Binding to Switch.IsEnabled has no effect", PlatformAffected.WinPhone)]
	public partial class Issue3643 : ContentPage
	{
		public Issue3643() {
			InitializeComponent();
			BindingContext = this;
		}

        ObservableCollection<SomeItem> _items;
        public ObservableCollection<SomeItem> Items {
            get {
                if (_items == null) {
                    _items = new ObservableCollection<SomeItem>(Enumerable.Range(0, 5).Select(c => {
                        return new SomeItem() { Name = string.Format("Item {0}", c) };
                    }));
                }

                return _items;
            }
        }

        void Button_Clicked(System.Object sender, System.EventArgs e) {
            foreach (SomeItem item in Items) {
                item.Name = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
            }
        }

		public class SomeItem : INotifyPropertyChanged
		{
			string name;
			public string Name {
				get => name;
				set {
					name = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;
		}
	}
}
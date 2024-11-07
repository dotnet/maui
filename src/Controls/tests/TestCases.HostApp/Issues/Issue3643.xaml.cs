using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 3643, "CollectionView items don't change their size when item content size is changed", PlatformAffected.iOS)]
	public partial class Issue3643 : ContentPage
	{
		public Issue3643() 
		{
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

		string testing = String.Empty;
		void Button_Clicked(System.Object sender, System.EventArgs e) {

			if (sender is not Button button) {
				return;
			}

			if (testing == button.Text) 
			{
				foreach (SomeItem item in Items) {
					item.Name = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.";
				}
				return;
			}
			else
			{
				int i = 0;
				foreach (SomeItem item in Items) {
					item.Name = string.Format("Item {0}", i++);
				}

			}

			testing = button.Text;

			if (testing == "Label")
			{
				cv.ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding("Name"));
					return new Grid() { label };
				});
			}

			else if (testing == "Button")
			{
				cv.ItemTemplate = new DataTemplate(() =>
				{
					var button = new Button(){ LineBreakMode = LineBreakMode.WordWrap };
					button.SetBinding(Button.TextProperty, new Binding("Name"));
					return new Grid() { button };
				});
			}

			else if (testing == "Editor")
			{
				cv.ItemTemplate = new DataTemplate(() =>
				{
					var editor = new Editor(){ AutoSize = EditorAutoSizeOption.TextChanges };
					editor.SetBinding(Editor.TextProperty, new Binding("Name"));
					return new Grid() { editor };
				});
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
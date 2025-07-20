using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 31333,
	"Focus() on Entry in ViewCell brings up keyboard, but doesn't have cursor in EditText", PlatformAffected.Android)]
public class Bugzilla31333 : TestContentPage
{

	public class Model31333 : INotifyPropertyChanged
	{
		public string Data
		{
			get { return _data; }
			set
			{
				_data = value;
				OnPropertyChanged();
			}
		}

		bool _isFocused;
		string _data;

		public bool IsFocused
		{
			get { return _isFocused; }
			set
			{
				_isFocused = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}


	public interface IHaveControlFocusedProperty
	{
		void SetBinding();
	}


	public class ExtendedEntry : Entry, IHaveControlFocusedProperty
	{
		public static readonly BindableProperty IsControlFocusedProperty =
			BindableProperty.Create("IsControlFocused", typeof(bool), typeof(ExtendedEntry), false);

		public bool IsControlFocused
		{
			get { return (bool)GetValue(IsControlFocusedProperty); }
			set { SetValue(IsControlFocusedProperty, value); }
		}

		protected override void OnPropertyChanged(string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			if (propertyName == IsControlFocusedProperty.PropertyName)
			{
				if (IsControlFocused)
				{
					Focus();
				}
				else
				{
					Unfocus();
				}
			}
		}

		public void SetBinding()
		{
			this.SetBinding(IsControlFocusedProperty, nameof(IsFocused));
		}
	}


	public class ExtendedEditor : Editor, IHaveControlFocusedProperty
	{
		public static readonly BindableProperty IsControlFocusedProperty =
			BindableProperty.Create("IsControlFocused", typeof(bool), typeof(ExtendedEditor), false);

		public bool IsControlFocused
		{
			get { return (bool)GetValue(IsControlFocusedProperty); }
			set { SetValue(IsControlFocusedProperty, value); }
		}

		protected override void OnPropertyChanged(string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			if (propertyName == IsControlFocusedProperty.PropertyName)
			{
				if (IsControlFocused)
				{
					Focus();
				}
				else
				{
					Unfocus();
				}
			}
		}

		public void SetBinding()
		{
			this.SetBinding(IsControlFocusedProperty, nameof(IsFocused));
		}
	}


	public class ExtendedCell<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T> : ViewCell where T : View, IHaveControlFocusedProperty
	{
		public ExtendedCell(string automationId = null)
		{
			var control = (T)Activator.CreateInstance(typeof(T));
			control.SetBinding();
#pragma warning disable CS0618 // Type or member is obsolete
			control.HorizontalOptions = LayoutOptions.FillAndExpand;
#pragma warning restore CS0618 // Type or member is obsolete
			if (!string.IsNullOrEmpty(automationId))
			{
				control.AutomationId = automationId;
			}
			View = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Fill,
				Children = {
					control
				}
			};
		}
	}

	StackLayout CreateListViewTestSection(Type controlType, string automationId)
	{
		var name = controlType.GenericTypeArguments[0].Name;
		name = name.Replace("Extended", "", StringComparison.InvariantCultureIgnoreCase);

		var button = new Button() { Text = $"Focus {name} in ListView" };

		var data = new ObservableCollection<Model31333> { new Model31333() };

		var listView = new ListView
		{
			VerticalOptions = LayoutOptions.Start,
			ItemsSource = data,
			ItemTemplate = new DataTemplate(() => new ExtendedCell<ExtendedEntry>(automationId))
		};

		button.Clicked += (sender, args) =>
		{
			var item = data[0];
			if (item != null)
			{
				item.IsFocused = !item.IsFocused;
			}
		};

		return new StackLayout() { Children = { button, listView } };
	}

	StackLayout CreateTableViewTestSection<T>(string automationId) where T : View, IHaveControlFocusedProperty
	{
		var name = typeof(T).Name;
		name = name.Replace("Extended", "", StringComparison.InvariantCultureIgnoreCase);

		var button = new Button() { Text = $"Focus {name} in Table" };

		var data = new Model31333();

		var tableView = new TableView
		{
			VerticalOptions = LayoutOptions.Start
		};

		var tableRoot = new TableRoot();
		var tableSection = new TableSection();

		var cell = new ExtendedCell<T>(automationId);

		cell.BindingContext = data;

		tableSection.Add(cell);
		tableRoot.Add(tableSection);
		tableView.Root = tableRoot;

		button.Clicked += (sender, args) =>
		{
			var item = data;
			item.IsFocused = !item.IsFocused;
		};

		return new StackLayout() { Children = { button, tableView } };
	}

	protected override void Init()
	{
		var entrySection = CreateListViewTestSection(typeof(ExtendedCell<ExtendedEntry>), "EntryListView");
		var editorSection = CreateListViewTestSection(typeof(ExtendedCell<ExtendedEditor>), "EditorListView");

		var entryTableSection = CreateTableViewTestSection<ExtendedEntry>("EntryTable");
		var editorTableSection = CreateTableViewTestSection<ExtendedEditor>("EditorTable");

		Content = new StackLayout() { Children = { entrySection, editorSection, entryTableSection, editorTableSection } };
	}
}
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 55745, "[iOS] NRE in ListView with HasUnevenRows=true after changing content and rebinding", PlatformAffected.iOS)]
	public class Bugzilla55745 : TestContentPage
	{
		const string ButtonId = "button";
		ViewModel vm;

		protected override void Init()
		{
			vm = new ViewModel();
			BindingContext = vm;

			var listView = new ListView
			{
				HasUnevenRows = true,
				ItemTemplate = new DataTemplate(() =>
				{
					var label1 = new Label();
					label1.SetBinding(Label.TextProperty, nameof(DataViewModel.TextOne));
					var label2 = new Label();
					label2.SetBinding(Label.TextProperty, nameof(DataViewModel.TextTwo));
					return new ViewCell { View = new StackLayout { Children = { label1, label2 } } };
				})
			};

			listView.SetBinding(ListView.ItemsSourceProperty, nameof(vm.MyCollection));

			var button = new Button { Text = "Tap me twice. The app should not crash.", AutomationId = ButtonId };
			button.Clicked += Button_Clicked;

			Content = new StackLayout { Children = { button, listView } };
		}

		void Button_Clicked(object sender, System.EventArgs e)
		{
			vm.ToggleContent();
		}

		[Preserve(AllMembers = true)]
		class DataViewModel : INotifyPropertyChanged
		{
			string mTextOne;

			string mTextTwo;

			public event PropertyChangedEventHandler PropertyChanged;

			public string TextOne
			{
				get { return mTextOne; }
				set
				{
					mTextOne = value;
					OnPropertyChanged(nameof(TextOne));
				}
			}

			public string TextTwo
			{
				get { return mTextTwo; }
				set
				{
					mTextTwo = value;
					OnPropertyChanged(nameof(TextTwo));
				}
			}

			protected virtual void OnPropertyChanged(string propertyName)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		[Preserve(AllMembers = true)]
		class ViewModel : INotifyPropertyChanged
		{
			public List<DataViewModel> myList = new List<DataViewModel>()
			{
				new DataViewModel() { TextOne = "Super", TextTwo = "Juuu"},
				new DataViewModel() { TextOne = "Michael", TextTwo = "Maier"},
				new DataViewModel() { TextOne = "House", TextTwo = "Cat"},
				new DataViewModel() { TextOne = "Flower", TextTwo = "Rock"},
				new DataViewModel() { TextOne = "Job", TextTwo = "Dog"},
				new DataViewModel() { TextOne = "Super", TextTwo = "Juuu"},
				new DataViewModel() { TextOne = "Michael", TextTwo = "Maier"},
				new DataViewModel() { TextOne = "House", TextTwo = "Cat"},
				new DataViewModel() { TextOne = "Flower", TextTwo = "Rock"},
				new DataViewModel() { TextOne = "Job", TextTwo = "Dog"}
			};

			ObservableCollection<DataViewModel> mMyCollection;

			DataViewModel mSelectedData;

			public ViewModel()
			{
				MyCollection = new ObservableCollection<DataViewModel>(myList);
			}

			public event PropertyChangedEventHandler PropertyChanged;

			public ObservableCollection<DataViewModel> MyCollection
			{
				get
				{
					return mMyCollection;
				}
				set
				{
					mMyCollection = value;
					OnPropertyChanged(nameof(MyCollection));
				}
			}

			public DataViewModel SelectedData
			{
				get { return mSelectedData; }
				set
				{
					mSelectedData = value;
					OnPropertyChanged(nameof(SelectedData));
				}
			}

			public void ToggleContent()
			{
				if (MyCollection.Count < 3)
				{
					MyCollection.Clear();
					MyCollection = new ObservableCollection<DataViewModel>(myList);
				}
				else
				{
					MyCollection.Clear();
					MyCollection.Add(myList[2]);
				}
			}

			protected virtual void OnPropertyChanged(string propertyName)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}

#if UITEST
		[Test]
		public void Bugzilla55745Test()
		{
			RunningApp.WaitForElement(q => q.Marked(ButtonId));
			RunningApp.Tap(q => q.Marked(ButtonId));
			RunningApp.Tap(q => q.Marked(ButtonId));
		}
#endif
	}
}
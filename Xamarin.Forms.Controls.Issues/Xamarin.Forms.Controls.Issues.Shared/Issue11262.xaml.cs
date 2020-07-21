using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.SwipeView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11262, "[Bug] SwipeView with StackLayout Bindable delete item exception Xamarin.Forms.Platform.Android.SwipeViewRenderer", PlatformAffected.Android)]
	public partial class Issue11262 : TestContentPage
	{
		public Issue11262()
		{
#if APP
			Device.SetFlags(new List<string> { ExperimentalFlags.SwipeViewExperimental });
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			BindingContext = new Issue11262ViewModel();
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue11262Model : BindableObject
	{
		public string Title { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue11262ViewModel : BindableObject
	{
		ObservableCollection<Issue11262Model> _items;

		public Issue11262ViewModel()
		{
			LoadItems();
		}

		public ObservableCollection<Issue11262Model> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		public ICommand RemoveCommand => new Command<Issue11262Model>(ExecuteRemove);

		void LoadItems()
		{
			Items = new ObservableCollection<Issue11262Model>
			{
				new Issue11262Model { Title = "Item 1" },
				new Issue11262Model { Title = "Item 2" },
				new Issue11262Model { Title = "Item 3" }
			};
		}

		void ExecuteRemove(Issue11262Model parameter)
		{
			Items.Remove(parameter);
		}
	}
}
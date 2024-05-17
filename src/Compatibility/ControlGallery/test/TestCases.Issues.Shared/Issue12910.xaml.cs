using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12910,
		"[Bug] 'Cannot access a disposed object. Object name: 'DefaultRenderer' - on ios with CollectionView and EmptyView",
		PlatformAffected.iOS)]
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Issue12910 : TestContentPage
	{
		readonly Issue12910ViewModel _viewModel;

		public Issue12910()
		{
			_viewModel = new Issue12910ViewModel();
#if APP
			InitializeComponent();
			BindingContext = _viewModel;
#endif
		}

		protected override void Init()
		{

		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			_viewModel.OnAppearing();
		}
	}

	public class Issue12910Model
	{
		public string Id { get; set; }
		public string Text { get; set; }
		public string Description { get; set; }
	}

	public class Issue12910ViewModel : BindableObject
	{
		bool _isBusy;
		Issue12910Model _selectedItem;

		public ObservableCollection<Issue12910Model> Items { get; }
		public Command LoadItemsCommand { get; }
		public Command AddItemCommand { get; }
		public Command<Issue12910Model> ItemTapped { get; }

		public Issue12910ViewModel()
		{
			Items = new ObservableCollection<Issue12910Model>();
			LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
			ItemTapped = new Command<Issue12910Model>(OnItemSelected);
			AddItemCommand = new Command(OnAddItem);
		}

		async Task ExecuteLoadItemsCommand()
		{
			IsBusy = true;

			try
			{
				Items.Clear();

				await Task.Delay(500);

				Items.Add(new Issue12910Model { Id = "1", Text = "Text 1", Description = "Description 1" });
				Items.Add(new Issue12910Model { Id = "2", Text = "Text 2", Description = "Description 2" });
				Items.Add(new Issue12910Model { Id = "3", Text = "Text 3", Description = "Description 3" });
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
			finally
			{
				IsBusy = false;
			}
		}

		public void OnAppearing()
		{
			IsBusy = true;
			SelectedItem = null;
		}

		public bool IsBusy
		{
			get => _isBusy;
			set
			{
				_isBusy = value;
				OnPropertyChanged();
			}
		}

		public Issue12910Model SelectedItem
		{
			get => _selectedItem;
			set
			{
				_selectedItem = value;
				OnPropertyChanged();
			}
		}

		void OnAddItem(object obj)
		{

		}

		void OnItemSelected(Issue12910Model item)
		{

		}
	}
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
	[Category(UITestCategories.CarouselView)]
	[Category(UITestCategories.UwpIgnore)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9827, "CarouselView doesn't update the CurrentItem on Swipe under strange condition", PlatformAffected.Android)]
	public partial class Issue9827 : TestContentPage
	{
		ViewModelIssue9827 ViewModel => BindingContext as ViewModelIssue9827;
		public Issue9827()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			BindingContext = new ViewModelIssue9827();
		}

		protected override void OnAppearing()
		{
			if (ViewModel.Items.Count == 0)
				ViewModel.LoadItemsCommand.Execute(null);
			base.OnAppearing();
		}

#if UITEST
		[Test]
		public void Issue9827Test()
		{
			RunningApp.WaitForElement("Pos:0");
			RunningApp.Tap(c => c.Marked("btnNext"));
			RunningApp.WaitForElement("Item 1 with some additional text");
			RunningApp.WaitForElement("Pos:1");
		
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class ViewModelIssue9827 : System.ComponentModel.INotifyPropertyChanged
	{
		public ViewModelIssue9827()
		{
			Items = new ObservableCollection<ModelIssue9827>();
			LoadItemsCommand = new Command(ExecuteLoadItemsCommand);
			GoNextCommand = new Command(GoNext);
			GoPrevCommand = new Command(GoPrev);
		}

		ModelIssue9827 carouselCurrentItem;

		void GoNext()
		{
			var index = Items.IndexOf(carouselCurrentItem);
			var newItem = Items[Math.Min(index + 1, Items.Count - 1)];
			CarouselCurrentItem = newItem;
		}
		void GoPrev()
		{
			var index = Items.IndexOf(carouselCurrentItem);
			var newItem = Items[Math.Max(0, index - 1)];
			CarouselCurrentItem = newItem;
		}
		void ExecuteLoadItemsCommand()
		{
			if (IsBusy)
				return;

			IsBusy = true;

			try
			{
				Items.Clear();

				var items = Enumerable.Range(0, 10).Select(i => new ModelIssue9827() { Title = $"Item {i} with some additional text" });

				foreach (var item in items)
				{
					Items.Add(item);
				}
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

		public ModelIssue9827 CarouselCurrentItem
		{
			get { return carouselCurrentItem; }
			set { SetProperty(ref carouselCurrentItem, value); }
		}

		public ObservableCollection<ModelIssue9827> Items { get; set; }

		public Command LoadItemsCommand { get; set; }

		public Command GoNextCommand { get; set; }
		public Command GoPrevCommand { get; set; }

		bool isBusy = false;

		string title = string.Empty;

		protected bool SetProperty<T>(ref T backingStore,
									  T value,
									  [CallerMemberName] string propertyName = "",
									  Action onChanged = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return false;

			backingStore = value;
			onChanged?.Invoke();
			OnPropertyChanged(propertyName);
			return true;
		}

		public bool IsBusy { get { return isBusy; } set { SetProperty(ref isBusy, value); } }

		public string Title { get { return title; } set { SetProperty(ref title, value); } }

		#region INotifyPropertyChanged
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			var changed = PropertyChanged;
			if (changed == null)
				return;

			changed.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}

	[Preserve(AllMembers = true)]
	public class ModelIssue9827 : System.ComponentModel.INotifyPropertyChanged
	{
		string _title;

		protected bool SetProperty<T>(ref T backingStore,
									  T value,
									  [CallerMemberName] string propertyName = "",
									  Action onChanged = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return false;

			backingStore = value;
			onChanged?.Invoke();
			OnPropertyChanged(propertyName);
			return true;
		}

		public Guid Id { get; set; }

		public string Title
		{
			get { return _title; }
			set
			{
				if (_title == value)
				{
					return;
				}

				_title = value;
				OnPropertyChanged();
			}
		}

		#region INotifyPropertyChanged

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			var changed = PropertyChanged;
			if (changed == null)
				return;

			changed.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
		}
		#endregion INotifyPropertyChanged
	}
}

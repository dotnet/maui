using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	// Issue9827 (src\ControlGallery\src\Issues.Shared\Issue9827.cs
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 9827, "CarouselView doesn't update the CurrentItem on Swipe under strange condition", PlatformAffected.Android)]
	public partial class CarouselViewUpdateCurrentItem : ContentPage
	{
		public CarouselViewUpdateCurrentItem()
		{
			InitializeComponent();

			BindingContext = new ViewModelIssue9827();
		}
		
		ViewModelIssue9827 ViewModel => BindingContext as ViewModelIssue9827;

		protected override void OnAppearing()
		{
			if (ViewModel.Items.Count == 0)
				ViewModel.LoadItemsCommand.Execute(null);

			base.OnAppearing();
		}
	}

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
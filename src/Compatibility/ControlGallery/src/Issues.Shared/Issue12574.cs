using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.CarouselView)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12574, "CarouselView Loop=True default freezes iOS app", PlatformAffected.Default)]
	public class Issue12574 : TestContentPage
	{
		ViewModelIssue12574 viewModel;
		CarouselView _carouselView;
		Button _btn;
		Button _btn2;
		string carouselAutomationId = "carouselView";
		string btnRemoveAutomationId = "btnRemove";
		string btnRemoveAllAutomationId = "btnRemoveAll";

		protected override void Init()
		{
			_btn = new Button
			{
				Text = "Remove Last",
				AutomationId = btnRemoveAutomationId
			};
			_btn.SetBinding(Button.CommandProperty, "RemoveLastItemCommand");

			_btn2 = new Button
			{
				Text = "Remove All",
				AutomationId = btnRemoveAllAutomationId
			};
			_btn2.SetBinding(Button.CommandProperty, "RemoveAllItemsCommand");

			_carouselView = new CarouselView
			{
				AutomationId = carouselAutomationId,
				Margin = new Thickness(30),
				BackgroundColor = Colors.Yellow,
				ItemTemplate = new DataTemplate(() =>
				{

					var stacklayout = new StackLayout();
					var labelId = new Label();
					var labelText = new Label();
					var labelDescription = new Label();
					labelId.SetBinding(Label.TextProperty, "Id");
					labelText.SetBinding(Label.TextProperty, "Text");
					labelDescription.SetBinding(Label.TextProperty, "Description");

					stacklayout.Children.Add(labelId);
					stacklayout.Children.Add(labelText);
					stacklayout.Children.Add(labelDescription);
					return stacklayout;
				})
			};
			_carouselView.SetBinding(CarouselView.ItemsSourceProperty, "Items");
			this.SetBinding(Page.TitleProperty, "Title");

			var layout = new Grid();
			layout.RowDefinitions.Add(new RowDefinition { Height = 100 });
			layout.RowDefinitions.Add(new RowDefinition { Height = 100 });
			layout.RowDefinitions.Add(new RowDefinition());
			Grid.SetRow(_btn2, 1);
			Grid.SetRow(_carouselView, 2);
			layout.Children.Add(_btn);
			layout.Children.Add(_btn2);
			layout.Children.Add(_carouselView);

			BindingContext = viewModel = new ViewModelIssue12574();
			Content = layout;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			viewModel.OnAppearing();
		}

#if UITEST
		[Test]
		public void Issue12574Test()
		{
			RunningApp.WaitForElement("0 item");

			var rect = RunningApp.Query(c => c.Marked(carouselAutomationId)).First().Rect;
			var centerX = rect.CenterX;
			var rightX = rect.X - 5;
			RunningApp.DragCoordinates(centerX + 40, rect.CenterY, rightX, rect.CenterY);

			RunningApp.WaitForElement("1 item");

			RunningApp.DragCoordinates(centerX + 40, rect.CenterY, rightX, rect.CenterY);

			RunningApp.WaitForElement("2 item");

			RunningApp.Tap(btnRemoveAutomationId);
			
			RunningApp.WaitForElement("1 item");

			rightX = rect.X + rect.Width - 1;
			RunningApp.DragCoordinates(rect.X, rect.CenterY, rightX, rect.CenterY);

			RunningApp.WaitForElement("0 item");
		}

		[Test]
		public void RemoveItemsQuickly()
		{
			RunningApp.WaitForElement("0 item");
			RunningApp.Tap(btnRemoveAllAutomationId);

			// If we haven't crashed, then the other button should be here
			RunningApp.WaitForElement(btnRemoveAutomationId);
		}
#endif
	}

	class ViewModelIssue12574 : BaseViewModel1
	{
		public ObservableCollection<ModelIssue12574> Items { get; set; }
		public Command LoadItemsCommand { get; set; }
		public Command RemoveAllItemsCommand { get; set; }
		public Command RemoveLastItemCommand { get; set; }

		public ViewModelIssue12574()
		{
			Title = "CarouselView Looping";
			Items = new ObservableCollection<ModelIssue12574>();
			LoadItemsCommand = new Command(() => ExecuteLoadItemsCommand());
			RemoveAllItemsCommand = new Command(() => ExecuteRemoveItemsCommand(), () => Items.Count > 0);
			RemoveLastItemCommand = new Command(() => ExecuteRemoveLastItemCommand(), () => Items.Count > 0);
		}

		void ExecuteRemoveItemsCommand()
		{
			while (Items.Count > 0)
			{
				Items.Remove(Items.Last());
				Items.Remove(Items.Last());
				Items.Remove(Items.Last());
			}
			RemoveAllItemsCommand.ChangeCanExecute();
			RemoveLastItemCommand.ChangeCanExecute();
		}

		void ExecuteRemoveLastItemCommand()
		{
			Items.Remove(Items.Last());
			RemoveAllItemsCommand.ChangeCanExecute();
			RemoveLastItemCommand.ChangeCanExecute();
		}

		void ExecuteLoadItemsCommand()
		{
			IsBusy = true;

			try
			{
				Items.Clear();
				for (int i = 0; i < 3; i++)
				{
					Items.Add(new ModelIssue12574 { Id = Guid.NewGuid().ToString(), Text = $"{i} item", Description = "This is an item description." });
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
			finally
			{
				IsBusy = false;
				RemoveAllItemsCommand.ChangeCanExecute();
				RemoveLastItemCommand.ChangeCanExecute();
			}
		}

		public void OnAppearing()
		{
			IsBusy = true;
			LoadItemsCommand.Execute(null);
		}
	}

	class ModelIssue12574
	{
		public string Id { get; set; }
		public string Text { get; set; }
		public string Description { get; set; }
	}

	class BaseViewModel1 : INotifyPropertyChanged
	{
		public string Title { get; set; }
		public bool IsInitialized { get; set; }

		bool _isBusy;

		/// <summary>
		/// Gets or sets if VM is busy working
		/// </summary>
		public bool IsBusy
		{
			get { return _isBusy; }
			set { _isBusy = value; OnPropertyChanged("IsBusy"); }
		}

		//INotifyPropertyChanged Implementation
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged == null)
				return;

			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}

}
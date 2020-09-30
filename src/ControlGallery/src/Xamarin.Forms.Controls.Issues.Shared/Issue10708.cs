using System.Collections.ObjectModel;
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
	[Category(UITestCategories.CarouselView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10708, "[Bug] CarouselView - Setting BindingIndex to null - Exception IItemsViewSource is empty", PlatformAffected.iOS)]
	public class Issue10708 : TestNavigationPage
	{
		public Issue10708()
		{

		}

		protected override void Init()
		{
			PushAsync(new Issue10708FirstView());
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue10708FirstView : ContentPage
	{
		public Issue10708FirstView()
		{
			Title = "Issue 10708";

			var layout = new StackLayout();

			var navigateButton = new Button
			{
				Text = "Navigate"
			};

			layout.Children.Add(navigateButton);

			Content = layout;

			navigateButton.Clicked += (sender, args) =>
			{
				Navigation.PushAsync(new Issue10708SecondView());
			};
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue10708SecondView : ContentPage
	{
		readonly CarouselView _carouselView;

		public Issue10708SecondView()
		{
			Title = "Issue 10708";

			_carouselView = new CarouselView();

			_carouselView.SetBinding(ItemsView.ItemsSourceProperty, "Items");
			_carouselView.SetBinding(CarouselView.CurrentItemProperty, "SelectedItem");

			Content = _carouselView;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			_carouselView.BindingContext = new Issue10708ViewModel();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			_carouselView.BindingContext = null;
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue10708ViewModel : BindableObject
	{
		public Issue10708ViewModel()
		{
			LoadItems();
		}

		public ObservableCollection<string> Items { get; set; }

		public string SelectedItem { get; set; }

		void LoadItems()
		{
			Items = new ObservableCollection<string>
			{
				"Item 1",
				"Item 2",
				"Item 3"
			};

			SelectedItem = Items[1];
		}
	}
}
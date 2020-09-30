using System.Collections.Generic;
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
	[Issue(IssueTracker.Github, 10530, "[Bug] Swipe View Null Reference Exception while trying to change visibility of swipe item", PlatformAffected.Android)]
	public class Issue10530 : TestContentPage
	{
		public Issue10530()
		{
			Title = "Issue 10530";

			var vm = new Issue10530ViewModel();

			BindingContext = vm;

			var layout = new StackLayout();

			var swipeView = new SwipeView();

			var swipeViewContent = new Grid
			{
				BackgroundColor = Color.LightGray,
				HeightRequest = 60
			};

			var infoLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			infoLabel.SetBinding(Label.TextProperty, "Item.Text");

			swipeViewContent.Children.Add(infoLabel);
			swipeView.Content = swipeViewContent;

			var swipeItemView = new SwipeItemView();

			swipeItemView.SetBinding(IsVisibleProperty, "Item.RetryAvailable");

			var swipeItemContent = new Grid
			{
				BackgroundColor = Color.Orange,
				WidthRequest = 100
			};

			var swipeItemLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Retry",
				TextColor = Color.White
			};

			swipeItemContent.Children.Add(swipeItemLabel);

			swipeItemView.Content = swipeItemContent;

			swipeView.LeftItems = new SwipeItems { swipeItemView };

			layout.Children.Add(swipeView);

			var changeButton = new Button
			{
				Text = "Update Item RetryAvailable"
			};

			layout.Children.Add(changeButton);

			Content = layout;

			swipeItemView.Invoked += (sender, args) =>
			{
				vm.MakeRetryInvisibleCommand.Execute(vm.Item);
			};

			changeButton.Clicked += (sender, args) =>
			{
				var item = vm.Item;

				if (item.RetryAvailable)
					vm.MakeRetryInvisibleCommand.Execute(vm.Item);
				else
					vm.MakeRetryVisibleCommand.Execute(vm.Item);
			};

			vm.MakeRetryVisibleCommand.Execute(vm.Item);
		}

		protected override void Init()
		{
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue10530Item : BindableObject
	{
		bool _retryAvailable;

		public string Text { get; set; }

		public bool RetryAvailable
		{
			get => _retryAvailable;
			set
			{
				_retryAvailable = value;
				OnPropertyChanged();
			}
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue10530ViewModel : BindableObject
	{
		public Issue10530ViewModel()
		{
			LoadItem();

			MakeRetryInvisibleCommand = new Command<Issue10530Item>((item) => MakeRetryInvisible(item));
			MakeRetryVisibleCommand = new Command<Issue10530Item>((item) => MakeRetryVisible(item));
		}

		public Issue10530Item Item { get; set; }

		public Command<Issue10530Item> MakeRetryInvisibleCommand { get; set; }
		public Command<Issue10530Item> MakeRetryVisibleCommand { get; set; }

		void LoadItem()
		{
			Item = new Issue10530Item { Text = "Item 1" };
		}

		void MakeRetryVisible(Issue10530Item item)
		{
			item.RetryAvailable = true;
		}

		void MakeRetryInvisible(Issue10530Item item)
		{
			item.RetryAvailable = false;
		}
	}
}
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3652, "Loses the correct reference to the cell after adding and removing items to a ListView", PlatformAffected.UWP)]
	public class Issue3652 : TestContentPage
	{
		MainPageViewModel model = new MainPageViewModel();

		protected override void Init()
		{
			BindingContext = model;

			Content = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = "Remove the items using the context menu. Then add 3 more items and try to delete them as well. " +
						"If all items are deleted successfully, then the test is passed.",
						BackgroundColor = Color.Aqua
					},
					new Button {
						Command = model.AddListItemCommand,
						Text = "Add an item"
					},
					new StackLayout
					{
						Children = {
							new ListView
							{
								ItemsSource = model.ItemCollection,
								ItemTemplate = new DataTemplate (typeof(ICell))
							}
						}
					}
				}
			};
		}

		[Preserve(AllMembers = true)]
		public class ICell : ViewCell
		{
			public ICell()
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, "Description");

				var menu = new MenuItem { Text = "Remove" };
				menu.Command = new Command(() => ((ListItemViewModel)BindingContext).Remove.Execute((this, BindingContext)));
				ContextActions.Add(menu);
				var stack = new StackLayout
				{
					Children =
					{
						label
					}
				};
				View = stack;
			}
		}

		[Preserve(AllMembers = true)]
		public class MainPageViewModel
		{
			int newItemNumber = 1;

			public MainPageViewModel()
			{
				MessagingCenter.Subscribe<ListItemViewModel, ListItemViewModel>(this, "Remove", (sender, arg) => RemoveAnItem(arg));
				AddListItemCommand = new Command(AddListItem);
				for (int i = 0; i < 3; i++)
					AddListItem();
			}

			public ObservableCollection<ListItemViewModel> ItemCollection { get; set; } = new ObservableCollection<ListItemViewModel>();

			public Command AddListItemCommand { get; set; }

			void RemoveAnItem(ListItemViewModel item) => ItemCollection.Remove(item);

			void AddListItem()
			{
				ItemCollection.Add(new ListItemViewModel()
				{
					Number = newItemNumber++
				});
			}
		}

		[Preserve(AllMembers = true)]
		public class ListItemViewModel
		{
			public int Number { get; set; }

			public string Description => $"Remove me using the context menu. #{Number}";

			public Command Remove =>
				new Command(() => MessagingCenter.Send(this, "Remove", this));
		}
	}
}
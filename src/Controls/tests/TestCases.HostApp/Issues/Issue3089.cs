using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 3089, "TextCell text doesn't change when using Recycling on ListViews")]
	public class Issue3089 : NavigationPage
	{
		public Issue3089() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			const string Reload = "reload";
			const string Success = "success";

			public MainPage()
			{
				var oc = new ObservableCollection<string>(new[] { $"Click {Reload}", "and this text should go away" });

				Enumerable.Range(0, 100).ToList().ForEach(x => oc.Add(x.ToString()));

				Navigation.PushAsync(new MainPageCode
				{
					BindingContext = new MainViewModel
					{
						ViewModel1 = new ListViewModel
						{
							Items = oc
						}
					}
				});
			}


			public class MainPageCode : TabbedPage
			{
				public MainPageCode()
				{
					ToolbarItems.Add(new Microsoft.Maui.Controls.ToolbarItem() { Text = "add1" });

					ToolbarItems.Add(new Microsoft.Maui.Controls.ToolbarItem() { AutomationId = "reload", Text = "reload" });

					ToolbarItems[0].SetBinding(ToolbarItem.CommandProperty, "Add1Command");
					ToolbarItems[1].SetBinding(ToolbarItem.CommandProperty, "Add2Command");

					ListPageCode page = new ListPageCode();
					page.SetBinding(ListPageCode.BindingContextProperty, "ViewModel1");
					Children.Add(page);
				}
			}


			public class MainViewModel
			{

				void AddItems(ObservableCollection<string> list)
				{
					list.Add("new item");
					list.Add(Success);
				}

				public MainViewModel()
				{
					Add1Command = new Command(() => AddItems(ViewModel1.Items));
					Add2Command = new Command(() =>
					{
						ViewModel1.ReloadData();
						AddItems(ViewModel1.Items);
					});
				}

				public ListViewModel ViewModel1 { get; set; }

				public ICommand Add1Command { get; }
				public ICommand Add2Command { get; }
			}


			public class ListViewModel : INotifyPropertyChanged
			{
				public ObservableCollection<string> Items { get; set; }
				public bool IsVisible { get; set; } = true;

				public event PropertyChangedEventHandler PropertyChanged;

				public void ReloadData()
				{
					Items = new ObservableCollection<string>();
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Items)));
				}
			}


			public class ListPageCode : ContentPage
			{
				public ListPageCode()
				{
					IconImageSource = "coffee.png";
					ListView view = new ListView(ListViewCachingStrategy.RecycleElement);
					Content = view;

					view.SetBinding(ListView.ItemsSourceProperty, "Items");
				}
			}
		}
	}
}

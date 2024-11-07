using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25518, "[Windows] Fix for the screen does not display when changing the CurrentPage of a TabbedPage", PlatformAffected.UWP)]
	public partial class Issue25518 : ContentPage
	{
		public Issue25518()
		{
			Children = new ObservableCollection<Page>();
			Children.CollectionChanged += Children_CollectionChanged;

			var page = new TabMultiPage25518();
			Children.Add(page);

		}

		private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (var item in e.NewItems)
				{
					var page = item as Page;
					page.Parent = this;
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (var item in e.OldItems)
				{
					var page = item as Page;
					page.Parent = null;
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				foreach (var item in e.OldItems)
				{
					var page = item as Page;
					page.Parent = null;
				}
			}
		}

		public ObservableCollection<Page> Children { get; }
	}

	public class TabMultiPage25518 : TabbedPage
	{
		public TabMultiPage25518()
		{
			var page = new FirstPage25518() { Title = "One" };
			Children.Add(page);
			CurrentPage = Children[0];
		}

		public void SwitchTabMultiPageAsync()
		{
			var page = new SecondPage25518() { Title = "Two" };
			Children.Add(page);
			CurrentPage = Children[1];
		}
	}

	public class FirstPage25518 : ContentPage
	{
		public FirstPage25518()
		{
			var button = new Button 
			{ 
				Text = "Move to second page", 
				AutomationId="MoveToSecondPage" 
			};

			button.Clicked += (sender, e) =>
			{
				var page = Application.Current.MainPage;
				if (page is Issue25518 multiLayerPage)
				{
					var tabMultiPage = multiLayerPage.Children.FirstOrDefault() as TabMultiPage25518;
					tabMultiPage?.SwitchTabMultiPageAsync();
				}
			};

			Content = button;
		}
	}

	public class SecondPage25518 : ContentPage
	{
		public SecondPage25518()
		{
			Content = new Label 
			{ 
				Text = "Welcome to Second Page",
				VerticalOptions = LayoutOptions.Center, 
				HorizontalOptions = LayoutOptions.Center 
			};
		}
	}

#if WINDOWS

	public partial class MultiLayerPageHandler25518 : ViewHandler<Issue25518, ContentPanel>
	{

		internal class ContentPanel25518 : ContentPanel
		{

		}

		public static PropertyMapper<Issue25518, MultiLayerPageHandler25518> PropertyMapper = new PropertyMapper<Issue25518, MultiLayerPageHandler25518>(ViewHandler.ViewMapper)
		{
		};

		public MultiLayerPageHandler25518() : base(PropertyMapper)
		{
		}

		protected override ContentPanel CreatePlatformView()
		{
			var view = new ContentPanel25518()
			{
				CrossPlatformLayout = VirtualView,
			};

			return view;
		}

		protected override void ConnectHandler(ContentPanel platformView)
		{
			base.ConnectHandler(platformView);
			platformView.Loaded += OnLoaded;

			foreach (var child in VirtualView.Children)
			{
				var element = child.ToPlatform(MauiContext);
				platformView.Children.Add(element);
			}
		}

		protected override void DisconnectHandler(ContentPanel platformView)
		{
			platformView.Loaded -= OnLoaded;
			base.DisconnectHandler(platformView);
		}

		private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			if (this is IPlatformViewHandler handler)
			{
				if (handler.VirtualView is Issue25518 multiLayerPage)
				{
					foreach (var child in multiLayerPage.Children)
					{
						child.SendAppearing();
					}
				}
			}
		}
	}

#endif

	public static class Issue25518Extensions
	{
		public static MauiAppBuilder Issue25518ConfigureHandlers(this MauiAppBuilder builder)
		{
			builder.ConfigureMauiHandlers(handlers =>
			{
#if WINDOWS
				handlers.AddHandler(typeof(Issue25518), typeof(MultiLayerPageHandler25518));
#endif
			});

			return builder;
		}
	}
}

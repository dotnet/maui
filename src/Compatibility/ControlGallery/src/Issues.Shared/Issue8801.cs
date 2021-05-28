using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8801, "[Android] Attempt to read from field 'int android.view.ViewGroup$LayoutParams.width' on a null object reference",
		PlatformAffected.Android, navigationBehavior: NavigationBehavior.SetApplicationRoot)]
	public class Issue8801 : TestContentPage
	{
		View[] _elements = null;
		protected override void Init()
		{
			_elements = new View[]
			{
				new Button() { Text = "Edit", HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start },
				new Button() { Text = "Save" , HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start},
				new Button() { Text = "Cancel", HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start },
				new Label() { Text = "Some Label", HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start },
				new Label() { Text = "Success", HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start },
			};

			var layout = new PopupStackLayout()
			{
			};

			foreach (View element in _elements)
				layout.Children.Add(element);

			var grid = new Grid()
			{
				Children = {
					layout
				}
			};

			grid.AddChild(new Label() { Text = "Success" }, 0, 0);
			grid.AddChild(layout, 0, 1);

			Content = grid;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Device.BeginInvokeOnMainThread(() =>
			{
				foreach (var button in _elements.OfType<Button>())
				{
					button.Text += "changed";
				}

				foreach (var label in _elements.OfType<Label>())
				{
					if (label.Text != "Success")
						label.Text += "changed";
				}
			});
		}

		public interface IViewPositionService
		{
			Point GetRelativePosition(VisualElement subView, VisualElement parent);
		}

		public class PopupStackLayout : StackLayout, INotifyPropertyChanged
		{
			private static readonly Guid PageRootGridId = Guid.NewGuid();
			private readonly Guid ShowButtonId = Guid.NewGuid();
			private readonly IViewPositionService viewPositionService;
			private readonly StackLayout popupStack;
			private ContentPage rootPage;
			private Button showButton;

			public PopupStackLayout()
			{
				this.viewPositionService = DependencyService.Get<IViewPositionService>();
				this.BackgroundColor = Colors.Red;

				showButton = new Button()
				{
					Text = "Show",
					AutomationId = ShowButtonId.ToString(),
					Command = new Command(ImageButtonCommandAsync)
				};

				popupStack = new StackLayout()
				{
					BindingContext = this.BindingContext,
					AutomationId = Guid.NewGuid().ToString(),
					BackgroundColor = Colors.Blue,
					IsVisible = false,
					Margin = this.Margin,
					HorizontalOptions = LayoutOptions.Start,
					VerticalOptions = LayoutOptions.Start,
					Orientation = this.Orientation,
					WidthRequest = Device.RuntimePlatform == Device.UWP ? 20 : 50,
				};

				this.SetBinding(HeightRequestProperty, new Binding(nameof(Height), BindingMode.OneWay, source: showButton));
				this.SetBinding(WidthRequestProperty, new Binding(nameof(Width), BindingMode.OneWay, source: showButton));

				this.Children.Insert(0, showButton);
			}


			private bool isOpen = false;
			public bool IsOpen
			{
				get
				{
					return this.isOpen;
				}

				set
				{
					if (value != this.isOpen)
					{
						this.isOpen = value;
						NotifyPropertyChanged();
					}
				}
			}

			public async Task CloseAsync()
			{
				if (popupStack.IsVisible)
				{
					await popupStack.FadeTo(0, 100, Easing.Linear);
					popupStack.IsVisible = false;
					IsOpen = false;
				}
			}

			protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
			{
				var sizeRequest = base.OnMeasure(widthConstraint, heightConstraint);

				PositionStack();

				return sizeRequest;

			}

			protected override void OnChildAdded(Element child)
			{
				if (child != showButton && child is Button button)
				{
					if (Device.RuntimePlatform == Device.Android)
					{
						button.Clicked -= PopupStackLayout_Clicked;
						button.Clicked += PopupStackLayout_Clicked;
					}

					popupStack.Children.Add((View)child);
				}
				else
				{
					base.OnChildAdded(child);
				}
			}

			private async void PopupStackLayout_Clicked(object sender, EventArgs e)
			{
				await popupStack.FadeTo(0, 50, Easing.Linear);
				popupStack.IsVisible = false;

				popupStack.IsVisible = true;
				await popupStack.FadeTo(1, 50, Easing.Linear);
			}



			protected override void LayoutChildren(double x, double y, double width, double height)
			{
				if (rootPage == null)
				{
					var page = Navigation.NavigationStack.LastOrDefault();
					rootPage = GetRootPage(page);
				}

				if (rootPage != null)

				{
					if (rootPage.Content.AutomationId != PageRootGridId.ToString())
					{
						var rootGrid = new Grid() { AutomationId = PageRootGridId.ToString() };
						var content = rootPage.Content;
						rootPage.Content = null;
						rootGrid.Children.Add(content);
						content.Parent = rootGrid;

						rootGrid.Children.Add(popupStack);
						popupStack.Parent = rootGrid;
						rootPage.Content = rootGrid;
						rootGrid.Parent = rootPage;

						rootGrid.RaiseChild(popupStack);
					}
					else
					{
						var rootGrid = rootPage.Content as Grid;
						popupStack.Layout(new Rectangle(x, y, popupStack.WidthRequest, height));
						rootGrid.Children.Add(popupStack);
						popupStack.Parent = rootGrid;
						rootGrid.RaiseChild(popupStack);
					}
				}

				base.LayoutChildren(x, y, width, height);
			}

			private async void ImageButtonCommandAsync()
			{
				if (popupStack.IsVisible)
				{
					showButton.Text = "Show";
					await CloseAsync();
				}
				else
				{
					//  ((Grid)popupStack.Parent).Children[0].IsVisible = false;
					showButton.Text = "Hide";
					PositionStack();
					popupStack.Opacity = 0;
					popupStack.IsVisible = true;
					IsOpen = true;
					await popupStack.FadeTo(1, 100, Easing.Linear);
				}
			}

			private void PositionStack()
			{
				if (rootPage == null)
				{
					var page = Navigation.NavigationStack.LastOrDefault();
					if (page == null)
					{
						page = Application.Current.MainPage;
					}

					rootPage = GetRootPage(page);
				}

				if (rootPage != null)
				{
					var newPos = viewPositionService.GetRelativePosition(this, rootPage);
					popupStack.TranslateTo(newPos.X - popupStack.WidthRequest, newPos.Y, 1, null);
				}
			}

			private ContentPage GetRootPage(Page page)
			{
				if (page is FlyoutPage mdPage)
				{
					return GetRootPage(mdPage.Detail);
				}
				else if (page is NavigationPage navPage)
				{
					return GetRootPage(navPage.CurrentPage);
				}
				else if (page is TabbedPage tabPage)
				{
					return GetRootPage(tabPage.CurrentPage);
				}
				else if (page is CarouselPage carouselPage)
				{
					return GetRootPage(carouselPage.CurrentPage);
				}
				if (page is ContentPage cPage)
				{
					return cPage;
				}
				return null;
			}

			public new event PropertyChangedEventHandler PropertyChanged;

			private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
			{
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
				}
			}
		}


#if UITEST && __ANDROID__
		[Test]
		public void NotAddingElementsNativelyDoesntCrashAndroid()
		{
			RunningApp.WaitForElement("Success");
		}
#endif
	}
}

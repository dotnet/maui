using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	/// <summary>
	/// NavigationPage Feature Matrix Test - Based on Microsoft Documentation
	/// https://learn.microsoft.com/en-us/dotnet/maui/user-interface/pages/navigationpage?view=net-maui-9.0
	/// 
	/// Tests all NavigationPage properties and methods:
	/// - Direct Properties: BarBackgroundColor, BarBackground, BarTextColor
	/// - Attached Properties: HasNavigationBar, HasBackButton, BackButtonTitle, IconColor, TitleIconImageSource, TitleView
	/// - Navigation Methods: PushAsync, PopAsync, PopToRootAsync, PushModalAsync, PopModalAsync
	/// - Stack Properties: CurrentPage, RootPage, NavigationStack, ModalStack
	/// </summary>
	public class NavigationPageControlPage : NavigationPage
	{
		private NavigationPageViewModel _viewModel;

		public NavigationPageControlPage()
		{
			_viewModel = new NavigationPageViewModel();

			// Subscribe to property changes to update NavigationPage properties
			_viewModel.PropertyChanged += OnViewModelPropertyChanged;

			// Set initial NavigationPage properties based on Microsoft documentation
			SetInitialNavigationPageProperties();

			PushAsync(new NavigationPageControlMainPage(_viewModel));
		}

		private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			// Update NavigationPage properties when ViewModel properties change
			try
			{
				switch (e.PropertyName)
				{
					case nameof(_viewModel.BarBackgroundColor):
						BarBackgroundColor = _viewModel.BarBackgroundColor;
						break;
					case nameof(_viewModel.BarBackground):
						BarBackground = _viewModel.BarBackground;
						break;
					case nameof(_viewModel.BarTextColor):
						BarTextColor = _viewModel.BarTextColor;
						break;
				}
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"NavigationPage property update error: {ex.Message}");
			}
		}

		private void SetInitialNavigationPageProperties()
		{
			try
			{
				// Set NavigationPage properties (direct properties, not attached)
				BarBackgroundColor = _viewModel.BarBackgroundColor;
				BarBackground = _viewModel.BarBackground;
				BarTextColor = _viewModel.BarTextColor;
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Initial NavigationPage setup error: {ex.Message}");
			}
		}

		public NavigationPageViewModel ViewModel => _viewModel;
	}

	public partial class NavigationPageControlMainPage : ContentPage
	{
		private NavigationPageViewModel _viewModel;
		private int _preNavStackCount;
		private string _pendingOperation;
		// Carry context across events
		private string _pendingDestinationTitle; // predicted destination of current nav op
		private string _lastPoppedPageTitle;     // page title that was popped (for Pop/PopToRoot)

		public NavigationPageControlMainPage(NavigationPageViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			BindingContext = _viewModel;

			// Subscribe to property changes to update attached properties
			_viewModel.PropertyChanged += OnViewModelPropertyChanged;

			// Subscribe to navigation lifecycle events
			NavigatedTo += OnNavigatedTo;
			NavigatedFrom += OnNavigatedFrom;
			NavigatingFrom += OnNavigatingFrom;
		}

		private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			// Update NavigationPage attached properties when ViewModel properties change
			try
			{
				switch (e.PropertyName)
				{
					case nameof(_viewModel.HasNavigationBar):
						NavigationPage.SetHasNavigationBar(this, _viewModel.HasNavigationBar);
						break;
					case nameof(_viewModel.HasBackButton):
						NavigationPage.SetHasBackButton(this, _viewModel.HasBackButton);
						break;
					case nameof(_viewModel.BackButtonTitle):
						if (!string.IsNullOrEmpty(_viewModel.BackButtonTitle))
							NavigationPage.SetBackButtonTitle(this, _viewModel.BackButtonTitle);
						break;
					case nameof(_viewModel.IconColor):
						NavigationPage.SetIconColor(this, _viewModel.IconColor);
						break;
					case nameof(_viewModel.TitleIconImageSource):
						NavigationPage.SetTitleIconImageSource(this, _viewModel.TitleIconImageSource);
						break;
					case nameof(_viewModel.TitleView):
						NavigationPage.SetTitleView(this, _viewModel.TitleView);
						break;
				}
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Attached property update error: {ex.Message}");
			}
		}

		// Apply button - Updates all properties from UI inputs
		private void ApplyButton_Clicked(object sender, EventArgs e)
		{
			try
			{
				// Update text properties from entries
				if (!string.IsNullOrEmpty(BackButtonTitleEntry.Text))
					_viewModel.BackButtonTitle = BackButtonTitleEntry.Text;

				// Update boolean properties from checkboxes
				_viewModel.HasNavigationBar = HasNavigationBarCheckBox.IsChecked;
				_viewModel.HasBackButton = HasBackButtonCheckBox.IsChecked;

				UpdateNavigationInfo();
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Apply button error: {ex.Message}");
			}
		}

		// Reset button - Restores default property values and returns to root page
		private async void ResetButton_Clicked(object sender, EventArgs e)
		{
			try
			{
				_viewModel.Reset();

				// Pop to root if not already there
				if (Navigation?.NavigationStack?.Count > 1)
				{
					try
					{
						await Navigation.PopToRootAsync(false); // no animation for speed in tests
					}
					catch (System.Exception ex)
					{
						System.Diagnostics.Debug.WriteLine($"Reset PopToRoot error: {ex.Message}");
					}
				}

				// Re-sync UI controls to defaults
				BackButtonTitleEntry.Text = _viewModel.BackButtonTitle ?? string.Empty;
				HasNavigationBarCheckBox.IsChecked = _viewModel.HasNavigationBar;
				HasBackButtonCheckBox.IsChecked = _viewModel.HasBackButton;

				// Reapply attached properties to this root page
				NavigationPage.SetHasNavigationBar(this, _viewModel.HasNavigationBar);
				NavigationPage.SetHasBackButton(this, _viewModel.HasBackButton);
				NavigationPage.SetBackButtonTitle(this, _viewModel.BackButtonTitle);
				NavigationPage.SetIconColor(this, _viewModel.IconColor);
				NavigationPage.SetTitleIconImageSource(this, _viewModel.TitleIconImageSource);
				NavigationPage.SetTitleView(this, _viewModel.TitleView);

				UpdateNavigationInfo();
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Reset button error: {ex.Message}");
			}
		}

		// BarBackgroundColor - NavigationPage direct property
		private void BarBackgroundColorButton_Clicked(object sender, EventArgs e)
		{
			var button = (Button)sender;
			switch (button.Text)
			{
				case "Blue":
					_viewModel.BarBackgroundColor = Colors.Blue;
					break;
				case "Red":
					_viewModel.BarBackgroundColor = Colors.Red;
					break;
				case "Green":
					_viewModel.BarBackgroundColor = Colors.Green;
					break;
				case "Default":
					_viewModel.BarBackgroundColor = null;
					break;
			}
		}

		// BarBackground - NavigationPage direct property (Brush)
		private void BarBackgroundButton_Clicked(object sender, EventArgs e)
		{
			// Create a linear gradient brush for testing
			var gradient = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 1)
			};
			gradient.GradientStops.Add(new GradientStop { Color = Colors.Purple, Offset = 0.0f });
			gradient.GradientStops.Add(new GradientStop { Color = Colors.Orange, Offset = 1.0f });

			_viewModel.BarBackground = gradient;
		}

		private void BarBackgroundRadialButton_Clicked(object sender, EventArgs e)
		{
			// Create a radial gradient brush for testing
			var radialGradient = new RadialGradientBrush
			{
				Center = new Point(0.5, 0.5),
				Radius = 0.7
			};
			radialGradient.GradientStops.Add(new GradientStop { Color = Colors.Cyan, Offset = 0.0f });
			radialGradient.GradientStops.Add(new GradientStop { Color = Colors.Navy, Offset = 1.0f });

			_viewModel.BarBackground = radialGradient;
		}

		private void BarBackgroundClearButton_Clicked(object sender, EventArgs e)
		{
			_viewModel.BarBackground = null;
		}

		// BarTextColor - NavigationPage direct property
		private void BarTextColorButton_Clicked(object sender, EventArgs e)
		{
			var button = (Button)sender;
			switch (button.Text)
			{
				case "White":
					_viewModel.BarTextColor = Colors.White;
					break;
				case "Black":
					_viewModel.BarTextColor = Colors.Black;
					break;
				case "Yellow":
					_viewModel.BarTextColor = Colors.Yellow;
					break;
				case "Default":
					_viewModel.BarTextColor = null;
					break;
			}
		}

		// IconColor - NavigationPage attached property
		// NOTE: IconColor affects navigation bar icons, NOT toolbar buttons
		// This property colors icons like back button, title icon, etc. in the navigation bar
		private void IconColorButton_Clicked(object sender, EventArgs e)
		{
			var button = (Button)sender;
			switch (button.Text)
			{
				case "Red":
					_viewModel.IconColor = Colors.Red;
					break;
				case "Orange":
					_viewModel.IconColor = Colors.Orange;
					break;
				case "Purple":
					_viewModel.IconColor = Colors.Purple;
					break;
				case "Default":
					_viewModel.IconColor = null;
					break;
			}
		}

		// TitleIconImageSource - NavigationPage attached property
		private void TitleIconButton_Clicked(object sender, EventArgs e)
		{
			_viewModel.TitleIconImageSource = "coffee.png";
		}

		private void TitleIconClearButton_Clicked(object sender, EventArgs e)
		{
			_viewModel.TitleIconImageSource = null;
		}

		// TitleView - NavigationPage attached property
		private void TitleViewButton_Clicked(object sender, EventArgs e)
		{
			var customTitleView = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				WidthRequest = 200,
				HeightRequest = 44,
				Children =
				{
					new Image { Source = "coffee.png", WidthRequest = 24, HeightRequest = 24 },
					new Label { Text = "Custom Title", VerticalOptions = LayoutOptions.Center, TextColor = Colors.White, FontSize = 16 }
				}
			};
			_viewModel.TitleView = customTitleView;
		}

		private void TitleViewClearButton_Clicked(object sender, EventArgs e)
		{
			_viewModel.TitleView = null;
		}

		// HasNavigationBar - NavigationPage attached property
		private void OnHasNavigationBarChanged(object sender, CheckedChangedEventArgs e)
		{
			_viewModel.HasNavigationBar = HasNavigationBarCheckBox.IsChecked;
		}

		// HasBackButton - NavigationPage attached property
		private void OnHasBackButtonChanged(object sender, CheckedChangedEventArgs e)
		{
			_viewModel.HasBackButton = HasBackButtonCheckBox.IsChecked;
		}

		// Push a new page to test NavigationPage properties and attached properties
		private async void PushPage_Clicked(object sender, EventArgs e)
		{
			try
			{
				_pendingOperation = "Push";
				var pageNumber = Navigation.NavigationStack.Count + 1;
				var isLastPage = pageNumber >= 3; // Limit to 3 pages for testing

				// IMPORTANT: BackButtonTitle for the NEXT page is taken from the PREVIOUS page on iOS.
				// Set it on the current top-of-stack page BEFORE pushing the new page.
				var currentTop = Navigation?.NavigationStack?.LastOrDefault();
				if (!string.IsNullOrEmpty(_viewModel.BackButtonTitle) && currentTop != null)
				{
					NavigationPage.SetBackButtonTitle(currentTop, _viewModel.BackButtonTitle);
				}

				var stackLayout = new StackLayout
				{
					Padding = 20,
					Spacing = 15,
					Children =
					{
						new Label
						{
							Text = $"Page {pageNumber}",
							FontSize = 24,
							FontAttributes = FontAttributes.Bold,
							HorizontalTextAlignment = TextAlignment.Center,
							TextColor = Colors.Blue
						},
						new Label
						{
							Text = $"NavigationPage Test Page {pageNumber}",
							FontSize = 16,
							HorizontalTextAlignment = TextAlignment.Center
						},
						new Label
						{
							Text = $"HasNavigationBar: {_viewModel.HasNavigationBar}",
							FontSize = 14,
							HorizontalTextAlignment = TextAlignment.Center
						},
						new Label
						{
							Text = $"HasBackButton: {_viewModel.HasBackButton}",
							FontSize = 14,
							HorizontalTextAlignment = TextAlignment.Center
						},
						new Label
						{
							Text = $"BackButtonTitle: {_viewModel.BackButtonTitle}",
							FontSize = 14,
							HorizontalTextAlignment = TextAlignment.Center
						}
					}
				};

				// Add navigation buttons based on page level
				if (!isLastPage)
				{
					// Add "Push New Page" button for non-last pages
					stackLayout.Children.Add(new Button
					{
						Text = $"Push Page {pageNumber + 1}",
						Command = new Command(async () =>
						{
							try
							{
								_pendingOperation = "Push";
								await PushNextPage();
							}
							catch (System.Exception ex)
							{
								System.Diagnostics.Debug.WriteLine($"Push next page error: {ex.Message}");
							}
						}),
						AutomationId = $"PushPage{pageNumber + 1}Button",
						BackgroundColor = Colors.Green,
						TextColor = Colors.White,
						Margin = new Thickness(0, 10, 0, 0)
					});
				}
				else
				{
					// For last page, add Pop to Root and Pop Page buttons
					stackLayout.Children.Add(new Label
					{
						Text = "This is the last page in the stack",
						FontSize = 14,
						FontAttributes = FontAttributes.Italic,
						HorizontalTextAlignment = TextAlignment.Center,
						TextColor = Colors.Red,
						Margin = new Thickness(0, 10, 0, 0)
					});
				}

				// Always add Pop Page button
				stackLayout.Children.Add(new Button
				{
					Text = "Pop Page",
					Command = new Command(async () =>
					{
						try
						{
							_pendingOperation = "Pop";
							await Navigation.PopAsync();
						}
						catch (System.Exception ex)
						{
							System.Diagnostics.Debug.WriteLine($"Pop page error: {ex.Message}");
						}
					}),
					AutomationId = "PopPageButton",
					BackgroundColor = Colors.Orange,
					TextColor = Colors.White
				});

				// Add Pop to Root button (for all pages except root)
				if (pageNumber > 1)
				{
					stackLayout.Children.Add(new Button
					{
						Text = "Pop to Root",
						Command = new Command(async () =>
						{
							try
							{
								_pendingOperation = "PopToRoot";
								await Navigation.PopToRootAsync();
							}
							catch (System.Exception ex)
							{
								System.Diagnostics.Debug.WriteLine($"Pop to root error: {ex.Message}");
							}
						}),
						AutomationId = "PopToRootPageButton",
						BackgroundColor = Colors.Red,
						TextColor = Colors.White
					});
				}

				var newPage = new ContentPage
				{
					Title = $"Page {pageNumber}",
					Content = stackLayout
				};

				// Hook navigation events on created pages
				newPage.NavigatedTo += OnChildPageNavigatedTo;
				newPage.NavigatedFrom += OnChildPageNavigatedFrom;
				newPage.NavigatingFrom += OnChildPageNavigatingFrom;

				// Apply NavigationPage attached properties to the new page (do NOT set BackButtonTitle here)
				NavigationPage.SetHasNavigationBar(newPage, _viewModel.HasNavigationBar);
				NavigationPage.SetHasBackButton(newPage, _viewModel.HasBackButton);
				// BackButtonTitle intentionally applied to currentTop above

				// Always set these properties (including null values to clear them)
				NavigationPage.SetIconColor(newPage, _viewModel.IconColor);
				NavigationPage.SetTitleIconImageSource(newPage, _viewModel.TitleIconImageSource);
				NavigationPage.SetTitleView(newPage, _viewModel.TitleView);

				// Record destination title for this Push before navigating
				_pendingDestinationTitle = newPage?.Title;

				await Navigation.PushAsync(newPage);
				UpdateNavigationInfo();
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Push error: {ex.Message}");
			}
		}

		// Helper method to push next page from within pushed pages
		private async Task PushNextPage()
		{
			// Call the push page logic directly to avoid async void issues
			try
			{
				_pendingOperation = "Push";
				// Set BackButtonTitle on the current top-of-stack page BEFORE pushing
				var currentTop = Navigation?.NavigationStack?.LastOrDefault();
				if (!string.IsNullOrEmpty(_viewModel.BackButtonTitle) && currentTop != null)
				{
					NavigationPage.SetBackButtonTitle(currentTop, _viewModel.BackButtonTitle);
				}
				var pageNumber = Navigation.NavigationStack.Count + 1;
				var stackLayout = new StackLayout
				{
					Padding = 20,
					Spacing = 15,
					Children =
					{
						new Label
						{
							Text = $"Page {pageNumber}",
							FontSize = 24,
							FontAttributes = FontAttributes.Bold,
							HorizontalTextAlignment = TextAlignment.Center,
							TextColor = Colors.Blue
						},
						new Label
						{
							Text = $"NavigationPage Test Page {pageNumber}",
							FontSize = 16,
							HorizontalTextAlignment = TextAlignment.Center
						},
						new Button
						{
							Text = "Pop Page",
							Command = new Command(async () => {
								try
								{
									_pendingOperation = "Pop";
									await Navigation.PopAsync();
								}
								catch (System.Exception ex)
								{
									System.Diagnostics.Debug.WriteLine($"Pop page error: {ex.Message}");
								}
							}),
							AutomationId = "PopPageButton",
							BackgroundColor = Colors.Orange,
							TextColor = Colors.White
						},
						new Button
						{
							Text = "Pop to Root",
							Command = new Command(async () => {
								try
								{
									_pendingOperation = "PopToRoot";
									await Navigation.PopToRootAsync();
								}
								catch (System.Exception ex)
								{
									System.Diagnostics.Debug.WriteLine($"Pop to root error: {ex.Message}");
								}
							}),
							AutomationId = "PopToRootPageButton",
							BackgroundColor = Colors.Red,
							TextColor = Colors.White
						}
					}
				};

				var newPage = new ContentPage
				{
					Title = $"Page {pageNumber}",
					Content = stackLayout
				};

				// Hook navigation events on created pages
				newPage.NavigatedTo += OnChildPageNavigatedTo;
				newPage.NavigatedFrom += OnChildPageNavigatedFrom;
				newPage.NavigatingFrom += OnChildPageNavigatingFrom;

				// Apply NavigationPage attached properties to the new page (mirrors PushPage_Clicked)
				NavigationPage.SetHasNavigationBar(newPage, _viewModel.HasNavigationBar);
				NavigationPage.SetHasBackButton(newPage, _viewModel.HasBackButton);
				// BackButtonTitle is intentionally NOT set on newPage
				NavigationPage.SetIconColor(newPage, _viewModel.IconColor);
				NavigationPage.SetTitleIconImageSource(newPage, _viewModel.TitleIconImageSource);
				NavigationPage.SetTitleView(newPage, _viewModel.TitleView);

				// Record destination title for this Push before navigating
				_pendingDestinationTitle = newPage?.Title;

				await Navigation.PushAsync(newPage);
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"PushNextPage error: {ex.Message}");
			}
		}

		// PopToRoot method - from Microsoft documentation
		private async void PopToRoot_Clicked(object sender, EventArgs e)
		{
			try
			{
				_pendingOperation = "PopToRoot";
				await Navigation.PopToRootAsync();
				UpdateNavigationInfo();
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"PopToRoot error: {ex.Message}");
			}
		}

		// Update navigation information display
		private void UpdateNavigationInfo()
		{
			try
			{
				// Get NavigationPage from parent
				var navigationPage = Parent as NavigationPage;
				if (navigationPage != null)
				{
					CurrentPageLabel.Text = navigationPage.CurrentPage?.Title ?? "N/A";
					RootPageLabel.Text = navigationPage.RootPage?.Title ?? "N/A";
				}
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"UpdateNavigationInfo error: {ex.Message}");
			}
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			try
			{
				// Initialize UI controls with current values
				BackButtonTitleEntry.Text = _viewModel.BackButtonTitle ?? "";
				HasNavigationBarCheckBox.IsChecked = _viewModel.HasNavigationBar;
				HasBackButtonCheckBox.IsChecked = _viewModel.HasBackButton;

				// Apply initial NavigationPage attached properties
				NavigationPage.SetHasNavigationBar(this, _viewModel.HasNavigationBar);
				NavigationPage.SetHasBackButton(this, _viewModel.HasBackButton);
				if (!string.IsNullOrEmpty(_viewModel.BackButtonTitle))
					NavigationPage.SetBackButtonTitle(this, _viewModel.BackButtonTitle);
				if (_viewModel.IconColor != null)
					NavigationPage.SetIconColor(this, _viewModel.IconColor);
				if (_viewModel.TitleIconImageSource != null)
					NavigationPage.SetTitleIconImageSource(this, _viewModel.TitleIconImageSource);
				if (_viewModel.TitleView != null)
					NavigationPage.SetTitleView(this, _viewModel.TitleView);

				// Update navigation information
				UpdateNavigationInfo();
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"OnAppearing error: {ex.Message}");
			}
		}

		// Navigation event handlers (Page lifecycle navigation events)
		private void OnNavigatedTo(object sender, NavigatedToEventArgs e)
		{
			_viewModel.NavigatedToCount += 1;
			_viewModel.NavigatedToRaised = true;
			_viewModel.LastNavigationEvent = $"NavigatedTo: {Title}";
			_viewModel.LastNavigatedToPage = Title;
			try
			{
				var stack = this.Navigation?.NavigationStack;
				string prevTitle = null;
				// After Pop/PopToRoot the previous page is no longer in the stack; use stored popped title
				if (_pendingOperation == "Pop" || _pendingOperation == "PopToRoot")
				{
					prevTitle = _lastPoppedPageTitle;
				}
				else if (stack != null && stack.Count >= 2)
				{
					prevTitle = stack[stack.Count - 2]?.Title;
				}
				var toParams = $"PreviousPageTitle={prevTitle ?? "<none>"}, StackCount={stack?.Count ?? 0}";
				_viewModel.LastNavigatedToParameters = toParams;
				_viewModel.LastNavigationParameters = toParams;
			}
			catch { }
			UpdateNavigationInfo();
			_pendingOperation = null;
			// Clear one-shot context after completion
			_pendingDestinationTitle = null;
			_lastPoppedPageTitle = null;
		}

		private void OnNavigatedFrom(object sender, NavigatedFromEventArgs e)
		{
			_viewModel.NavigatedFromCount += 1;
			_viewModel.NavigatedFromRaised = true;
			_viewModel.LastNavigationEvent = $"NavigatedFrom: {Title}";
			_viewModel.LastNavigatedFromPage = Title;
			try
			{
				var page = (Page)sender;
				var nav = page?.Navigation ?? this.Navigation;
				var stack = nav?.NavigationStack;
				var afterStack = stack;
				if ((afterStack == null || afterStack.Count == 0) && (_pendingOperation == "Pop" || _pendingOperation == "PopToRoot"))
				{
					// Use destination navigation stack when sender's nav is detached
					afterStack = this.Navigation?.NavigationStack;
				}
				var afterCount = afterStack?.Count ?? 0;
				var dest = (afterCount > 0) ? afterStack[afterCount - 1] : null;
				bool stillInStack = stack?.Contains(page) == true;
				string navType = stillInStack ? "Push" : (_pendingOperation ?? "Unknown");
				var fromParams = $"DestinationTitle={(dest?.Title ?? _pendingDestinationTitle ?? "<none>")}, Type={navType}, BeforeCount={_preNavStackCount}, AfterCount={afterCount}";
				_viewModel.LastNavigatedFromParameters = fromParams;
				_viewModel.LastNavigationParameters = fromParams;
			}
			catch { }
			_pendingOperation = null;
		}

		private void OnNavigatingFrom(object sender, NavigatingFromEventArgs e)
		{
			_viewModel.NavigatingFromCount += 1;
			_viewModel.NavigatingFromRaised = true;
			_viewModel.LastNavigationEvent = $"NavigatingFrom: {Title}";
			_viewModel.LastNavigatingFromPage = Title;
			try
			{
				_preNavStackCount = this.Navigation?.NavigationStack?.Count ?? 0;
				// Predict destination for Push using the page prepared for navigation
				string predictedDest = null;
				if (_pendingOperation == "Push")
					predictedDest = _pendingDestinationTitle;
				var preParams = $"BeforeCount={_preNavStackCount}, Requested={_pendingOperation ?? "<unknown>"}, DestinationTitle={predictedDest ?? "<none>"}";
				_viewModel.LastNavigatingFromParameters = preParams;
				_viewModel.LastNavigationParameters = preParams;
			}
			catch { }
		}

		// Child page event handlers to observe events across the stack
		private void OnChildPageNavigatedTo(object sender, NavigatedToEventArgs e)
		{
			if (sender is Page p)
			{
				_viewModel.NavigatedToCount += 1;
				_viewModel.NavigatedToRaised = true;
				_viewModel.LastNavigationEvent = $"NavigatedTo: {p.Title}";
				_viewModel.LastNavigatedToPage = p.Title;
				try
				{
					var nav = p.Navigation ?? this.Navigation;
					var stack = nav?.NavigationStack;
					string prevTitle = null;
					if (stack != null && stack.Count >= 2)
						prevTitle = stack[stack.Count - 2]?.Title;
					var toParams = $"PreviousPageTitle={prevTitle ?? "<none>"}, StackCount={stack?.Count ?? 0}";
					_viewModel.LastNavigatedToParameters = toParams;
					_viewModel.LastNavigationParameters = toParams;
				}
				catch { }
				UpdateNavigationInfo();
			}
		}

		private void OnChildPageNavigatedFrom(object sender, NavigatedFromEventArgs e)
		{
			if (sender is Page p)
			{
				_viewModel.NavigatedFromCount += 1;
				_viewModel.NavigatedFromRaised = true;
				_viewModel.LastNavigationEvent = $"NavigatedFrom: {p.Title}";
				_viewModel.LastNavigatedFromPage = p.Title;
				_lastPoppedPageTitle = p.Title; // capture for upcoming NavigatedTo
				try
				{
					var nav = p.Navigation ?? this.Navigation;
					var stack = nav?.NavigationStack;
					var afterStack = stack;
					if ((afterStack == null || afterStack.Count == 0) && (_pendingOperation == "Pop" || _pendingOperation == "PopToRoot"))
					{
						// Use destination navigation stack when sender's nav is detached
						afterStack = this.Navigation?.NavigationStack;
					}
					var afterCount = afterStack?.Count ?? 0;
					var dest = (afterCount > 0) ? afterStack[afterCount - 1] : null;
					bool stillInStack = stack?.Contains(p) == true;
					string navType = stillInStack ? "Push" : (_pendingOperation ?? "Unknown");
					var fromParams = $"DestinationTitle={(dest?.Title ?? _pendingDestinationTitle ?? "<none>")}, Type={navType}, BeforeCount={_preNavStackCount}, AfterCount={afterCount}";
					_viewModel.LastNavigatedFromParameters = fromParams;
					_viewModel.LastNavigationParameters = fromParams;
				}
				catch { }
			}
		}

		private void OnChildPageNavigatingFrom(object sender, NavigatingFromEventArgs e)
		{
			if (sender is Page p)
			{
				_viewModel.NavigatingFromCount += 1;
				_viewModel.NavigatingFromRaised = true;
				_viewModel.LastNavigationEvent = $"NavigatingFrom: {p.Title}";
				_viewModel.LastNavigatingFromPage = p.Title;
				try
				{
					var nav = p.Navigation ?? this.Navigation;
					_preNavStackCount = nav?.NavigationStack?.Count ?? 0;
					// Predict destination for Pop/PopToRoot based on current stack
					string predictedDest = null;
					if (_pendingOperation == "PopToRoot")
					{
						predictedDest = nav?.NavigationStack?.FirstOrDefault()?.Title;
					}
					else if (_pendingOperation == "Pop")
					{
						var count = nav?.NavigationStack?.Count ?? 0;
						if (count >= 2)
							predictedDest = nav.NavigationStack[count - 2]?.Title;
					}
					else if (_pendingOperation == "Push")
					{
						predictedDest = _pendingDestinationTitle;
					}
					_pendingDestinationTitle = predictedDest;
					_lastPoppedPageTitle = p.Title;
					var preParams = $"BeforeCount={_preNavStackCount}, Requested={_pendingOperation ?? "<unknown>"}, DestinationTitle={predictedDest ?? "<none>"}";
					_viewModel.LastNavigatingFromParameters = preParams;
					_viewModel.LastNavigationParameters = preParams;
				}
				catch { }
			}
		}
	}
}

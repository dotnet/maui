using Microsoft.Maui.Layouts;
#if IOS
using Microsoft.Maui.Handlers;
using UIKit;
#endif
using ControlsPage = Microsoft.Maui.Controls.Page;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33037, "iOS Large Title display disappears when scrolling in non-Shell NavigationPage", PlatformAffected.iOS, issueTestNumber: 1)]
public class Issue33037NonShell : NavigationPage
{
	public Issue33037NonShell() : base(new Issue33037NonShellRootPage())
	{
		BarBackgroundColor = Colors.Transparent;
		BackgroundColor = Colors.Brown;
		Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetPrefersLargeTitles(this, true);
	}
}

public class Issue33037NonShellRootPage : ContentPage
{
	public Issue33037NonShellRootPage()
	{
		Title = "Issue 33037 Non-Shell";

		Content = new ScrollView
		{
			AutomationId = "Issue33037ScenarioMenuScroller",
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 12,
				Children =
				{
					new Label
					{
						Text = "Select a non-Shell NavigationPage large-title scenario.",
						FontAttributes = FontAttributes.Bold
					},
					CreateModalButton(),
					CreateReporterScenarioButton(),
					CreateOpaqueNavigationButton(),
					CreateButton("Issue33037ScrollViewButton", "Direct ScrollView", () => new Issue33037NonShellScrollViewPage()),
					CreateButton("Issue33037GridScrollViewButton", "Grid wrapping ScrollView", () => new Issue33037NonShellGridScrollViewPage()),
					CreateButton("Issue33037ContentViewGridScrollViewButton", "ContentView wrapping Grid/ScrollView", () => new Issue33037NonShellContentViewGridScrollViewPage()),
					CreateButton("Issue33037DynamicContentViewGridScrollViewButton", "Late ContentView wrapping Grid/ScrollView", () => new Issue33037NonShellDynamicContentViewGridScrollViewPage()),
					CreateButton("Issue33037ListViewButton", "ListView", () => new Issue33037NonShellListViewPage()),
					CreateButton("Issue33037CollectionViewButton", "CollectionView", () => new Issue33037NonShellCollectionViewPage()),
					CreateButton("Issue33037LegacyCollectionViewButton", "Legacy CollectionView with header", () => new Issue33037NonShellLegacyCollectionViewPage()),
#if IOS
					CreateButton("Issue33037NativeTableViewButton", "Custom control backed by native UITableView", () => new Issue33037NativeTableViewPage()),
#endif
					CreateButton("Issue33037TableViewButton", "Grid wrapping TableView", () => new Issue33037NonShellTableViewPage()),
					CreateButton("Issue33037WebViewButton", "Grid wrapping WebView", () => new Issue33037NonShellWebViewPage()),
					CreateButton("Issue33037CandidateSelectionButton", "Hidden and horizontal scrollers before vertical content", () => new Issue33037NonShellCandidateSelectionPage()),
					CreateButton("Issue33037FixedHeaderCollectionViewButton", "Fixed header with CollectionView", () => new Issue33037NonShellFixedHeaderCollectionViewPage()),
					CreateButton("Issue33037OrdinaryHeaderButton", "Ordinary header preserves safe area", () => new Issue33037NonShellOrdinaryHeaderPage()),
					CreateButton("Issue33037MultipleCandidatesButton", "Multiple candidates preserve safe area", () => new Issue33037NonShellMultipleCandidatesPage()),
					CreateButton("Issue33037ExplicitSafeAreaButton", "Explicit safe-area ownership and reset", () => new Issue33037NonShellExplicitSafeAreaPage()),
					CreateButton("Issue33037LargeTitleNeverButton", "Large-title opt-out preserves safe area", () => new Issue33037NonShellLargeTitleNeverPage()),
					CreateButton("Issue33037ShortFixedHeaderCollectionViewButton", "Short fixed header with CollectionView", () => new Issue33037NonShellShortFixedHeaderCollectionViewPage()),
					CreateButton("Issue33037ProgrammaticCollectionViewButton", "Programmatic CollectionView scroll", () => new Issue33037NonShellProgrammaticCollectionViewPage()),
					CreateButton("Issue33037AppearingCollectionViewButton", "OnAppearing CollectionView scroll", () => new Issue33037NonShellAppearingCollectionViewPage()),
					CreateButton("Issue33037HiddenNavigationBarButton", "Hidden navigation bar", () => new Issue33037NonShellHiddenNavigationBarPage())
				}
			}
		};
	}

	Button CreateButton(string automationId, string text, Func<ControlsPage> createPage)
	{
		var button = new Button
		{
			AutomationId = automationId,
			Text = text
		};

		button.Clicked += async (_, _) => await Navigation.PushAsync(createPage());
		return button;
	}

	Button CreateModalButton()
	{
		var button = new Button
		{
			AutomationId = "Issue33037ModalListViewButton",
			Text = "Modal NavigationPage with ListView"
		};

		button.Clicked += async (_, _) =>
		{
			var navigationPage = new NavigationPage(new Issue33037NonShellModalListViewPage())
			{
				BarBackgroundColor = Colors.Transparent
			};
			Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetPrefersLargeTitles(navigationPage, true);
			await Navigation.PushModalAsync(navigationPage);
		};

		return button;
	}

	Button CreateReporterScenarioButton()
	{
		var button = new Button
		{
			AutomationId = "Issue33037ReporterScenarioButton",
			Text = "Reporter 19-row ListView with bottom overlay"
		};

		button.Clicked += async (_, _) =>
		{
			var navigationPage = new NavigationPage(new Issue33037ReporterScenarioPage())
			{
				BarBackgroundColor = Colors.Transparent
			};
			Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetPrefersLargeTitles(navigationPage, true);
			await Navigation.PushModalAsync(navigationPage);
		};

		return button;
	}

	Button CreateOpaqueNavigationButton()
	{
		var button = new Button
		{
			AutomationId = "Issue33037OpaqueNavigationButton",
			Text = "Opaque navigation bar preserves safe area"
		};

		button.Clicked += async (_, _) =>
		{
			var navigationPage = new NavigationPage(new Issue33037OpaqueNavigationPage())
			{
				BarBackgroundColor = Colors.DarkBlue,
				BarTextColor = Colors.White
			};
			Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetPrefersLargeTitles(navigationPage, true);
			await Navigation.PushModalAsync(navigationPage);
		};

		return button;
	}
}

abstract class Issue33037NonShellScenarioPage : ContentPage
{
	protected Issue33037NonShellScenarioPage(string title)
	{
		Title = title;
		Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetLargeTitleDisplay(
			this,
			Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.LargeTitleDisplayMode.Automatic);
	}

	protected static View CreateStackContent(string automationIdPrefix)
	{
		var stack = new VerticalStackLayout
		{
			Padding = 16,
			Spacing = 4
		};

		stack.Children.Add(new Label
		{
			AutomationId = $"{automationIdPrefix}Instructions",
			Text = "Scroll down to collapse the large navigation title.",
			FontAttributes = FontAttributes.Bold
		});

		for (int i = 0; i < 60; i++)
		{
			stack.Children.Add(new Label
			{
				AutomationId = $"{automationIdPrefix}Item{i}",
				Text = $"Item {i}"
			});
		}

		return stack;
	}

	internal static IList<string> CreateItems(int count = 60)
	{
		var items = new List<string>();
		for (int i = 0; i < count; i++)
		{
			items.Add($"Item {i}");
		}

		return items;
	}
}

class Issue33037NonShellScrollViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellScrollViewPage() : base("Issue33037 Direct")
	{
		Content = new ScrollView
		{
			AutomationId = "Issue33037ScrollViewScroller",
			Content = CreateStackContent("Issue33037Direct")
		};
	}
}

class Issue33037NonShellGridScrollViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellGridScrollViewPage() : base("Issue33037 Grid")
	{
		var grid = new Grid
		{
			Children =
			{
				new ScrollView
				{
					AutomationId = "Issue33037GridScrollViewScroller",
					Content = CreateStackContent("Issue33037Grid")
				},
				new ActivityIndicator
				{
					AutomationId = "Issue33037GridActivityIndicator",
					IsVisible = false
				}
			}
		};

		Content = grid;
	}
}

class Issue33037NonShellContentViewGridScrollViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellContentViewGridScrollViewPage() : base("Issue33037 Wrapped")
	{
		Content = new ContentView
		{
			Content = new Grid
			{
				Children =
				{
					new ScrollView
					{
						AutomationId = "Issue33037ContentViewGridScrollViewScroller",
						Content = CreateStackContent("Issue33037Wrapped")
					}
				}
			}
		};
	}
}

class Issue33037NonShellDynamicContentViewGridScrollViewPage : Issue33037NonShellScenarioPage
{
	readonly ContentView _contentView = new();
	bool _contentCreated;

	public Issue33037NonShellDynamicContentViewGridScrollViewPage() : base("Issue33037 Dynamic")
	{
		Content = _contentView;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		if (_contentCreated)
			return;

		_contentCreated = true;
		_contentView.Content = new Grid
		{
			Children =
			{
				new ScrollView
				{
					AutomationId = "Issue33037DynamicContentViewGridScrollViewScroller",
					Content = CreateStackContent("Issue33037Dynamic")
				}
			}
		};
	}
}

class Issue33037NonShellListViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellListViewPage() : base("Issue33037 List")
	{
#pragma warning disable CS0618 // ListView/ViewCell obsolete - intentionally covering issue #33037 legacy ListView behavior
		Content = new ListView
		{
			AutomationId = "Issue33037ListViewScroller",
			ItemsSource = CreateItems(),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(16, 12)
				};
				label.SetBinding(Label.TextProperty, ".");
				return new ViewCell { View = label };
			})
		};
#pragma warning restore CS0618
	}
}

class Issue33037NonShellModalListViewPage : ContentPage
{
	public Issue33037NonShellModalListViewPage()
	{
		Title = "Issue33037 Modal List";

#pragma warning disable CS0618 // ListView/ViewCell obsolete - intentionally matching the reported issue #33037 scenario
		var listView = new ListView
		{
			AutomationId = "Issue33037ModalListViewScroller",
			ItemsSource = Issue33037NonShellScenarioPage.CreateItems(60),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					HeightRequest = 50,
					Padding = new Thickness(16, 12)
				};
				label.SetBinding(Label.TextProperty, ".");
				return new ViewCell { View = label };
			})
		};
#pragma warning restore CS0618

		var closeButton = new Button
		{
			AutomationId = "Issue33037ModalListViewCloseButton",
			HorizontalOptions = LayoutOptions.Center,
			Margin = 12,
			Text = "Close"
		};
		closeButton.Clicked += async (_, _) => await Navigation.PopModalAsync();

		AbsoluteLayout.SetLayoutFlags(listView, AbsoluteLayoutFlags.All);
		AbsoluteLayout.SetLayoutBounds(listView, new Rect(0, 0, 1, 1));

		var listContainer = new AbsoluteLayout
		{
			Children =
			{
				listView
			}
		};

		var content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Auto)
			},
			Children =
			{
				listContainer,
				closeButton
			}
		};
		Grid.SetRow(closeButton, 1);
		Content = content;
	}
}

class Issue33037ReporterScenarioPage : ContentPage
{
	public Issue33037ReporterScenarioPage()
	{
		Title = "Large Title Demo";

#pragma warning disable CS0618 // ListView/ViewCell intentionally match the reporter's issue #33037 sample.
		var listView = new ListView
		{
			AutomationId = "Issue33037ReporterScroller",
			BackgroundColor = Color.FromArgb("#F4F7FB"),
			RowHeight = 50,
			SeparatorColor = Color.FromArgb("#D5DBE3"),
			ItemsSource = Issue33037NonShellScenarioPage.CreateItems(19),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					BackgroundColor = Colors.White,
					Padding = new Thickness(20, 0),
					TextColor = Color.FromArgb("#1D2733"),
					VerticalTextAlignment = TextAlignment.Center
				};
				label.SetBinding(Label.TextProperty, ".");
				return new ViewCell { View = label };
			})
		};
#pragma warning restore CS0618

		var closeButton = new Button
		{
			AutomationId = "Issue33037ReporterCloseButton",
			BackgroundColor = Color.FromArgb("#0A84FF"),
			CornerRadius = 12,
			FontAttributes = FontAttributes.Bold,
			Text = "Close",
			TextColor = Colors.White,
			ZIndex = 1
		};
		closeButton.Clicked += async (_, _) => await Navigation.PopModalAsync();

		AbsoluteLayout.SetLayoutFlags(listView, AbsoluteLayoutFlags.All);
		AbsoluteLayout.SetLayoutBounds(listView, new Rect(0, 0, 1, 1));
		AbsoluteLayout.SetLayoutFlags(closeButton, AbsoluteLayoutFlags.PositionProportional);
		AbsoluteLayout.SetLayoutBounds(closeButton, new Rect(0.5, 1, 200, 40));

		Content = new AbsoluteLayout
		{
			Children =
			{
				listView,
				closeButton
			}
		};
	}
}

class Issue33037OpaqueNavigationPage : ContentPage
{
	public Issue33037OpaqueNavigationPage()
	{
		Title = "Issue33037 Opaque";
		Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetLargeTitleDisplay(
			this,
			Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.LargeTitleDisplayMode.Automatic);

		var closeButton = new Button
		{
			AutomationId = "Issue33037OpaqueNavigationCloseButton",
			Text = "Close",
			ZIndex = 1
		};
		closeButton.Clicked += async (_, _) => await Navigation.PopModalAsync();

		Content = new Grid
		{
			Children =
			{
				new CollectionView
				{
					AutomationId = "Issue33037OpaqueNavigationScroller",
					ItemsSource = Issue33037NonShellScenarioPage.CreateItems()
				},
				new VerticalStackLayout
				{
					VerticalOptions = LayoutOptions.End,
					ZIndex = 1,
					Children = { closeButton }
				}
			}
		};
	}
}

class Issue33037NonShellCollectionViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellCollectionViewPage() : base("Issue33037 Collection")
	{
		Content = new CollectionView
		{
			AutomationId = "Issue33037CollectionViewScroller",
			ItemsSource = CreateItems(),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(16, 12)
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};
	}
}

class Issue33037NonShellLegacyCollectionViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellLegacyCollectionViewPage() : base("Issue33037 Legacy Collection")
	{
		Content = new Maui.Controls.Sample.CollectionView1
		{
			AutomationId = "Issue33037LegacyCollectionViewScroller",
			Header = new Label
			{
				AutomationId = "Issue33037LegacyCollectionViewHeader",
				Padding = new Thickness(16, 12),
				Text = "Legacy CollectionView header"
			},
			ItemsSource = CreateItems(),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(16, 12)
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};
	}
}

#if IOS
class Issue33037NativeTableViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NativeTableViewPage() : base("Issue33037 Native")
	{
		Content = new Issue33037NativeTableView
		{
			AutomationId = "Issue33037NativeTableViewScroller"
		};
	}
}

public class Issue33037NativeTableView : View
{
}

public class Issue33037NativeTableViewHandler : ViewHandler<Issue33037NativeTableView, UITableView>
{
	readonly Issue33037NativeTableViewSource _source = new();

	public static readonly IPropertyMapper<Issue33037NativeTableView, Issue33037NativeTableViewHandler> Mapper =
		new PropertyMapper<Issue33037NativeTableView, Issue33037NativeTableViewHandler>(ViewHandler.ViewMapper);

	public Issue33037NativeTableViewHandler() : base(Mapper)
	{
	}

	protected override UITableView CreatePlatformView()
	{
		return new UITableView
		{
			AlwaysBounceVertical = true,
			RowHeight = 50,
			SeparatorInset = new UIEdgeInsets(0, 16, 0, 0),
			Source = _source
		};
	}
}

class Issue33037NativeTableViewSource : UITableViewSource
{
	const string ReuseIdentifier = "Issue33037NativeCell";

	public override nint RowsInSection(UITableView tableview, nint section) => 60;

	public override UITableViewCell GetCell(UITableView tableView, Foundation.NSIndexPath indexPath)
	{
		var cell = tableView.DequeueReusableCell(ReuseIdentifier) ??
			new UITableViewCell(UITableViewCellStyle.Default, ReuseIdentifier);
		var label = cell.ContentView.ViewWithTag(1) as UILabel;
		if (label is null)
		{
			label = new UILabel(new CoreGraphics.CGRect(
				16,
				0,
				Math.Max(0, cell.ContentView.Bounds.Width - 32),
				cell.ContentView.Bounds.Height))
			{
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
				Tag = 1
			};
			cell.ContentView.AddSubview(label);
		}

		label.Text = $"Item {indexPath.Row}";
		return cell;
	}
}
#endif

class Issue33037NonShellTableViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellTableViewPage() : base("Issue33037 Table")
	{
		var section = new TableSection();
		for (int i = 0; i < 60; i++)
			section.Add(new TextCell { Text = $"Item {i}" });

		Content = new Grid
		{
			Children =
			{
				new TableView
				{
					AutomationId = "Issue33037TableViewScroller",
					Root = new TableRoot { section }
				}
			}
		};
	}
}

class Issue33037NonShellWebViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellWebViewPage() : base("Issue33037 Web")
	{
		var readyLabel = new Label
		{
			AutomationId = "Issue33037WebViewReady",
			Text = "Loading"
		};

		var webView = new WebView
		{
			AutomationId = "Issue33037WebViewScroller",
			Source = new HtmlWebViewSource
			{
				Html = """
					<!doctype html>
					<html>
					<head><meta name="viewport" content="width=device-width, initial-scale=1"></head>
					<body style="margin: 0; font: 20px sans-serif;">
						<div style="height: 3000px; padding: 16px;">Scrollable WebView content</div>
					</body>
					</html>
					"""
			}
		};
		webView.Navigated += (_, _) => readyLabel.Text = "Ready";

		var scrollButton = new Button
		{
			AutomationId = "Issue33037WebViewScrollButton",
			Text = "Scroll WebView"
		};
		scrollButton.Clicked += async (_, _) =>
		{
			await webView.EvaluateJavaScriptAsync("window.scrollTo(0, 1200)");
			await Task.Delay(500);
			readyLabel.Text = "Scrolled";
		};

		Grid.SetRow(webView, 1);

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				new HorizontalStackLayout
				{
					ZIndex = 1,
					Children =
					{
						scrollButton,
						readyLabel
					}
				},
				webView
			}
		};
	}
}

class Issue33037NonShellCandidateSelectionPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellCandidateSelectionPage() : base("Issue33037 Candidates")
	{
		var horizontalCollectionView = new CollectionView
		{
			AutomationId = "Issue33037HorizontalCollectionView",
			HeightRequest = 80,
			ZIndex = 1,
			ItemsSource = CreateItems(10),
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					WidthRequest = 100,
					Padding = 8
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		var verticalCollectionView = new CollectionView
		{
			AutomationId = "Issue33037CandidateSelectionScroller",
			ItemsSource = CreateItems(),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(16, 12)
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		Grid.SetRow(verticalCollectionView, 1);

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				new ScrollView
				{
					AutomationId = "Issue33037HiddenScrollView",
					IsVisible = false,
					Content = CreateStackContent("Issue33037HiddenCandidate")
				},
				horizontalCollectionView,
				verticalCollectionView
			}
		};
	}
}

class Issue33037NonShellFixedHeaderCollectionViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellFixedHeaderCollectionViewPage() : base("Issue33037 Fixed Header")
	{
		var header = new Label
		{
			AutomationId = "Issue33037FixedHeader",
			Text = "Fixed header must remain below the navigation bar",
			BackgroundColor = Colors.LightBlue,
			FontAttributes = FontAttributes.Bold,
			Padding = 12,
			ZIndex = 1
		};

		var collectionView = new CollectionView
		{
			AutomationId = "Issue33037FixedHeaderCollectionViewScroller",
			ItemsSource = CreateItems(),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(16, 12)
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		Grid.SetRow(collectionView, 1);

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				header,
				collectionView
			}
		};
	}
}

class Issue33037NonShellShortFixedHeaderCollectionViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellShortFixedHeaderCollectionViewPage() : base("Issue33037 Short Header")
	{
		var header = new Label
		{
			AutomationId = "Issue33037ShortFixedHeader",
			Text = "Short content must still collapse the title",
			BackgroundColor = Colors.LightBlue,
			FontAttributes = FontAttributes.Bold,
			Padding = 12,
			ZIndex = 1
		};

		var collectionView = new CollectionView
		{
			AutomationId = "Issue33037ShortFixedHeaderCollectionViewScroller",
			ItemsSource = CreateItems(17),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					HeightRequest = 50,
					Padding = new Thickness(16, 0),
					VerticalTextAlignment = TextAlignment.Center
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		Grid.SetRow(collectionView, 1);

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				header,
				collectionView
			}
		};
	}
}

class Issue33037NonShellOrdinaryHeaderPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellOrdinaryHeaderPage() : base("Issue33037 Ordinary Header")
	{
		var header = new Label
		{
			AutomationId = "Issue33037OrdinaryHeader",
			Text = "This ordinary header must remain in the safe-area layout.",
			BackgroundColor = Colors.LightBlue,
			FontAttributes = FontAttributes.Bold,
			Padding = 12
		};

		var collectionView = new CollectionView
		{
			AutomationId = "Issue33037OrdinaryHeaderScroller",
			ItemsSource = CreateItems(),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(16, 12)
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		Grid.SetRow(collectionView, 1);

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				header,
				collectionView
			}
		};
	}
}

class Issue33037NonShellMultipleCandidatesPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellMultipleCandidatesPage() : base("Issue33037 Multiple Candidates")
	{
		var first = new CollectionView
		{
			AutomationId = "Issue33037FirstCandidate",
			ItemsSource = CreateItems()
		};

		var second = new CollectionView
		{
			AutomationId = "Issue33037SecondCandidate",
			ItemsSource = CreateItems()
		};

		Content = new AbsoluteLayout
		{
			Children =
			{
				first,
				second
			}
		};

		AbsoluteLayout.SetLayoutBounds(first, new Rect(0, 0, 0.5, 1));
		AbsoluteLayout.SetLayoutFlags(first, AbsoluteLayoutFlags.All);
		AbsoluteLayout.SetLayoutBounds(second, new Rect(1, 0, 0.5, 1));
		AbsoluteLayout.SetLayoutFlags(second, AbsoluteLayoutFlags.All);
	}
}

class Issue33037NonShellExplicitSafeAreaPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellExplicitSafeAreaPage() : base("Issue33037 Explicit Safe Area")
	{
		var scrollView = new ScrollView
		{
			AutomationId = "Issue33037ExplicitSafeAreaScroller",
			Content = CreateStackContent("Issue33037ExplicitSafeArea")
		};

		var toggle = new Button
		{
			AutomationId = "Issue33037ExplicitSafeAreaToggle",
			Text = "Take explicit safe-area ownership"
		};
		toggle.Clicked += (_, _) =>
		{
			scrollView.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
			toggle.Text = "Explicit safe-area ownership active";
		};

		Content = new Grid
		{
			Children =
			{
				scrollView,
				new VerticalStackLayout
				{
					VerticalOptions = LayoutOptions.End,
					ZIndex = 1,
					Children = { toggle }
				}
			}
		};
	}
}

class Issue33037NonShellLargeTitleNeverPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellLargeTitleNeverPage() : base("Issue33037 No Large Title")
	{
		Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetLargeTitleDisplay(
			this,
			Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.LargeTitleDisplayMode.Never);

		Content = new CollectionView
		{
			AutomationId = "Issue33037LargeTitleNeverScroller",
			ItemsSource = CreateItems()
		};
	}
}

class Issue33037NonShellProgrammaticCollectionViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellProgrammaticCollectionViewPage() : base("Issue33037 Programmatic")
	{
		var collectionView = new CollectionView
		{
			AutomationId = "Issue33037ProgrammaticCollectionViewScroller",
			ItemsSource = CreateItems(),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(16, 12)
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		var scrollButton = new Button
		{
			AutomationId = "Issue33037ProgrammaticScrollButton",
			Text = "Scroll to item 50",
			ZIndex = 1
		};
		scrollButton.Clicked += (_, _) => collectionView.ScrollTo(50, position: ScrollToPosition.Start, animate: false);

		Grid.SetRow(collectionView, 1);

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				scrollButton,
				collectionView
			}
		};
	}
}

class Issue33037NonShellAppearingCollectionViewPage : Issue33037NonShellScenarioPage
{
	readonly CollectionView _collectionView;
	bool _scrolled;

	public Issue33037NonShellAppearingCollectionViewPage() : base("Issue33037 Appearing")
	{
		_collectionView = new CollectionView
		{
			AutomationId = "Issue33037AppearingCollectionViewScroller",
			ItemsSource = CreateItems(),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(16, 12)
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		Content = new Grid
		{
			Children =
			{
				_collectionView
			}
		};
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (_scrolled)
			return;

		_scrolled = true;
		await Task.Yield();
		_collectionView.ScrollTo(50, position: ScrollToPosition.Start, animate: false);
	}
}

class Issue33037NonShellHiddenNavigationBarPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellHiddenNavigationBarPage() : base("Issue33037 Hidden Navigation")
	{
		NavigationPage.SetHasNavigationBar(this, false);

		var topMarker = new Label
		{
			AutomationId = "Issue33037HiddenNavigationBarTopMarker",
			Text = "This content must remain below the status bar.",
			BackgroundColor = Colors.LightGreen,
			FontAttributes = FontAttributes.Bold,
			Padding = 12
		};

		var backButton = new Button
		{
			AutomationId = "Issue33037HiddenNavigationBarBackButton",
			Text = "Back"
		};
		backButton.Clicked += async (_, _) => await Navigation.PopAsync();

		var scrollView = new ScrollView
		{
			AutomationId = "Issue33037HiddenNavigationBarScroller",
			Content = CreateStackContent("Issue33037HiddenNavigationBar")
		};

		Grid.SetRow(backButton, 1);
		Grid.SetRow(scrollView, 2);

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				topMarker,
				backButton,
				scrollView
			}
		};
	}
}

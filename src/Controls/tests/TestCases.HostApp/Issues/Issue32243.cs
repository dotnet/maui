using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32243, "CollectionView does not disconnect handlers when DataTemplateSelector changes template", PlatformAffected.Android | PlatformAffected.UWP)]
public class Issue32243 : NavigationPage
{
	public Issue32243() : base(new _Issue32243MainPage())
	{
		_Issue32243TrackingLabel.ResetAll();
	}
}

class _Issue32243MainPage : ContentPage
{
	readonly Label _handlerCountLabel;
	readonly Label _statusLabel;

	public _Issue32243MainPage()
	{
		Title = "Issue 32243";

		_statusLabel = new Label
		{
			Text = "Navigate to the CollectionView page, switch templates, go back, then check handlers.",
			AutomationId = "StatusLabel"
		};

		_handlerCountLabel = new Label
		{
			Text = "Connected handlers will be shown here.",
			AutomationId = "HandlerCountLabel"
		};

		var navigateButton = new Button
		{
			Text = "Navigate to template page",
			AutomationId = "NavigateButton"
		};

		navigateButton.Clicked += async (sender, args) =>
		{
			await Navigation.PushAsync(new _Issue32243CollectionPage());
		};

		var checkHandlersButton = new Button
		{
			Text = "Show connected handlers",
			AutomationId = "CheckHandlersButton"
		};

		checkHandlersButton.Clicked += (sender, args) =>
		{
			var labelsWithHandlers = _Issue32243TrackingLabel.GetLabelsWithConnectedHandlers();

			if (labelsWithHandlers.Count == 0)
			{
				_handlerCountLabel.Text = "✓ No labels have connected handlers (all cleaned up!)";
				_handlerCountLabel.TextColor = Colors.Green;
				_statusLabel.Text = "Status: No connected handlers found.";
			}
			else
			{
				var details = string.Join("\n", labelsWithHandlers.Select(label =>
					$"Label #{label.InstanceId} - Text: '{label.Text}'"));

				_handlerCountLabel.Text = $"⚠️ {labelsWithHandlers.Count} labels still have connected handlers:\n{details}";
				_handlerCountLabel.TextColor = Colors.Brown;
				_statusLabel.Text = "Status: Connected handlers are still present.";
			}
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children =
			{
				new Label
				{
					Text = "This test mirrors the sandbox flow: navigate, switch templates, navigate back, then verify disconnected handlers."
				},
				navigateButton,
				checkHandlersButton,
				_statusLabel,
				_handlerCountLabel
			}
		};
	}
}

class _Issue32243CollectionPage : ContentPage
{
	readonly List<_Issue32243Item> _items;
	readonly CollectionView _collectionView;
	readonly Label _statusLabel;

	public _Issue32243CollectionPage()
	{
		Title = "Template Page";

		_items = Enumerable.Range(1, 50).Select(i => new _Issue32243Item
		{
			Name = $"Item {i}",
			UseTemplateA = i % 2 == 1
		}).ToList();

		var templateA = new DataTemplate(() =>
		{
			var label = new _Issue32243TrackingLabel();
			label.SetBinding(Label.TextProperty, nameof(_Issue32243Item.Name));
			return new VerticalStackLayout
			{
				BackgroundColor = Colors.LightBlue,
				Padding = new Thickness(10),
				Children =
				{
					label,
					new Label { Text = "Template A", TextColor = Colors.Blue }
				}
			};
		});

		var templateB = new DataTemplate(() =>
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, nameof(_Issue32243Item.Name));
			return new VerticalStackLayout
			{
				BackgroundColor = Colors.LightGray,
				Padding = new Thickness(10),
				Children =
				{
					label,
					new Label { Text = "Template B", TextColor = Colors.Gray }
				}
			};
		});

		_statusLabel = new Label
		{
			Text = "Status: Mixed templates active",
			AutomationId = "TemplatePageStatusLabel"
		};

		var switchTemplateButton = new Button
		{
			Text = "Switch to all Template B",
			AutomationId = "SwitchTemplateButton"
		};

		switchTemplateButton.Clicked += (sender, args) =>
		{
			foreach (var item in _items)
			{
				item.UseTemplateA = false;
			}

			_collectionView.ItemsSource = _items.ToList();
			_statusLabel.Text = "Status: All items using Template B";
		};

		var navigateBackButton = new Button
		{
			Text = "Navigate back",
			AutomationId = "NavigateBackButton"
		};

		navigateBackButton.Clicked += async (sender, args) =>
		{
			await Navigation.PopAsync();
		};

		_collectionView = new CollectionView
		{
			AutomationId = "ItemsCollectionView",
			SelectionMode = SelectionMode.None,
			ItemTemplate = new _Issue32243TemplateSelector
			{
				TemplateA = templateA,
				TemplateB = templateB
			},
			ItemsSource = _items
		};

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			},
			Children =
			{
				new VerticalStackLayout
				{
					Padding = new Thickness(20, 20, 20, 10),
					Spacing = 8,
					Children =
					{
						new Label { Text = "50 items — odd=Template A (blue), even=Template B (gray)" },
						switchTemplateButton,
						navigateBackButton,
						_statusLabel
					}
				},
				_collectionView
			}
		};

		Grid.SetRow(_collectionView, 1);
	}
}

class _Issue32243TrackingLabel : Label
{
	static int _instanceCounter;
	static readonly List<WeakReference<_Issue32243TrackingLabel>> _allInstances = [];
	readonly int _instanceId;

	public _Issue32243TrackingLabel()
	{
		_instanceId = ++_instanceCounter;
		_allInstances.Add(new WeakReference<_Issue32243TrackingLabel>(this));
		DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
	}

	protected override void OnHandlerChanged()
	{
		base.OnHandlerChanged();

		if (Handler is null)
		{
			DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
		}
	}

	public int InstanceId => _instanceId;

	public static List<_Issue32243TrackingLabel> GetLabelsWithConnectedHandlers()
	{
		var result = new List<_Issue32243TrackingLabel>();

		_allInstances.RemoveAll(wr =>
		{
			if (wr.TryGetTarget(out var label))
			{
				if (label.Handler != null)
				{
					result.Add(label);
				}

				return false;
			}

			return true;
		});

		return result;
	}

	void OnMainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
	{
		_ = _instanceId;
	}

	public static void ResetAll()
	{
		_allInstances.Clear();
		_instanceCounter = 0;
	}
}

class _Issue32243TemplateSelector : DataTemplateSelector
{
	public DataTemplate TemplateA { get; set; }
	public DataTemplate TemplateB { get; set; }

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		return item is _Issue32243Item { UseTemplateA: true } ? TemplateA : TemplateB;
	}
}

class _Issue32243Item : INotifyPropertyChanged
{
	string _name = string.Empty;
	bool _useTemplateA;

	public string Name
	{
		get => _name;
		set
		{
			_name = value;
			OnPropertyChanged();
		}
	}

	public bool UseTemplateA
	{
		get => _useTemplateA;
		set
		{
			_useTemplateA = value;
			OnPropertyChanged();
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

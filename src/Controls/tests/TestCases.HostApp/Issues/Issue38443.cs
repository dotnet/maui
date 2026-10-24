#nullable enable
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38443, "CollectionView SelectedItem set during Shell navigation does not update the visual state", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue38443 : TestShell
{
    const string DetailRoute = "Issue38443Detail";

    protected override void Init()
    {
        Routing.RegisterRoute(DetailRoute, typeof(Issue38443DetailPage));

        var navigateButton = new Button
        {
            AutomationId = "NavigateButton",
            Text = "Navigate to CollectionView"
        };
        navigateButton.Clicked += async (sender, args) => await GoToAsync(DetailRoute);

        Items.Add(new ShellContent
        {
            Title = "Issue 38443",
            Content = new ContentPage
            {
                Content = new Grid
                {
                    Children = { navigateButton }
                }
            }
        });
    }
}

public class Issue38443DetailPage : ContentPage, IQueryAttributable, INotifyPropertyChanged
{
    string? _selectedBoxColor;

    public Issue38443DetailPage()
    {
        Title = "Box Colors";
        BindingContext = this;

        var collectionView = new CollectionView
        {
            AutomationId = "ColorsCollectionView",
            HorizontalOptions = LayoutOptions.Center,
            ItemsSource = BoxColors,
            SelectionMode = SelectionMode.Single,
            VerticalOptions = LayoutOptions.Center,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                ItemSpacing = 5
            },
            ItemTemplate = new DataTemplate(() =>
            {
                var boxView = new BoxView
                {
                    HeightRequest = 40,
                    WidthRequest = 40
                };
                boxView.SetBinding(BoxView.ColorProperty, ".");

                var border = new Border
                {
                    Padding = 4,
                    Stroke = Colors.Transparent,
                    StrokeShape = new RoundRectangle { CornerRadius = 4 },
                    StrokeThickness = 3,
                    Content = boxView
                };

                VisualStateManager.SetVisualStateGroups(border,
                [
                    new VisualStateGroup
                    {
                        Name = "CommonStates",
                        States =
                        {
                            new VisualState { Name = "Normal" },
                            new VisualState
                            {
                                Name = "Selected",
                                Setters =
                                {
                                    new Setter { Property = Border.StrokeProperty, Value = Colors.Black }
                                }
                            }
                        }
                    }
                ]);

                return border;
            })
        };
        collectionView.SetBinding(SelectableItemsView.SelectedItemProperty, nameof(SelectedBoxColor));

        var instructionsLabel = new Label
        {
            HorizontalTextAlignment = TextAlignment.Center,
            AutomationId = "InstructionsLabel",
            Text = "Expected: the cyan item is selected when this page opens."
        };

        var selectedColorLabel = new Label
        {
            AutomationId = "SelectedColorLabel",
            HorizontalTextAlignment = TextAlignment.Center
        };
        selectedColorLabel.SetBinding(Label.TextProperty, new Binding(nameof(SelectedBoxColor), stringFormat: "SelectedItem: {0}"));

        var content = new Grid
        {
            Padding = 24,
            RowSpacing = 16,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };
        content.Add(instructionsLabel);
        content.Add(collectionView, row: 1);
        content.Add(selectedColorLabel, row: 2);

        Content = content;
    }

    public ObservableCollection<string> BoxColors { get; } =
    [
        "#113FFC", "#00A1CD", "#7c0e64", "#d2a023", "#ca1765", "#14e147"
    ];

    public string? SelectedBoxColor
    {
        get => _selectedBoxColor;
        set
        {
            if (_selectedBoxColor == value)
                return;

            _selectedBoxColor = value;
            OnPropertyChanged();
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        SelectedBoxColor = BoxColors[1];
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
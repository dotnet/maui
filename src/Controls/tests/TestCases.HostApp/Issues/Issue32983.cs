using System.Collections.ObjectModel;
#if IOS || MACCATALYST
using Microsoft.Maui.Platform;
using UIKit;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32983, "CollectionView messes up Measure operation on Views", PlatformAffected.iOS)]
public class Issue32983 : ContentPage
{
    readonly Label _measuredHeightLabel;
    readonly Label _statusLabel;
    readonly VerticalStackLayout _rootLayout;

    public Issue32983()
    {
        _measuredHeightLabel = new Label
        {
            AutomationId = "MeasuredHeight",
            Text = "Pending"
        };

        _statusLabel = new Label
        {
            AutomationId = "StatusLabel",
            Text = "Tap the button to reproduce the issue"
        };

        var showButton = new Button
        {
            Text = "ShowBottomSheet",
            AutomationId = "ShowBottomSheetButton",
            HorizontalOptions = LayoutOptions.Fill
        };
        showButton.Clicked += OnShowBottomSheetClicked;

        _rootLayout = new VerticalStackLayout
        {
            Padding = 10,
            Spacing = 8,
            Children =
            {
                showButton,
                _statusLabel,
                new Label { Text = "Measured height (ContentView unmounted):" },
                _measuredHeightLabel
            }
        };

        Content = _rootLayout;
    }

    void OnShowBottomSheetClicked(object sender, EventArgs e)
    {
#if IOS || MACCATALYST
        var bottomSheetContent = new Issue32983BottomSheetContentView();
        var vm = new Issue32983ViewModel();
        for (int i = 0; i < 10; i++)
        {
            vm.ListOfStuff.Add(new Issue32983Item("Item #" + (i + 1)));
        }
        bottomSheetContent.BindingContext = vm;

        var mauiContext = this.Handler?.MauiContext ?? throw new Exception("MauiContext is null");
        var parent = this.ToUIViewController(mauiContext);
        var viewControllerToPresent = bottomSheetContent.ToUIViewController(mauiContext);

        var display = DeviceDisplay.MainDisplayInfo;
        double widthDip = display.Width / display.Density;
        double heightDip = display.Height / display.Density;

        var minimumSize = bottomSheetContent.Measure(widthDip, heightDip);
        var detentSize = minimumSize.ToCGSize();

        if (OperatingSystem.IsIOSVersionAtLeast(16) || OperatingSystem.IsMacCatalystVersionAtLeast(16))
        {
            var detent = UISheetPresentationControllerDetent.Create(
                "ContentHeight",
                (ctx) => (nfloat)detentSize.Height);

            var sheet = viewControllerToPresent.SheetPresentationController;
            if (sheet is not null)
            {
                sheet.Detents = new[] { detent };
                sheet.LargestUndimmedDetentIdentifier = UISheetPresentationControllerDetentIdentifier.Unknown;
                sheet.PrefersScrollingExpandsWhenScrolledToEdge = false;
                sheet.PrefersEdgeAttachedInCompactHeight = true;
                sheet.WidthFollowsPreferredContentSizeWhenEdgeAttached = true;
            }
        }
        parent.PresentViewController(viewControllerToPresent, animated: true, completionHandler: null);
#endif
    }
}

public class Issue32983BottomSheetContentView : Microsoft.Maui.Controls.ContentView
{
    public Issue32983BottomSheetContentView()
    {
        BackgroundColor = Colors.AliceBlue;

        var collectionView = new CollectionView
        {
            SelectionMode = SelectionMode.Single,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(10),
            AutomationId = "BottomSheetCollectionView",

            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical),

            ItemTemplate = new DataTemplate(() =>
            {
                var label = new Label
                {
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold
                };

                label.SetBinding(Label.TextProperty, nameof(Issue32983Item.Name));
                label.SetBinding(Label.AutomationIdProperty, nameof(Issue32983Item.Name));

                return label;
            })
        };

        collectionView.SetBinding(ItemsView.ItemsSourceProperty, nameof(Issue32983ViewModel.ListOfStuff));

        Content = collectionView;
    }
}

public class Issue32983Item(string name)
{
    public string Name => name;
}

public class Issue32983ViewModel
{
    public ObservableCollection<Issue32983Item> ListOfStuff { get; } = new();
}

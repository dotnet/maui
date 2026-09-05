using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Shapes;
 
namespace Maui.Controls.Sample.Issues;
 
[Issue(IssueTracker.Github, 35492, "Border.StrokeDashArray leaks dashed Borders when using a shared Application resource", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue35492 : NavigationPage
{
    const string SharedDashArrayResourceKey = "SharedDashArray";
 
    public Issue35492() : base(new Issue35492MainPage())
    {
        if (Application.Current?.Resources is not null && !Application.Current.Resources.ContainsKey(SharedDashArrayResourceKey))
            Application.Current.Resources.Add(SharedDashArrayResourceKey, new DoubleCollection(new[] { 6d, 3d, 1d, 3d }));
    }
 
    internal static DoubleCollection GetSharedDashArray() =>
        Application.Current?.Resources?[SharedDashArrayResourceKey] as DoubleCollection;
}
 
sealed class Issue35492MainPage : ContentPage
{
    readonly Label _pageCountLabel;
    readonly Label _aliveCountLabel;
    readonly List<WeakReference> _trackedPages = new();
    int _pagesPushed;
 
    public Issue35492MainPage()
    {
        BackgroundColor = Colors.White;
 
        _pageCountLabel = new Label
        {
            Text = "Pages pushed: 0",
            AutomationId = "PageCountLabel"
        };
 
        _aliveCountLabel = new Label
        {
            Text = "Alive count: 0/0",
            AutomationId = "SummaryLabel"
        };
 
        var pushPageButton = new Button
        {
            Text = "Push Page",
            AutomationId = "PushPageButton"
        };
        pushPageButton.Clicked += OnPushPage;
 
        var forceGcButton = new Button
        {
            Text = "Force GC",
            AutomationId = "ForceGCButton"
        };
        forceGcButton.Clicked += OnForceGc;
 
        var collectionView = Issue35492CollectionPage.CreateCollectionView();
 
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(16),
                Spacing = 12,
                Children =
                {
                    new Label
                    {
                        Text = "Border StrokeDashArray Memory Leak Test",
                        FontSize = 22,
                        FontAttributes = FontAttributes.Bold,
                        AutomationId = "TitleLabel"
                    },
                    new Label
                    {
                        Text = "Repro for memory leak with CollectionView item template borders. Issue reproduces when template contains a Border with StrokeDashArray.",
                        AutomationId = "DescriptionLabel"
                    },
                    pushPageButton,
                    forceGcButton,
                    _pageCountLabel,
                    _aliveCountLabel,
                }
            }
        };
 
        UpdateAliveCount();
    }
 
    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateAliveCount();
    }
 
    async void OnPushPage(object sender, EventArgs e)
    {
        _pagesPushed++;
        _pageCountLabel.Text = $"Pages pushed: {_pagesPushed}";
 
        var page = new Issue35492CollectionPage();
        _trackedPages.Add(new WeakReference(page));
        UpdateAliveCount();
 
        await Navigation.PushAsync(page, false);
    }
 
    async void OnForceGc(object sender, EventArgs e)
    {
        try
        {
            await GarbageCollectionHelper.WaitForGC(10000, _trackedPages.ToArray());
        }
        catch
        {
            // Keep the count visible for UITest assertion when GC does not complete in time.
        }

        UpdateAliveCount();
    }
 
    void UpdateAliveCount()
    {
        var alive = 0;
        foreach (var pageRef in _trackedPages)
        {
            if (pageRef.IsAlive)
                alive++;
        }
 
        _aliveCountLabel.Text = $"Alive count: {alive}/{_trackedPages.Count}";
    }
}
 
sealed class Issue35492CollectionPage : ContentPage
{
    int _tapCount;
 
    public Issue35492CollectionPage()
    {
        Content = BuildCollectionView(attachPageHandler: true);
    }
 
    public static CollectionView CreateCollectionView()
    {
        return BuildCollectionView(attachPageHandler: false);
    }
 
    CollectionView BuildCollectionView(bool attachPageHandler)
        => BuildCollectionView(attachPageHandler, this);
 
    static CollectionView BuildCollectionView(bool attachPageHandler, Issue35492CollectionPage page = null)
    {
        return new CollectionView
        {
            AutomationId = "TestCollectionView",
            ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)
            {
                HorizontalItemSpacing = 8,
                VerticalItemSpacing = 8
            },
            ItemsSource = Enumerable.Range(1, 20).Select(i => $"Card {i}").ToArray(),
            ItemTemplate = new DataTemplate(() =>
            {
                var label = new Label
                {
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };
                label.SetBinding(Label.TextProperty, ".");
 
                var border = new Border
                {
                    Stroke = Colors.Black,
                    StrokeThickness = 1,
                    StrokeShape = new RoundRectangle { CornerRadius = 6 },
                    StrokeDashArray = Issue35492.GetSharedDashArray(),
                    Padding = new Thickness(12),
                    Content = label
                };
 
                if (attachPageHandler && page is not null)
                {
                    var tapGesture = new TapGestureRecognizer();
                    tapGesture.Tapped += page.OnBorderTapped;
                    border.GestureRecognizers.Add(tapGesture);
                }
 
                return border;
            })
        };
    }
 
    void OnBorderTapped(object sender, EventArgs e)
    {
        _tapCount++;
    }
}
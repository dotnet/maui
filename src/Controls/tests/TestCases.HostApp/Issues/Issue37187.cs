namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37187, "Shell flyout footer unsubscribes MeasureInvalidated when replaced", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue37187 : Shell
{
    readonly Label _statusLabel;
    MeasureProbeView _oldFooter;
    MeasureProbeView _currentFooter;

    public Issue37187()
    {
        _statusLabel = new Label
        {
            AutomationId = "FooterMeasureStatus",
            Text = "Not started",
        };

        var prepareButton = new Button
        {
            AutomationId = "PrepareFooters",
            Text = "Replace footer A with B",
        };
        prepareButton.Clicked += OnPrepareFootersClicked;

        var invalidateButton = new Button
        {
            AutomationId = "InvalidateOldFooter",
            Text = "Invalidate removed footer A",
        };
        invalidateButton.Clicked += OnInvalidateOldFooterClicked;

        Items.Add(new FlyoutItem
        {
            Title = "Footer test",
            Items =
            {
                new ShellContent
                {
                    Title = "Footer test",
                    Content = new ContentPage
                    {
                        Content = new VerticalStackLayout
                        {
                            Padding = 20,
                            Spacing = 12,
                            Children =
                            {
                                prepareButton,
                                invalidateButton,
                                _statusLabel,
                            },
                        },
                    },
                },
            },
        });
    }

    async void OnPrepareFootersClicked(object sender, EventArgs e)
    {
        FlyoutIsPresented = true;
        await Task.Delay(250);

        _oldFooter = new MeasureProbeView("Footer A");
        FlyoutFooter = _oldFooter;
        await Task.Delay(250);

        _currentFooter = new MeasureProbeView("Footer B");
        FlyoutFooter = _currentFooter;
        await Task.Delay(250);

        FlyoutIsPresented = false;
        await Task.Delay(250);

        _currentFooter.ResetMeasureCount();
        _statusLabel.Text = "Ready";
    }

    void OnInvalidateOldFooterClicked(object sender, EventArgs e)
    {
        if (_oldFooter is null || _currentFooter is null)
        {
            _statusLabel.Text = "Not prepared";
            return;
        }

        _oldFooter.InvalidateMeasure();
        _statusLabel.Text = $"Current footer measure count: {_currentFooter.MeasureCount}";
    }

    sealed class MeasureProbeView : ContentView
    {
        public MeasureProbeView(string text)
        {
            Content = new Label { Text = text };
        }

        public int MeasureCount { get; private set; }

        public void ResetMeasureCount() => MeasureCount = 0;

        protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
        {
            MeasureCount++;
            return base.MeasureOverride(widthConstraint, heightConstraint);
        }
    }
}
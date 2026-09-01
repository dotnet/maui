using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37635, "BackButtonBehavior.Command retains discarded behaviors", PlatformAffected.All)]
public class Issue37635 : Shell
{
    public Issue37635()
    {
        Items.Add(new ShellContent
        {
            Title = "Issue 37635",
            ContentTemplate = new DataTemplate(() => new Issue37635Page())
        });
    }
}

file class Issue37635Page : ContentPage
{
    const int CohortSize = 30;
    readonly Issue37635Command _sharedCommand = new();
    readonly Label _resultLabel;

    public Issue37635Page()
    {
        Title = "BackButtonBehavior leak";

        _resultLabel = new Label
        {
            AutomationId = "ResultLabel",
            Text = "Run the test to check retained payloads."
        };

        var runButton = new Button
        {
            AutomationId = "RunTestButton",
            Text = "Run test"
        };
        runButton.Clicked += OnRunTestClicked;

        Content = new VerticalStackLayout
        {
            Padding = 24,
            Spacing = 16,
            Children =
            {
                new Label
                {
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 22,
                    Text = "BackButtonBehavior.Command memory test"
                },
                new Label
                {
                    Text = "Creates discarded behaviors with 1 MB BindingContext payloads. The shared command must not retain them."
                },
                runButton,
                _resultLabel
            }
        };
    }

    async void OnRunTestClicked(object sender, EventArgs e)
    {
        _resultLabel.Text = "Collecting...";

        WeakReference[] control = CreateCohort(null);
        WeakReference[] command = CreateCohort(_sharedCommand);

        try
        {
            await GarbageCollectionHelper.WaitForGC(5000, control.Concat(command).ToArray());
        }
        catch
        {
            // Report retained references so the UI test fails with the observed counts.
        }

        int controlAlive = control.Count(reference => reference.IsAlive);
        int commandAlive = command.Count(reference => reference.IsAlive);
        _resultLabel.Text = $"Control: {controlAlive}/{CohortSize}; Command: {commandAlive}/{CohortSize}";
        GC.KeepAlive(_sharedCommand);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference[] CreateCohort(ICommand command)
    {
        var references = new WeakReference[CohortSize];

        for (int index = 0; index < CohortSize; index++)
        {
            references[index] = CreatePayloadReference(command);
        }

        return references;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference CreatePayloadReference(ICommand command)
    {
        var payload = new Issue37635Payload();
        var behavior = new BackButtonBehavior
        {
            BindingContext = payload,
            Command = command
        };

        return new WeakReference(payload);
    }
}

file sealed class Issue37635Payload
{
    readonly byte[] _bytes = new byte[1024 * 1024];
}

file sealed class Issue37635Command : ICommand
{
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter) => true;

    public void Execute(object parameter)
    {
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
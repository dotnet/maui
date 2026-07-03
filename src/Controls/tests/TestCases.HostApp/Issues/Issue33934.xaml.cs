#nullable enable

using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33934, "[iOS] TranslateToAsync causes spurious SizeChanged events after animation completion", PlatformAffected.iOS)]
public partial class Issue33934 : ContentPage
{
    /// <summary>
    /// Stores the iteration count from the last opened dialog for test verification.
    /// </summary>
    public static int LastDialogIterationCount { get; set; }

    public Issue33934()
    {
        InitializeComponent();
    }

    async void OnShowDialogClicked(object? sender, EventArgs e)
    {
        var vm = new DialogViewModel();
        var view = new Issue33934DialogPage { BindingContext = vm };

        // CRITICAL: Set iOS modal presentation style (matches ViewPresenter behavior)
        view.SetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.ModalPresentationStyleProperty, UIModalPresentationStyle.OverFullScreen);

        await Navigation.PushModalAsync(view, animated: false);
        await vm.WaitForCloseAsync();

        if (Navigation.ModalStack.LastOrDefault() == view)
        {
            await Navigation.PopModalAsync(animated: false);
        }

        // Store iteration count for test verification
        LastDialogIterationCount = view.IterationCount;

        // Android workaround: fixes touch responsiveness issue after background/foreground cycle
        await Task.Yield();
    }
}

public class DialogViewModel : ViewModelBase
{
    public DialogViewModel()
    {
        // Create 2 rows with 3 actions each (keep it small enough for BottomSheet)
        QuickActionRows.Add(new ActionRowModel
        {
            AvailableQuickActions = new ObservableCollection<ActionModel>
            {
                new ActionModel { Title = "Time", Icon = "⏱️" },
                new ActionModel { Title = "Absence", Icon = "🏖️" },
                new ActionModel { Title = "Expense", Icon = "💰" }
            }
        });

        QuickActionRows.Add(new ActionRowModel
        {
            AvailableQuickActions = new ObservableCollection<ActionModel>
            {
                new ActionModel { Title = "Travel", Icon = "✈️" },
                new ActionModel { Title = "Invoice", Icon = "📄" },
                new ActionModel { Title = "Chat", Icon = "💬" }
            }
        });
    }

    public ObservableCollection<ActionRowModel> QuickActionRows { get; } = new();
}

public class ActionRowModel
{
    public ObservableCollection<ActionModel> AvailableQuickActions { get; set; } = new();
}

public class ActionModel
{
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;

    public Command SelectCommand => new Command(() =>
    {
        // Just close the dialog when tapped
        System.Diagnostics.Debug.WriteLine($"Action tapped: {Title}");
    });
}

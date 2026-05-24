using Microsoft.Maui.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Maui.Controls.Sample.Issues;

/// <summary>
/// Interface for ViewModels that need async lifecycle events
/// </summary>
public interface ISupportAsyncAppearingEvents
{
    Task WillAppearAsync(CancellationToken cancellationToken);
    Task DidDisappearAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Simplified ViewPage for dialog reproduction - only includes essentials
/// </summary>
public class Issue33934ViewPage : ContentPage
{
    protected Issue33934ViewPage()
    {
        NavigationPage.SetHasNavigationBar(this, false);
    }

    /// <summary>
    /// Lifecycle event when page is appearing
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (this.BindingContext is ISupportAsyncAppearingEvents target)
        {
            this.RunInBackground(target.WillAppearAsync);
        }
    }

    /// <summary>
    /// Lifecycle event when page is disappearing
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (this.BindingContext is ISupportAsyncAppearingEvents target)
        {
            this.RunInBackground(target.DidDisappearAsync);
        }
    }

    /// <summary>
    /// Runs a background task tied to the lifetime of this view
    /// </summary>
    protected void RunInBackground(Func<CancellationToken, Task> runFn, BackgroundTaskLifetime lifetime = BackgroundTaskLifetime.Disposal, CancellationToken cancellationToken = default)
    {
        // Simple fire-and-forget implementation for testing
        _ = Task.Run(async () =>
        {
            try
            {
                await runFn(cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [RunInBackground] EXCEPTION: {ex}");
                Console.WriteLine($"❌ [RunInBackground] EXCEPTION: {ex}");
                throw; // Re-throw so we see it crash
            }
        }, cancellationToken);
    }
}

public enum BackgroundTaskLifetime
{
    /// <summary>
    /// Task runs until the view is disposed
    /// </summary>
    Disposal,

    /// <summary>
    /// Task runs until the view disappears
    /// </summary>
    Disappearing
}

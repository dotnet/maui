#nullable enable

using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

/// <summary>
///     The view for the <see cref="Issue33934DialogPage" />.
/// </summary>
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class Issue33934DialogPage : Issue33934BottomSheetDialog
{
    /// <summary>
    ///     Creates a new instance of <see cref="Issue33934DialogPage" />.
    /// </summary>
    public Issue33934DialogPage()
    {
        this.InitializeComponent();
    }

    private void OnIterationCountLabelClicked(object? sender, EventArgs e)
    {
        // When button is clicked, update its text with the final iteration count
        if (sender is Button button)
        {
            button.Text = $"Animation Iterations: {this.IterationCount}";
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the animation exceeded 2 iterations (indicating spurious SizeChanged events).
    /// </summary>
    public bool HasExcessiveIterations => this.IterationCount > 2;
}

public class Issue33934BottomSheetDialog : Issue33934DialogBase
{
    /// <summary>
    ///     The bindable property definition for the <see cref="FullScreenThreshold" /> property.
    /// </summary>
    public static readonly BindableProperty FullScreenThresholdProperty = BindableProperty.Create(nameof(FullScreenThreshold), typeof(double), typeof(Issue33934BottomSheetDialog), 0.66);

    /// <summary>
    ///     The bindable property definition for the <see cref="EnableTransition" /> property.
    /// </summary>
    public static readonly BindableProperty EnableTransitionProperty = BindableProperty.Create(nameof(EnableTransition), typeof(bool), typeof(Issue33934BottomSheetDialog), true);

    /// <summary>
    ///     The bindable property definition for the <see cref="IterationCount" /> property.
    /// </summary>
    public static readonly BindableProperty IterationCountProperty = BindableProperty.Create(nameof(IterationCount), typeof(int), typeof(Issue33934BottomSheetDialog), 0);

    Size targetSize;
    bool hasStartedTransition;

    /// <summary>
    ///     Gets or sets the threshold (0.0-1.0) at which content height triggers full-screen mode.
    ///     Default is 0.66 (66% of available height).
    /// </summary>
    public double FullScreenThreshold
    {
        get => (double)this.GetValue(FullScreenThresholdProperty);
        set => this.SetValue(FullScreenThresholdProperty, value);
    }

    /// <summary>
    ///     Gets or sets whether the slide-up transition animation is enabled.
    ///     Default is true.
    /// </summary>
    public bool EnableTransition
    {
        get => (bool)this.GetValue(EnableTransitionProperty);
        set => this.SetValue(EnableTransitionProperty, value);
    }

    /// <summary>
    ///     Gets the number of animation iterations (for detecting spurious SizeChanged events).
    /// </summary>
    public int IterationCount
    {
        get => (int)this.GetValue(IterationCountProperty);
        private set => this.SetValue(IterationCountProperty, value);
    }

    /// <inheritdoc />
    protected override void OnDialogContentChanged(View? oldValue, View? newValue)
    {
        base.OnDialogContentChanged(oldValue, newValue);

        if (newValue is not null)
        {
            // Always use layout for bottom sheet positioning, even without transition
            var layout = new BottomSheetLayout { Child = newValue };

            if (!this.EnableTransition)
            {
                // Mark as already transitioned so it doesn't position offscreen
                layout.IsTransitionedIn = true;
            }

            this.Content = layout;
        }
    }

    /// <inheritdoc />
    protected override void OnSizeAllocated(double width, double height)
    {
        this.targetSize = new Size(width, height);

        if (this.EnableTransition && this.Content is TransitionLayout && !this.hasStartedTransition)
        {
            this.hasStartedTransition = true;
            base.OnSizeAllocated(width, height);
            this.RunInBackground(this.TransitionInAsync);
        }
        else
        {
            base.OnSizeAllocated(width, height);
        }
    }

    /// <inheritdoc />
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        // Propagate binding context to content
        View? content = this.DialogContent;
        if (content is not null)
        {
            SetInheritedBindingContext(content, this.BindingContext);
        }
    }

    async Task TransitionInAsync(CancellationToken cancellationToken = default)
    {
        System.Diagnostics.Debug.WriteLine($"[TransitionInAsync] {DateTime.Now:HH:mm:ss.fff} - STARTING");

        if (this.Content is not TransitionLayout layout || layout.Child is null)
        {
            System.Diagnostics.Debug.WriteLine($"[TransitionInAsync] {DateTime.Now:HH:mm:ss.fff} - ABORTED: No layout or child");
            return;
        }

        View child = layout.Child;
        int iterationCount = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            iterationCount++;
            System.Diagnostics.Debug.WriteLine($"════ [BottomSheet] ITERATION #{iterationCount} ════");
            Console.WriteLine($"[BottomSheet] ITERATION #{iterationCount}");

            // Update bindable property to track iterations (for UI test verification)
            this.IterationCount = iterationCount;

            CancellationTokenSource? restartCts = new CancellationTokenSource();

            void sizeChangedHandler(object? sender, EventArgs e)
            {
                var st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.Debug.WriteLine($"╔═══ [BottomSheet-SizeChanged] ═══");
                System.Diagnostics.Debug.WriteLine($"║ {child.Width}x{child.Height}, TY:{child.TranslationY}");
                System.Diagnostics.Debug.WriteLine($"║ STACK:");
                System.Diagnostics.Debug.WriteLine(st.ToString());
                System.Diagnostics.Debug.WriteLine($"╚══════════════════════");
                Console.WriteLine($"[BottomSheet] SizeChanged!");
                try
                { restartCts?.Cancel(); }
                catch (ObjectDisposedException) { }
            }

            void bindingContextChangedHandler(object? sender, EventArgs e)
            {
                var st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.Debug.WriteLine($"╔═══ [BottomSheet-BindingContextChanged] ═══");
                System.Diagnostics.Debug.WriteLine($"║ STACK:");
                System.Diagnostics.Debug.WriteLine(st.ToString());
                System.Diagnostics.Debug.WriteLine($"╚══════════════════════");
                Console.WriteLine($"[BottomSheet] BindingContextChanged!");
                try
                { restartCts?.Cancel(); }
                catch (ObjectDisposedException) { }
            }

            child.SizeChanged += sizeChangedHandler;
            child.BindingContextChanged += bindingContextChangedHandler;

            try
            {
                // Yield to let bindings resolve and layout happen
                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested();

                // Cancel any ongoing animations before starting new one
                child.CancelAnimations();

                double currentHeight = child.Height;

                // If no valid height yet, wait for size/binding change
                if (currentHeight <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[BottomSheet-TransitionInAsync] ⏳ Waiting 2s for valid height...");
                    Console.WriteLine($"[BottomSheet] ⏳ Waiting for valid height...");
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, restartCts.Token);
                    await Task.Delay(2000, linkedCts.Token);
                }
                else
                {
                    // Check if content is too large for bottom sheet
                    if (layout is BottomSheetLayout bottomSheetLayout && currentHeight > this.Height * this.FullScreenThreshold)
                    {
                        // Remove child from old parent first
                        bottomSheetLayout.Child = null;

                        // Switch to full screen layout
                        var fullScreenLayout = new FullScreenLayout { Child = child };
                        this.Content = fullScreenLayout;
                        layout = fullScreenLayout;

                        // Restart to handle new layout
                        child.SizeChanged -= sizeChangedHandler;
                        child.BindingContextChanged -= bindingContextChangedHandler;
                        restartCts?.Dispose();
                        continue;
                    }

                    // Ensure positioned offscreen, then animate in
                    System.Diagnostics.Debug.WriteLine($"[BottomSheet-TransitionInAsync] 🎬 Animation START: {currentHeight} → 0");
                    Console.WriteLine($"[BottomSheet] 🎬 Animation START");
                    child.TranslationY = currentHeight;

                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, restartCts.Token);
                    await child.TranslateToAsync(0, 0, 2000, Easing.SinOut).WaitAsync(linkedCts.Token);

                    System.Diagnostics.Debug.WriteLine($"[BottomSheet-TransitionInAsync] ✅ Animation COMPLETE!");
                    Console.WriteLine($"[BottomSheet] ✅ COMPLETE!");

                    // Mark as transitioned so future layouts don't reset position
                    layout.IsTransitionedIn = true;
                }

                // Animation completed successfully
                System.Diagnostics.Debug.WriteLine($"[BottomSheet-TransitionInAsync] 🎉 EXITING loop!");
                Console.WriteLine($"[BottomSheet] 🎉 EXIT loop");

                // Update UI to show final iteration count
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (this.FindByName("IterationCountLabel") is Label label)
                    {
                        label.Text = $"Animation Iterations: {this.IterationCount}";
                        label.IsVisible = true;
                    }
                });

                break;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                System.Diagnostics.Debug.WriteLine($"🔄 [BottomSheet] RESTARTING!");
                Console.WriteLine($"🔄 [BottomSheet] RESTART!");

                // Size or binding context changed during animation, restart
                child.SizeChanged -= sizeChangedHandler;
                child.BindingContextChanged -= bindingContextChangedHandler;
                restartCts?.Dispose();
                restartCts = null;
            }
            finally
            {
                child.SizeChanged -= sizeChangedHandler;
                child.BindingContextChanged -= bindingContextChangedHandler;
                restartCts?.Dispose();
                restartCts = null;
            }
        }
    }
}


#nullable enable

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Maui.Controls.Sample.Issues;

/// <summary>
///     A base page for dialog templates.
/// </summary>
[ContentProperty(nameof(DialogContent))]
public abstract class Issue33934DialogBase : Issue33934ViewPage
{
    /// <summary>
    ///     The bindable property definition for the <see cref="IsClosable" /> property.
    /// </summary>
    public static readonly BindableProperty IsClosableProperty = BindableProperty.Create(nameof(IsClosable), typeof(bool), typeof(Issue33934DialogBase), false, propertyChanged: (b, o, v) => ((Issue33934DialogBase)b).OnIsClosableChanged((bool)o, (bool)v));

    /// <summary>
    ///     The bindable property definition for the <see cref="DialogContent" /> property.
    /// </summary>
    public static readonly BindableProperty DialogContentProperty = BindableProperty.Create(nameof(DialogContent), typeof(View), typeof(Issue33934DialogBase), propertyChanged: (b, o, v) => ((Issue33934DialogBase)b).OnDialogContentChanged((View?)o, (View?)v));

    readonly Lock backgroundTasksLock = new();
    readonly List<BackgroundTask> backgroundTasks = new();
    bool canRunBackgroundTasks = true;

    /// <summary>
    ///     Creates a new instance of <see cref="Issue33934DialogBase" />.
    /// </summary>
    protected Issue33934DialogBase()
    {
        this.Loaded += this.OnLoaded;
    }


    /// <summary>
    ///     Gets or sets whether the dialog can be closed by the user.
    /// </summary>
    public bool IsClosable
    {
        get => (bool)this.GetValue(IsClosableProperty);
        set => this.SetValue(IsClosableProperty, value);
    }

    /// <summary>
    ///     Gets or sets the content of the dialog.
    /// </summary>
    public View? DialogContent
    {
        get => (View?)this.GetValue(DialogContentProperty);
        set => this.SetValue(DialogContentProperty, value);
    }

    /// <summary>
    ///     Run a background task tied to the lifetime of the dialog.
    /// </summary>
    /// <param name="runFn"></param>
    /// <param name="cancellationToken"></param>
    public void RunInBackground(Func<CancellationToken, Task> runFn, CancellationToken cancellationToken = default)
    {
        lock (this.backgroundTasksLock)
        {
            CancellationTokenSource cts = cancellationToken != CancellationToken.None
                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
                : new CancellationTokenSource();

            if (!this.canRunBackgroundTasks)
            {
                cts.Cancel();
            }

            Task task = runFn(cts.Token);

            task.ContinueWith(onTaskCompleted, TaskScheduler.FromCurrentSynchronizationContext());

            this.backgroundTasks.Add(new BackgroundTask(task, cts));
        }

        return;

        void onTaskCompleted(Task t)
        {
            this.RemoveCompletedTask(t);
            if (t.IsFaulted)
            {
                throw t.Exception!;
            }
        }
    }

    /// <summary>
    ///     Called when <see cref="DialogContent" /> property has changed.
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    protected virtual void OnDialogContentChanged(View? oldValue, View? newValue) { }

    /// <summary>
    ///     Called when <see cref="IsClosable" /> property has changed.
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    protected virtual void OnIsClosableChanged(bool oldValue, bool newValue) { }

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        this.StopAndCancelBackgroundTasks();
        base.OnDisappearing();
    }

    void OnLoaded(object? sender, EventArgs e) { }

    void RemoveCompletedTask(Task completedTask)
    {
        lock (this.backgroundTasksLock)
        {
            int index = this.backgroundTasks.FindIndex(t => ReferenceEquals(t.Task, completedTask));
            if (index >= 0)
            {
                BackgroundTask bgTask = this.backgroundTasks[index];
                this.backgroundTasks.RemoveAt(index);
                bgTask.Cts.Dispose();
            }
        }
    }

    void StopAndCancelBackgroundTasks()
    {
        List<BackgroundTask> tasksToCancel;

        lock (this.backgroundTasksLock)
        {
            this.canRunBackgroundTasks = false;
            tasksToCancel = new List<BackgroundTask>(this.backgroundTasks);
            this.backgroundTasks.Clear();
        }

        foreach (BackgroundTask bgTask in tasksToCancel)
        {
            try
            {
                bgTask.Cts.Cancel();
            }
            catch (ObjectDisposedException) { }
        }

        foreach (BackgroundTask bgTask in tasksToCancel)
        {
            try
            {
                bgTask.Task.Wait(TimeSpan.FromSeconds(2));
            }
            catch (AggregateException) { }
            finally
            {
                bgTask.Cts.Dispose();
            }
        }
    }

    readonly struct BackgroundTask(Task task, CancellationTokenSource cts)
    {
        public readonly Task Task = task;
        public readonly CancellationTokenSource Cts = cts;
    }

    /// <summary>
    ///     Base class for dialog layouts that support transitions.
    /// </summary>
    protected abstract class TransitionLayout : Layout
    {
        View? child;
        bool isTransitionedIn;

        public View? Child
        {
            get => this.child;
            set
            {
                if (this.child is not null)
                {
                    this.Remove(this.child);
                }
                this.child = value;
                if (this.child is not null)
                {
                    this.Add(this.child);
                }
            }
        }

        public bool IsTransitionedIn
        {
            get => this.isTransitionedIn;
            set => this.isTransitionedIn = value;
        }
    }

    /// <summary>
    ///     Layout that positions child at the bottom, sized to its content.
    /// </summary>
    protected class BottomSheetLayout : TransitionLayout
    {
        protected override ILayoutManager CreateLayoutManager()
        {
            return new BottomSheetLayoutManager(this);
        }

        class BottomSheetLayoutManager : ILayoutManager
        {
            readonly BottomSheetLayout layout;
            Rect lastBounds;
            Rect lastChildBounds;
            Size lastChildSize;

            public BottomSheetLayoutManager(BottomSheetLayout layout)
            {
                this.layout = layout;
            }

            public Size Measure(double widthConstraint, double heightConstraint)
            {
                if (this.layout.Child is null)
                {
                    return Size.Zero;
                }

                var childSize = this.layout.Child.Measure(widthConstraint, heightConstraint);
                return childSize;
            }

            public Size ArrangeChildren(Rect bounds)
            {
                if (this.layout.Child is null)
                {
                    return Size.Zero;
                }

                var childSize = this.layout.Child.DesiredSize;

                var childBounds = new Rect(
                    bounds.X,
                    bounds.Bottom - childSize.Height,
                    bounds.Width,
                    childSize.Height
                );

                // Track changes
                bool boundsChanged = bounds != this.lastBounds;
                bool childSizeChanged = childSize != this.lastChildSize;
                bool childBoundsChanged = childBounds != this.lastChildBounds;

                if (boundsChanged || childSizeChanged || childBoundsChanged)
                {
                    System.Diagnostics.Debug.WriteLine($"[BottomSheetLayout.ArrangeChildren] CHANGE DETECTED:");
                    System.Diagnostics.Debug.WriteLine($"  Received bounds: {bounds.Width}x{bounds.Height} @ ({bounds.X},{bounds.Y})");
                    System.Diagnostics.Debug.WriteLine($"  Child DesiredSize: {childSize.Width}x{childSize.Height}");
                    System.Diagnostics.Debug.WriteLine($"  Forwarding to child: {childBounds.Width}x{childBounds.Height} @ ({childBounds.X},{childBounds.Y})");
                    System.Diagnostics.Debug.WriteLine($"  Child.TranslationY: {this.layout.Child.TranslationY}");
                    System.Diagnostics.Debug.WriteLine($"  IsTransitionedIn: {this.layout.IsTransitionedIn}");

                    if (boundsChanged)
                        System.Diagnostics.Debug.WriteLine($"  → Bounds changed: {this.lastBounds} → {bounds}");
                    if (childSizeChanged)
                        System.Diagnostics.Debug.WriteLine($"  → Child size changed: {this.lastChildSize} → {childSize}");
                    if (childBoundsChanged)
                        System.Diagnostics.Debug.WriteLine($"  → Child bounds changed: {this.lastChildBounds} → {childBounds}");

                    Console.WriteLine($"[BottomSheetLayout] ArrangeChildren - bounds: {bounds.Width}x{bounds.Height}, child: {childSize.Width}x{childSize.Height}");

                    this.lastBounds = bounds;
                    this.lastChildSize = childSize;
                    this.lastChildBounds = childBounds;
                }

                this.layout.Child.Arrange(childBounds);

                if (!this.layout.IsTransitionedIn && this.layout.Child.TranslationY == 0)
                {
                    this.layout.Child.TranslationY = childSize.Height;
                }

                return childSize;
            }
        }
    }

    /// <summary>
    ///     Layout that fills the entire screen with content.
    /// </summary>
    protected class FullScreenLayout : TransitionLayout
    {
        protected override ILayoutManager CreateLayoutManager()
        {
            return new FullScreenLayoutManager(this);
        }

        class FullScreenLayoutManager : ILayoutManager
        {
            readonly FullScreenLayout layout;

            public FullScreenLayoutManager(FullScreenLayout layout)
            {
                this.layout = layout;
            }

            public Size Measure(double widthConstraint, double heightConstraint)
            {
                if (this.layout.Child is null)
                {
                    return Size.Zero;
                }

                System.Diagnostics.Debug.WriteLine($"[FullScreenLayout.Measure] {DateTime.Now:HH:mm:ss.fff} - Measuring child with constraints: {widthConstraint}x{heightConstraint}");
                this.layout.Child.Measure(widthConstraint, heightConstraint);
                return new Size(widthConstraint, heightConstraint);
            }

            public Size ArrangeChildren(Rect bounds)
            {
                if (this.layout.Child is null)
                {
                    return Size.Zero;
                }

                this.layout.Child.Arrange(bounds);

                if (!this.layout.IsTransitionedIn && this.layout.Child.TranslationY == 0)
                {
                    this.layout.Child.TranslationY = bounds.Height;
                }

                return bounds.Size;
            }
        }
    }
}

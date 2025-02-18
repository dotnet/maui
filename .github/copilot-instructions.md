# GitHub Copilot Instructions

## Memory Leaks

### Circular references on iOS and Catalyst

Even since the early days of [Xamarin.iOS][xamarin.ios], there has existed an
issue with "circular references" even in a garbage-collected runtime like .NET.
C# objects co-exist with a reference-counted world on Apple platforms, and so a
C# object that subclasses `NSObject` can run into situations where they can
accidentally live forever -- a memory leak. Note that this situation does not
occur on Android or Windows platforms.

Take for example, the following circular reference:

```csharp
class MyViewSubclass : UIView
{
    public UIView? Parent { get; set; }

    public void Add(MyViewSubclass subview)
    {
        subview.Parent = this;
        AddSubview(subview);
    }
}

//...

var parent = new MyViewSubclass();
var view = new MyViewSubclass();
parent.Add(view);
```

In this case:

* `parent` -> `view` via `Subviews`
* `view` -> `parent` via the `Parent` property
* The reference count of both objects is non-zero
* Both objects live forever

This problem isn't limited to a field or property, you can create similar
situations with C# events:

```csharp
class MyView : UIView
{
    public MyView()
    {
        var picker = new UIDatePicker();
        AddSubview(picker);
        picker.ValueChanged += OnValueChanged;
    }

    void OnValueChanged(object? sender, EventArgs e) { }

    // Use this instead and it doesn't leak!
    //static void OnValueChanged(object? sender, EventArgs e) { }
}
```

In this case:

* `MyView` -> `UIDatePicker` via `Subviews`
* `UIDatePicker` -> `MyView` via `ValueChanged` and `EventHandler.Target`
* Both objects live forever

A solution for this example, is to make `OnValueChanged` method `static`, which
would result in a `null` `Target` on the `EventHandler` instance.

Another solution, would be to put `OnValueChanged` in a non-`NSObject` subclass:

```csharp
class MyView : UIView
{
    readonly Proxy _proxy = new();

    public MyView()
    {
        var picker = new UIDatePicker();
        AddSubview(picker);
        picker.ValueChanged += _proxy.OnValueChanged;
    }

    class Proxy
    {
        public void OnValueChanged(object? sender, EventArgs e) { }
    }
}
```

If the class subscribing to the events are not an `NSObject` subclass, we can
also use a proxy, but use weak references to the primary object. A typical
example is with the view handlers which maps a virtual view to a platform view.

The handler will have a readonly field for the proxy, and the proxy will manage the
events between the platform view and the virtual view. The proxy should not have
a reference to the handler as the handler does not take part in the events.

* The handler has a strong reference to the proxy
* The platform view has a strong reference to the proxy via the event handler
* The proxy has a _weak_ reference to the virtual view

```csharp
class DatePickerHandler : ViewHandler<IDatePicker, UIDatePicker>
{
    readonly Proxy proxy = new();

    protected override void ConnectHandler(UIDatePicker platformView)
    {
        proxy.Connect(VirtualView, picker);

        base.ConnectHandler(platformView);
    }

    protected override void DisconnectHandler(UIDatePicker platformView)
    {
        proxy.Disconnect(VirtualView, picker);

        base.DisconnectHandler(platformView);
    }

    void OnValueChanged() { }
    
    class Proxy
    {
        WeakReference<IDatePicker>? _virtualView;

        IDatePicker? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

        public void Connect(IDatePicker handler, UIDatePicker platformView)
        {
            _virtualView = new(virtualView);

            platformView.ValueChanged += OnValueChanged;
        }

        public void Disconnect(UIDatePicker platformView)
        {
            _virtualView = null;

            platformView.ValueChanged -= OValueChanged;
        }

        public void OnValueChanged(object? sender, EventArgs e)
        {
            VirtualView?.OnValueChanged();
        }
    }
}
```

This is the pattern we've used in most .NET MAUI handlers and other `UIView`
subclasses.

See the [MemoryLeaksOniOS][iosleaks] sample repo, if you would like to play with
some of these scenarios in isolation in an iOS application without .NET MAUI.

[xamarin.ios]: https://stackoverflow.com/questions/13058521/is-this-a-bug-in-monotouch-gc/13059140#13059140
[iosleaks]: https://github.com/jonathanpeppers/MemoryLeaksOniOS

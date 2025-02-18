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

This is the pattern we've used in most .NET MAUI handlers and other `UIView`
subclasses.

See the [MemoryLeaksOniOS][iosleaks] sample repo, if you would like to play with
some of these scenarios in isolation in an iOS application without .NET MAUI.

[xamarin.ios]: https://stackoverflow.com/questions/13058521/is-this-a-bug-in-monotouch-gc/13059140#13059140
[iosleaks]: https://github.com/jonathanpeppers/MemoryLeaksOniOS

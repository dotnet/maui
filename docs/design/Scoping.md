.NET MAUI Scopes
===

# Introduction

.NET MAUI currently uses the [CreateScope](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.serviceproviderserviceextensions.createscope?view=dotnet-plat-ext-6.0) feature of MS.Ext.DI to create services that are scoped to the .NET MAUI concept of a window (AppCompatActivity, UIScene, UI.XAML.Window). 

We also have an internal scoping mechanism that we use to scope aspects of the platform application with a lifecycle that isn't tied to a Window. 

## Window Scoping

When a new Window is created we take the Application level `IServiceProvider` and generate a [new scope](https://github.com/dotnet/maui/blob/c1525fad1af4ff32aa7abe00119be94f7399d98f/src/Core/src/Platform/Android/ApplicationExtensions.cs#L38).

This allows us to register things that need to be scoped to a window (`IDispatcher`, `ITicket`, `IAnimationManager`)

## Blazor Scoping

??? 
 
## Custom internal scoping

### (`WrappedServiceProvider`)[https://github.com/dotnet/maui/blob/b09979d93ac544d4b0dd9fd0df5c25da7e91392c/src/Core/src/MauiContext.cs#L47]

```csharp
class WrappedServiceProvider : IServiceProvider
```

The purpose of this class is to intercept `GetServices` requests and return anything that we've registered as scoped to this particular `WrappedServiceProvider`. The primary reason for us needing to do this is that the Ms.Ext.DI `ServiceProvider` implementation only allows you to create a single level of scoping but we need a mechanism to create types that are scoped to particular areas of a window.

#### Modal Page scoping
We use this feature for `ModalPages`. Because `ModalPages` are basically a "Virtual Window" we use the `WrappedServiceProvider` to create sets of types that need to be scoped to this new Virtual Window instead of the actual window. For example things like the toolbar or top level container the user sees are all scoped to the "Virtual Window".

#### Android Fragment scoping
This scoping feature is very useful within Android because with (Android)[https://github.com/dotnet/maui/blob/8d8be50f4f94457b30551d7ffb020b6e5bb48915/src/Core/src/Platform/Android/MauiContextExtensions.cs#L51] you will get areas of your View that have to use features of a parent view opposed to those same features on the window. Think of it like needing different scopes for each context that exists within a window.

- Window Scope (this is for views created at the window level)
- Tab Scope (this is for views that are created inside each Tab.)
- Navigation Scope (this is for views that are created inside the Navigation)


#### Limitations
Any services registered with the `WrappedServiceProvider` don't participate in the resolution used by the Ms.Ext.DI.  The way the `WrappedServiceProvider` works is that it checks an internal dictionary for a requested `Type`. If it doesn't find that type then it passes the request to whatever it's wrapped. So on and so forth until it reaches window scoped `IServiceProvider`

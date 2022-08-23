using System;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Platform;

public static class ApplicationExtensions
{

	public static void CreatePlatformWindow(this Gtk.Application platformApplication, IApplication application, IPersistedState? state) =>
		platformApplication.CreatePlatformWindow(application, new OpenWindowRequest(state));

	public static void CreatePlatformWindow(this Gtk.Application platformApplication, IApplication application, OpenWindowRequest? args)
	{
		if (application.Handler?.MauiContext is not { } applicationContext)
			return;

		var mainWindow = new MauiGtkMainWindow();
		platformApplication.AddWindow(mainWindow);

		var mauiContext = applicationContext!.MakeWindowScope(mainWindow, out var windowScope);

		applicationContext.Services.InvokeLifecycleEvents<GtkLifecycle.OnMauiContextCreated>(del => del(mauiContext));

		var activationState = args?.State is not null ? new ActivationState(mauiContext, args.State) : new ActivationState(mauiContext, args?.State ?? new PersistedState());

		var window = application.CreateWindow(activationState);

		mainWindow.SetWindowHandler(window, mauiContext);

		applicationContext.Services.InvokeLifecycleEvents<GtkLifecycle.OnCreated>(del => del(mainWindow, EventArgs.Empty));

		mainWindow.QueueDraw();
		mainWindow.ShowAll();

		mainWindow.Present();
	}

}
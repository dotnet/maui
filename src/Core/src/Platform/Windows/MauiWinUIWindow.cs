using System;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui
{
	public class MauiWinUIWindow : UI.Xaml.Window
	{
		public MauiWinUIWindow()
		{
			Activated += OnActivated;
			Closed += OnClosed;
			VisibilityChanged += OnVisibilityChanged;

			if (!Application.Current.Resources.ContainsKey("MauiRootContainerStyle"))
			{
				var mauiComboBoxDictionary = new ResourceDictionary
				{
					Source = new Uri("ms-appx:///Microsoft.Maui/Platform/Windows/Styles/MauiComboBoxStyle.xbf")
				};
				Application.Current.Resources.MergedDictionaries.Add(mauiComboBoxDictionary);

				var mauiTextBoxDictionary = new ResourceDictionary
				{
					Source = new Uri("ms-appx:///Microsoft.Maui/Platform/Windows/Styles/MauiTextBoxStyle.xbf")
				};
				Application.Current.Resources.MergedDictionaries.Add(mauiTextBoxDictionary);
			}
		}

		protected virtual void OnActivated(object sender, UI.Xaml.WindowActivatedEventArgs args)
		{
			MauiWinUIApplication.Current.Services?.InvokeLifecycleEvents<WindowsLifecycle.OnActivated>(del => del(this, args));
		}

		protected virtual void OnClosed(object sender, UI.Xaml.WindowEventArgs args)
		{
			MauiWinUIApplication.Current.Services?.InvokeLifecycleEvents<WindowsLifecycle.OnClosed>(del => del(this, args));
		}

		protected virtual void OnVisibilityChanged(object sender, UI.Xaml.WindowVisibilityChangedEventArgs args)
		{
			MauiWinUIApplication.Current.Services?.InvokeLifecycleEvents<WindowsLifecycle.OnVisibilityChanged>(del => del(this, args));
		}
	}
}
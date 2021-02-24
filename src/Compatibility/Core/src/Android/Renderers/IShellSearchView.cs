using System;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IShellSearchView : IDisposable
	{
		AView View { get; }

		SearchHandler SearchHandler { get; set; }

		void LoadView();

		event EventHandler SearchConfirmed;

		bool ShowKeyboardOnAttached { get; set; }
	}
}
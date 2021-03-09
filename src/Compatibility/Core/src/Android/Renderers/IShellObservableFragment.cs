using System;
using AndroidX.Fragment.App;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IShellObservableFragment
	{
		Fragment Fragment { get; }

		event EventHandler AnimationFinished;
	}
}
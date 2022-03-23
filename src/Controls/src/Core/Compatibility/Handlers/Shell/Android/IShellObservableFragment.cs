using System;
using AndroidX.Fragment.App;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellObservableFragment
	{
		Fragment Fragment { get; }

		event EventHandler AnimationFinished;
	}
}
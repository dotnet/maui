using System;
using AndroidX.Fragment.App;

namespace Microsoft.Maui.Controls.Platform
{
	public interface IShellObservableFragment
	{
		Fragment Fragment { get; }

		event EventHandler AnimationFinished;
	}
}
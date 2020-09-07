using AndroidX.Fragment.App;
using System;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellObservableFragment
	{
		Fragment Fragment { get; }

		event EventHandler AnimationFinished;
	}
}
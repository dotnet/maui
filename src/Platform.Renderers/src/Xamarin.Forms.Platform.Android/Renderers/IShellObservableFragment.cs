using System;
using AndroidX.Fragment.App;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellObservableFragment
	{
		Fragment Fragment { get; }

		event EventHandler AnimationFinished;
	}
}
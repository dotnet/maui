using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Xamarin.Forms.DualScreen
{
	internal interface IDualScreenService : IDisposable
	{
		event EventHandler OnScreenChanged;
		bool IsSpanned { get; }
		bool IsLandscape { get; }
		Rectangle GetHinge();
		Point? GetLocationOnScreen(VisualElement visualElement);
	}
}

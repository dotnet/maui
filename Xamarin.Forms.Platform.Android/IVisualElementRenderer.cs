using System;
using Android.Views;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public interface IVisualElementRenderer : IRegisterable, IDisposable
	{
		VisualElement Element { get; }

		VisualElementTracker Tracker { get; }

		ViewGroup ViewGroup { get; }

		event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint);

		void SetElement(VisualElement element);

		void SetLabelFor(int? id);

		void UpdateLayout();
	}
}
using System;
using System.ComponentModel;
using Android.Views;
using AndroidX.AppCompat.View.Menu;
using AndroidX.Core.View;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility
{
	public class HandlerToRendererShim : IVisualElementRenderer
	{
		int? _defaultLabelFor;

		public HandlerToRendererShim(IViewHandler vh)
		{
			ViewHandler = vh;
		}

		IViewHandler ViewHandler { get; }

		public VisualElement Element { get; private set; }

		public VisualElementTracker Tracker { get; private set; }

		public ViewGroup ViewGroup => ViewHandler.NativeView as ViewGroup;

		public global::Android.Views.View View => ViewHandler.NativeView as global::Android.Views.View;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public void Dispose()
		{
			ViewHandler.DisconnectHandler();
		}

		public SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			return ViewHandler.GetDesiredSize(widthConstraint, heightConstraint);
		}

		public void SetElement(VisualElement element)
		{
			var oldElement = Element;
			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (element != null)
			{
				element.PropertyChanged += OnElementPropertyChanged;
			}

			Element = element;

			ViewHandler.SetVirtualView((IView)element);
			((IView)element).Handler = ViewHandler;

			if (Tracker == null)
			{
				Tracker = new VisualElementTracker(this);
			}

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(oldElement, Element));
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
		}

		public void SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
			{
				_defaultLabelFor = ViewCompat.GetLabelFor(View);
			}

			ViewCompat.SetLabelFor(View, (int)(id ?? _defaultLabelFor));
		}

		public void UpdateLayout()
		{
			Tracker?.UpdateLayout();
		}
	}
}

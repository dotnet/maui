using System;
using System.ComponentModel;
using Android.Views;
using AndroidX.AppCompat.View.Menu;
using AndroidX.Core.View;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Compatibility
{
	public class HandlerToRendererShim : IVisualElementRenderer
	{
		int? _defaultLabelFor;

		public HandlerToRendererShim(IPlatformViewHandler vh)
		{
			Compatibility.Hosting.MauiAppBuilderExtensions.CheckForCompatibility();
			ViewHandler = vh;
		}

		IPlatformViewHandler ViewHandler { get; }

		public VisualElement Element { get; private set; }

		public VisualElementTracker Tracker { get; private set; }

		public ViewGroup ViewGroup => null;

		public global::Android.Views.View View => ViewHandler.ContainerView ?? ViewHandler.PlatformView;

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
			if (element == Element)
				return;

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

			((IView)element).Handler = ViewHandler;

			if (ViewHandler.VirtualView != element)
				ViewHandler.SetVirtualView((IView)element);

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
#pragma warning disable CS0618 // Obsolete
				_defaultLabelFor = ViewCompat.GetLabelFor(View);
#pragma warning restore CS0618 // Obsolete
			}

#pragma warning disable CS0618 // Obsolete
			ViewCompat.SetLabelFor(View, (int)(id ?? _defaultLabelFor));
#pragma warning restore CS0618 // Obsolete
		}

		public void UpdateLayout()
		{
			Tracker?.UpdateLayout();
		}
	}
}

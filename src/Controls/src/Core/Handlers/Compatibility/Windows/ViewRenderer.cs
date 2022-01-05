#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class ViewRenderer<TElement, TNativeView> : ViewHandler<TElement, TNativeView>, IDisposable
		where TElement : Element, IView
		where TNativeView : FrameworkElement
	{

		// The casts here are to get around the fact that if you access VirtualView
		// before it's been initialized you'll get an exception
		public TElement? Element => ((IElementHandler)this).VirtualView as TElement;
		public TNativeView? Control => ((IElementHandler)this).NativeView as TNativeView;
		public FrameworkElement? Parent => Control?.Parent as FrameworkElement;
		bool _initialElementChangeCalled;
		public ViewRenderer(IPropertyMapper mapper) : base(ViewHandler.ViewMapper)
		{
		}

		protected override TNativeView CreateNativeView()
		{
			var oldElement = Element;
			OnElementChanged(new ElementChangedEventArgs<TElement>(oldElement, Element));
			_initialElementChangeCalled = true;
			return NativeView;
		}

		protected void SetNativeControl(TNativeView control)
		{
			// TODO MAUI: LOG ISSUE
			if (Control != null && control != Control)
				throw new InvalidOperationException("Changing the NativeView is currently not supported");

			base.NativeView = control;
		}

		private protected override void OnConnectHandler(FrameworkElement nativeView)
		{
			base.OnConnectHandler(nativeView);
		}
		private protected override void OnDisconnectHandler(FrameworkElement nativeView)
		{
			base.OnDisconnectHandler(nativeView);
			Dispose();
		}

		public override void SetVirtualView(IView view)
		{
			var oldElement = Element;
			base.SetVirtualView(view);

			if (oldElement != null || !_initialElementChangeCalled)
				OnElementChanged(new ElementChangedEventArgs<TElement>(oldElement, Element));
		}

		protected virtual Size MinimumSize()
		{
			return new Size();
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{

		}


		public override void UpdateValue(string property)
		{
			base.UpdateValue(property);
			OnElementPropertyChanged(VirtualView, new PropertyChangedEventArgs(property));
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{

		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var size = base.GetDesiredSize(widthConstraint, heightConstraint);
			var minimumSize = MinimumSize();

			if (size.Height < minimumSize.Height || size.Width < minimumSize.Width)
			{
				return new Size(Math.Max(size.Width, minimumSize.Width), Math.Max(size.Height, minimumSize.Height));
			}

			return size;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{

		}
	}
}
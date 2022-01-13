#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using AView = Android.Views.View;
using IViewParent = Android.Views.IViewParent;
using PlatformView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public partial class ViewRenderer<TElement, TNativeView> : ViewHandler<TElement, TNativeView>
		where TElement : Element, IView
		where TNativeView : PlatformView
	{

		TNativeView? _nativeView;
		// The casts here are to get around the fact that if you access VirtualView
		// before it's been initialized you'll get an exception
		public TElement? Element => ((IElementHandler)this).VirtualView as TElement;
		public TNativeView? Control => ((IElementHandler)this).NativeView as TNativeView;

		public IViewParent? Parent => Control?.Parent;

		public ViewRenderer(IPropertyMapper mapper) : base(ViewHandler.ViewMapper)
		{
		}

		protected override TNativeView CreateNativeView()
		{
			var nativeView = _nativeView;
			_nativeView = null;
			return nativeView ?? throw new NotImplementedException();
		}

		protected virtual TNativeView CreateNativeControl() => CreateNativeView();

		protected void SetNativeControl(TNativeView control)
		{			
			if(NativeView != null && control != null)
			{
				throw new NotImplementedException("Changing the NativeView is currently not supported");
			}

			_nativeView = control;
		}

		private protected override void OnConnectHandler(AView nativeView)
		{
			base.OnConnectHandler(nativeView);
			nativeView.ViewAttachedToWindow += OnViewAttachedToWindow;
			nativeView.ViewDetachedFromWindow += OnViewDetatchedFromWindow;
		}
		private protected override void OnDisconnectHandler(AView nativeView)
		{
			base.OnDisconnectHandler(nativeView);
			nativeView.ViewAttachedToWindow -= OnViewAttachedToWindow;
			nativeView.ViewDetachedFromWindow -= OnViewDetatchedFromWindow;
		}

		public override void SetVirtualView(IView view)
		{
			var oldElement = Element;
			base.SetVirtualView(view);
			OnElementChanged(new ElementChangedEventArgs<TElement>(oldElement, Element));
		}

		void OnViewDetatchedFromWindow(object? sender, AView.ViewDetachedFromWindowEventArgs e) => OnDetachedFromWindow();

		void OnViewAttachedToWindow(object? sender, AView.ViewAttachedToWindowEventArgs e) => OnAttachedToWindow();



		protected virtual void OnAttachedToWindow()
		{

		}

		protected virtual void OnDetachedFromWindow()
		{

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
			var size =  base.GetDesiredSize(widthConstraint, heightConstraint);
			var minimumSize = MinimumSize();

			if(size.Height < minimumSize.Height || size.Width < minimumSize.Width)
			{
				return new Size(Math.Max(size.Width, minimumSize.Width), Math.Max(size.Height, minimumSize.Height));
			}

			return size;
		}
	}
}

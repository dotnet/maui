using System;
using Microsoft.Maui.Graphics;
#if IOS || MACCATALYST
using PlatformView = UIKit.UIView;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{

	abstract class ViewHandlerProxy : IViewHandler
	{
		public bool HasContainer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public object? ContainerView => throw new NotImplementedException();

		public IView? VirtualView
		{
			get;
			private set;
		}

		public object? PlatformView
		{
			get;
			private set;
		}

		public IMauiContext? MauiContext
		{
			get;
			private set;
		}

		IElement? IElementHandler.VirtualView => VirtualView;

		public void DisconnectHandler()
		{
			throw new NotImplementedException();
		}

		public Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			throw new NotImplementedException();
		}

		public void Invoke(string command, object? args = null)
		{
			throw new NotImplementedException();
		}

		public void PlatformArrange(Rect frame)
		{
			throw new NotImplementedException();
		}

		public void SetMauiContext(IMauiContext mauiContext)
		{
			throw new NotImplementedException();
		}

		public void SetVirtualView(IElement view)
		{
			VirtualView = (IView)view;
		}

		public void UpdateValue(string property)
		{
			throw new NotImplementedException();
		}

		public virtual void ConnectHandler(PlatformView platformView)
		{
		}

		public virtual void DisconnectHandler(PlatformView platformView)
		{
		}

		public PlatformView CreatePlatformView()
		{
			if (PlatformView is not null)
				throw new InvalidOperationException("PlatformView already created");

			PlatformView = CreatePlatformViewCore();
			return (PlatformView)PlatformView;
		}

		protected abstract PlatformView CreatePlatformViewCore();
	}

	abstract class ViewHandlerProxy<TVirtualView, TPlatformView> : ViewHandlerProxy
		where TVirtualView : class, IView
#if !(NETSTANDARD || !PLATFORM) || IOS || ANDROID || WINDOWS || TIZEN
		where TPlatformView : PlatformView
#else
		where TPlatformView : class
#endif
	{
	}
}
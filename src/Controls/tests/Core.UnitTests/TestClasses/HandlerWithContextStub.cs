using System;
using Controls.Core.UnitTests.TestClasses;
using Microsoft.Maui;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Handlers;
namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class HandlerWithAnimationContext : ViewHandler<IView, object>
	{
		class TestContext : IMauiContext
		{
			public IServiceProvider Services => throw new NotImplementedException();

			public IMauiHandlersServiceProvider Handlers => throw new NotImplementedException();
			IAnimationManager manager = new TestAnimationManager
			{
				AutoStartTicker = false,
			};
			public IAnimationManager AnimationManager
			{
				get => manager;
				set => manager = value;
			}
		}


		public bool IsDisconnected { get; private set; }
		public int ConnectHandlerCount { get; set; } = 0;
		public int DisconnectHandlerCount { get; set; } = 0;
		static IMauiContext MainContext = new TestContext();

		public IAnimationManager AnimationManager
		{
			get => this.MauiContext?.AnimationManager ?? MainContext.AnimationManager;
			set => SetMauiContext(new TestContext { AnimationManager = value });
		}

		public HandlerWithAnimationContext() : base(new PropertyMapper<IView>())
		{
			this.SetMauiContext(MainContext);
		}

		public HandlerWithAnimationContext(PropertyMapper mapper) : base(mapper)
		{
		}

		protected override object CreateNativeView()
		{
			return new object();
		}

		protected override void ConnectHandler(object nativeView)
		{
			base.ConnectHandler(nativeView);
			ConnectHandlerCount++;
		}

		protected override void DisconnectHandler(object nativeView)
		{
			base.DisconnectHandler(nativeView);
			DisconnectHandlerCount++;
		}
	}
}
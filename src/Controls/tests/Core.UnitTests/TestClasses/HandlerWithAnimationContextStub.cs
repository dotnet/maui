using System;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class HandlerWithAnimationContextStub : ViewHandler<IView, object>
	{
		class TestContext : IMauiContext
		{
			public TestContext(IAnimationManager manager = null)
			{
				AnimationManager = manager ?? new TestAnimationManager();
			}

			public IServiceProvider Services => throw new NotImplementedException();

			public IMauiHandlersServiceProvider Handlers => throw new NotImplementedException();

			public IAnimationManager AnimationManager { get; }
		}

		public bool IsDisconnected { get; private set; }

		public int ConnectHandlerCount { get; set; } = 0;

		public int DisconnectHandlerCount { get; set; } = 0;

		public IAnimationManager AnimationManager => ((TestContext)MauiContext).AnimationManager;

		public HandlerWithAnimationContextStub()
			: base(new PropertyMapper<IView>())
		{
			SetMauiContext(new TestContext());
		}

		public HandlerWithAnimationContextStub(IAnimationManager manager)
			: base(new PropertyMapper<IView>())
		{
			SetMauiContext(new TestContext(manager));
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
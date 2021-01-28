using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Platform.Handlers.UnitTests
{
	public class HandlerStub : AbstractViewHandler<Forms.Button, object>
	{
		public bool IsDisconnected { get; private set; }
		public int ConnectHandlerCount { get; set; } = 0;
		public int DisconnectHandlerCount { get; set; } = 0;

		public HandlerStub() : base(new PropertyMapper<IView>())
		{
		}

		public HandlerStub(PropertyMapper mapper) : base(mapper)
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

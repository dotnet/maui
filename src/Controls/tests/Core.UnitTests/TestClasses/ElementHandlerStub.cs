using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ElementHandlerStub : ElementHandler
	{
		public ElementHandlerStub() : base(ElementHandler.ElementMapper)
		{
		}

		private protected override void OnConnectHandler(object platformView)
		{
		}

		private protected override object OnCreatePlatformElement()
		{
			return new object();
		}

		private protected override void OnDisconnectHandler(object platformView)
		{
		}
	}
}

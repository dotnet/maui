using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.UnitTests
{
	class ButtonHandlerStub : ViewHandler<IButton, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();
		public ButtonHandlerStub() : base(new PropertyMapper<IButton, ButtonHandlerStub>())
		{
		}
	}
}

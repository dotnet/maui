using Microsoft.Maui.Handlers;
using System;

namespace Microsoft.Maui.UnitTests
{
	class ButtonHandlerStub : ViewHandler<IButton, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();
		public ButtonHandlerStub() : base(new PropertyMapper<IButton, ButtonHandlerStub>())
		{
		}
	}
}

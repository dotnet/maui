using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class ButtonHandlerStub : ViewHandler<IButton, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();
		public ButtonHandlerStub() : base(new PropertyMapper<IButton, ButtonHandlerStub>())
		{
		}
	}
}
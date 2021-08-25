using Microsoft.Maui.Handlers;
using System;

namespace Microsoft.Maui.UnitTests
{
	class AlternateButtonHandlerStub : ViewHandler<IButton, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();
		public AlternateButtonHandlerStub() : base(new PropertyMapper<IButton, AlternateButtonHandlerStub>())
		{
		}
	}
}

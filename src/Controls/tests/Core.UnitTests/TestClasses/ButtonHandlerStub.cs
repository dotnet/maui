// Licensed to the .NET Foundation under one or more agreements.
// Licensed under the MIT License.

using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class ButtonHandlerStub : ViewHandler<IButton, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();
		public ButtonHandlerStub() : base(new PropertyMapper<IButton, ButtonHandlerStub>())
		{
		}
	}
}
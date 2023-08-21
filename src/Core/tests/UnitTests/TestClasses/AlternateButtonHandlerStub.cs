// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.UnitTests
{
	class AlternateButtonHandlerStub : ViewHandler<IButton, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();
		public AlternateButtonHandlerStub() : base(new PropertyMapper<IButton, AlternateButtonHandlerStub>())
		{
		}
	}
}

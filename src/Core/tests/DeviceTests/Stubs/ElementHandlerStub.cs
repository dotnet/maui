// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public class ElementHandlerStub : ElementHandler<ElementStub, object>
	{
		public ElementHandlerStub() : base(ElementHandler.ElementMapper)
		{

		}

		protected override object CreatePlatformElement() => new Object();
	}
}

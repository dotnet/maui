// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class ButtonWithContainerStub : ButtonStub
	{
	}


	public class ButtonWithContainerStubHandler : ButtonHandler
	{
		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
#if ANDROID
			ContainerView = new WrapperView(Context);
#else
			ContainerView = new WrapperView();
#endif
		}
	}
}

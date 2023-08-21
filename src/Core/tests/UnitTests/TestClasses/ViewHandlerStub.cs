// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Handlers;
namespace Microsoft.Maui.UnitTests
{
	class ViewHandlerStub : ViewHandler<IViewStub, PlatformViewStub>
	{
		public static PropertyMapper<IViewStub, ViewHandlerStub> MockViewMapper = new PropertyMapper<IViewStub, ViewHandlerStub>(ViewHandler.ViewMapper)
		{

		};

		public ViewHandlerStub() : base(MockViewMapper)
		{

		}

		public ViewHandlerStub(PropertyMapper mapper = null) : base(mapper ?? MockViewMapper)
		{

		}

		protected override PlatformViewStub CreatePlatformView() => new PlatformViewStub();
	}

}

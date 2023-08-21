// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class ApplicationHandlerStub : ElementHandler<IApplication, object>
	{
		public static IPropertyMapper<IApplication, ApplicationHandlerStub> Mapper = new PropertyMapper<IApplication, ApplicationHandlerStub>(ElementMapper)
		{
		};
		public ApplicationHandlerStub()
			: base(Mapper)
		{
		}


		protected override object CreatePlatformElement() => default(object);
	}
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class RefreshViewStub : StubBase, IRefreshView
	{
		public bool IsRefreshing { get; set; }

		public Paint RefreshColor { get; set; }

		public IView Content { get; set; }
	}
}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class ActivityIndicatorStub : StubBase, IActivityIndicator
	{
		public bool IsRunning { get; set; }

		public Color Color { get; set; }
	}
}
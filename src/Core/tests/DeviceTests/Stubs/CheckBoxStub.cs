// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class CheckBoxStub : StubBase, ICheckBox
	{
		public bool IsChecked { get; set; }
		public Paint Foreground { get; set; }
	}
}
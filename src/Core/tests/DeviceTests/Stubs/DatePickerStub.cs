// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class DatePickerStub : StubBase, IDatePicker
	{
		public string Format { get; set; }

		public DateTime Date { get; set; }

		public DateTime MinimumDate { get; set; }

		public DateTime MaximumDate { get; set; }

		public double CharacterSpacing { get; set; }

		public Font Font { get; set; }

		public Color TextColor { get; set; }
	}
}
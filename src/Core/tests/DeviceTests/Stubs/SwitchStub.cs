// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class SwitchStub : StubBase, ISwitch
	{
		public Action IsOnDelegate;
		Color _thumbColor;
		Color _trackColor;
		bool _isOn;

		public bool IsOn
		{
			get => _isOn;
			set
			{
				SetProperty(ref _isOn, value);
				IsOnDelegate?.Invoke();
			}
		}

		public Color TrackColor
		{
			get => _trackColor;
			set => SetProperty(ref _trackColor, value);
		}

		public Color ThumbColor
		{
			get => _thumbColor;
			set => SetProperty(ref _thumbColor, value);
		}
	}
}
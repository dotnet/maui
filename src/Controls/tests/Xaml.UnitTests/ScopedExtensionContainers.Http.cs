// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml.UnitTests.ScopedExtensions;

namespace httpExtensions;

public static class HttpExtensions
{
	extension(Label label)
	{
		public string HttpTag
		{
			set => ThreeWayRecorder.Record(label, value);
		}
	}
}

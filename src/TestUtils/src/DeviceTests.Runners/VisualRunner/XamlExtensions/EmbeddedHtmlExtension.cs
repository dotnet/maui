// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System;
using System.IO;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	class EmbeddedHtmlExtension : EmbeddedResourceExtension
	{
		public override object? ProvideValue(IServiceProvider serviceProvider)
		{
			if (base.ProvideValue(serviceProvider) is Stream stream)
			{
				using var reader = new StreamReader(stream, leaveOpen: false);
				return new HtmlWebViewSource { Html = reader.ReadToEnd() };
			}

			return null;
		}
	}
}
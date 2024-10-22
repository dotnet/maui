#nullable enable
using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	[AcceptEmptyServiceProvider]
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
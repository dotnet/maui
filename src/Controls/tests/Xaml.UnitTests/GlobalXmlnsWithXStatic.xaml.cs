using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "Microsoft.Maui.Controls.Xaml.UnitTests.NSGlobalXmlnsWithXStatic")]

namespace Microsoft.Maui.Controls.Xaml.UnitTests.NSGlobalXmlnsWithXStatic
{
	public class MockxStatic
	{
		public static string MockStaticProperty { get { return "Property"; } }
		public const string MockConstant = "MConstant";
		public static string MockField = "Field";
		public static string MockFieldRef = Icons.CLOSE;
		public string InstanceProperty { get { return "InstanceProperty"; } }
		public static readonly Color BackgroundColor = Colors.Fuchsia;

		public class Nested
		{
			public static string Foo = "FOO";
		}
	}
}

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class GlobalXmlnsWithXStatic : ContentPage
	{
		public GlobalXmlnsWithXStatic() => InitializeComponent();


		public class Tests : IDisposable
		{
			public Tests()
			{
				Application.SetCurrentApplication(new MockApplication());
				DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			}

			public void Dispose()
			{
				AppInfo.SetCurrent(null);
				DispatcherProvider.SetCurrent(null);
				Application.SetCurrentApplication(null);
			}

			[Theory]
			[Values]
			public void XStaticWithAggregatedXmlns(XamlInflator inflator)
			{
				if (inflator == XamlInflator.XamlC)
					MockCompiler.Compile(typeof(GlobalXmlnsWithXStatic));

				var page = new GlobalXmlnsWithXStatic(inflator);
				Assert.Equal("MConstant", page.label0.Text);
			}
		}

	}
}
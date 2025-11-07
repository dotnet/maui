using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17484 : ContentPage
{
	public Maui17484() => InitializeComponent();

	public class Test
	{
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => AppInfo.SetCurrent(new MockAppInfo());

		[Theory]
		[Values]
		public void ObsoleteinDT(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				// TODO: XUnit has no DoesNotThrow. Remove this or use try/catch if needed: // (() => MockCompiler.Compile(typeof(Maui17484)));
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui17484));
				Assert.Empty(result.Diagnostics);
			}
			else if (inflator == XamlInflator.Runtime)
			{
				// TODO: XUnit has no DoesNotThrow. Remove this or use try/catch if needed: // (() => new Maui17484(inflator));
			}
			else
			// TODO: Convert to [Theory(Skip="reason")] or use conditional Skip attribute
			{
				// TODO: This branch was using NUnit Assert.Skip, needs proper handling
			}

		}
	}
}

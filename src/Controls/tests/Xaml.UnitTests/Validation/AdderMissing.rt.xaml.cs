using System.Linq;

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class CollectionWithoutAdder : View, IEnumerable
{
	private readonly List<string> _items = new();
	
	public IEnumerator GetEnumerator() => _items.GetEnumerator();
	
	// Intentionally no Add() method
}

public partial class AdderMissing : ContentPage
{
	public AdderMissing() => InitializeComponent();

	[TestFixture]
class Tests
	{
		[Test]
		public void ThrowsOnMissingAddMethod([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(7, 3), () => MockCompiler.Compile(typeof(AdderMissing)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(7, 3), () => new AdderMissing(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
using System.Collections;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class CollectionWithoutAdder : View, IEnumerable
{
	private readonly List<string> _items = new();
	
	public IEnumerator GetEnumerator() => _items.GetEnumerator();
}

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class AdderMissing : ContentPage
{
	public AdderMissing() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(AdderMissing));
				Assert.That(result.Diagnostics, Is.Not.Empty);
				Assert.That(result.Diagnostics.Any(d => d.Id == "MAUIX2008"), Is.True);
			}
			else
				Assert.Ignore($"Unknown inflator {inflator}");
		}
	}
}

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class StyleSheet : ContentPage
	{
		public StyleSheet()
		{
			InitializeComponent();
		}

		public StyleSheet(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void SetUp()
			{
				Device.PlatformServices = new MockPlatformServices();
				Xamarin.Forms.Internals.Registrar.RegisterAll(new Type[0]);
			}

			[TestCase(false), TestCase(true)]
			public void EmbeddedStyleSheetsAreLoaded(bool useCompiledXaml)
			{
				var layout = new StyleSheet(useCompiledXaml);
				Assert.That(layout.Resources.StyleSheets[0].Styles.Count, Is.GreaterThanOrEqualTo(1));
			}

			[TestCase(false), TestCase(true)]
			public void StyleSheetsAreApplied(bool useCompiledXaml)
			{
				var layout = new StyleSheet(useCompiledXaml);
				Assert.That(layout.label0.TextColor, Is.EqualTo(Color.Azure));
				Assert.That(layout.label0.BackgroundColor, Is.EqualTo(Color.AliceBlue));
			}

			[TestCase(false), TestCase(true)]
			public void StyleSheetSourceIsApplied(bool useCompiledXaml)
			{
				// Having a custom ResourceProvider forces the LoadFromXaml code path 
				// for both XamlC and non-XamlC
				Xamarin.Forms.Internals.ResourceLoader.ResourceProvider = GetResource;

				var layout = new StyleSheet(useCompiledXaml);

				Assert.AreEqual("css/foo.css", layout.Resources.StyleSheets[0].Source.OriginalString);

				// Reset state to its initial
				Xamarin.Forms.Internals.ResourceLoader.ResourceProvider = null;
				DesignMode.IsDesignModeEnabled = false;
			}

			string GetResource(AssemblyName name, string path)
			{
				var assembly = Assembly.Load(name);
				var resourceId = assembly
					.GetCustomAttributes<XamlResourceIdAttribute>()
					.Where(x => x.Path == path)
					.Select(x => x.ResourceId)
					.FirstOrDefault();

				if (!string.IsNullOrEmpty(resourceId))
				{
					using (var stream = assembly.GetManifestResourceStream(resourceId))
					{
						if (stream != null)
							using (var reader = new StreamReader(stream))
								return reader.ReadToEnd();
					}
				}

				return null;
			}
		}
	}
}
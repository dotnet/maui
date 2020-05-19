using NUnit.Framework;
using System.Maui.Core.UnitTests;

namespace System.Maui.Xaml.UnitTests
{
	public partial class ButtonCornerRadius : ContentPage
	{
		public ButtonCornerRadius()
		{
			InitializeComponent();
		}

		public ButtonCornerRadius(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(false)]
			[TestCase(true)]
			public void EscapedStringsAreTreatedAsLiterals (bool useCompiledXaml)
			{
				var layout = new ButtonCornerRadius(useCompiledXaml);
				Assert.AreEqual(0, layout.Button0.CornerRadius);
			}
		}
	}
}

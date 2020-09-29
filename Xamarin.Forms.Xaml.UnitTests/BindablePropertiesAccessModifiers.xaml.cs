using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class AccessModifiersControl : View
	{
		public static BindableProperty PublicFooProperty = BindableProperty.Create("PublicFoo",
			typeof(string),
			typeof(AccessModifiersControl),
			"");

		public string PublicFoo
		{
			get => (string)GetValue(PublicFooProperty);
			set => SetValue(PublicFooProperty, value);
		}

		internal static BindableProperty InternalBarProperty = BindableProperty.Create("InternalBar",
			typeof(string),
			typeof(AccessModifiersControl),
			"");

		public string InternalBar
		{
			get => (string)GetValue(InternalBarProperty);
			set => SetValue(InternalBarProperty, value);
		}
	}

	public partial class BindablePropertiesAccessModifiers : ContentPage
	{
		class Data
		{
			public string Foo => "Foo";
			public string Bar => "Bar";
		}

		public BindablePropertiesAccessModifiers()
		{
			InitializeComponent();
		}

		public BindablePropertiesAccessModifiers(bool useCompiledXaml)
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

			[TestCase(true)]
			[TestCase(false)]
			public void BindProperties(bool useCompiledXaml)
			{
				var page = new BindablePropertiesAccessModifiers(useCompiledXaml);
				page.BindingContext = new Data();
				Assert.AreEqual("Bar", page.AMC.GetValue(AccessModifiersControl.InternalBarProperty));
				Assert.AreEqual("Foo", page.AMC.GetValue(AccessModifiersControl.PublicFooProperty));
			}
		}
	}
}
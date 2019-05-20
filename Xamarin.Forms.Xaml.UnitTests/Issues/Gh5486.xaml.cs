using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Gh5486VM : IGh5486VM
	{
		public Gh5486VM()
		{
			this.Data = new Tag5486()
			{
				Label = "test",
			};
		}

		public ITag5486 Data { get; set; }
	}

	public class Tag5486 : ITag5486
	{
		public string Label { get; set; }
	}

	public interface ITag5486
	{
		string Label { get; set; }
	}

	public interface IGh5486VM : IGh5486VMBase<ITag5486>
	{
	}

	public interface IGh5486VMBase<T>
	{
		T Data { get; }
	}

	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Gh5486 : ContentPage
	{
		public Gh5486()
		{
			InitializeComponent();
		}

		public Gh5486(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
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

			[TestCase(true), TestCase(false)]
			public void GenericBaseInterfaceResolution(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh5486)));
				var layout = new Gh5486(useCompiledXaml) { BindingContext = new Gh5486VM() };
				Assert.That(layout.label.Text, Is.EqualTo("test"));
			}
		}
	}
}

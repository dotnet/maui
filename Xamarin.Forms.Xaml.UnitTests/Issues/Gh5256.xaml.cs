using System.Windows.Input;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Gh5256Entry : Entry
	{
		public Gh5256Entry()
		{
			base.Completed += (o,e) => this.Completed?.Execute(o);
		}
		public static readonly BindableProperty CompletedProperty =
			BindableProperty.Create("Completed", typeof(ICommand), typeof(Gh5256Entry), default(ICommand));

		public new ICommand Completed {
			get => (ICommand)GetValue(CompletedProperty);
			set => SetValue(CompletedProperty, value);
		}
	}

	//[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh5256 : ContentPage
	{
		public Gh5256() => InitializeComponent();
		public Gh5256(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void EventOverriding([Values(false, true)]bool useCompiledXaml)
			{
				var layout = new Gh5256(useCompiledXaml) { BindingContext = new { CompletedCommand = new Command(() => Assert.Pass()) } };
				layout.entry.SendCompleted();
				Assert.Fail();
			}
		}

	}
}

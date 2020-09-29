using System;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Gh5290VM
	{
		public TimeSpan? Time { get; set; }
	}

	public partial class Gh5290 : ContentPage
	{
		public static readonly BindableProperty NullableTimeProperty =
			BindableProperty.Create("NullableTime", typeof(TimeSpan?), typeof(Gh5290), default(TimeSpan?));

		public TimeSpan? NullableTime
		{
			get => (TimeSpan?)GetValue(NullableTimeProperty);
			set => SetValue(NullableTimeProperty, value);
		}

		public Gh5290() => InitializeComponent();
		public Gh5290(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void TwoWayBindingToNullable([Values(false, true)] bool useCompiledXaml)
			{
				var vm = new Gh5290VM { Time = TimeSpan.FromMinutes(42) };
				var layout = new Gh5290(useCompiledXaml) { BindingContext = vm };
				Assert.That(layout.NullableTime, Is.EqualTo(TimeSpan.FromMinutes(42)));

				layout.SetValueFromRenderer(NullableTimeProperty, null);
				Assert.That(vm.Time, Is.Null);
			}
		}
	}
}
using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh5290VM
{
	public TimeSpan? Time { get; set; }
}

public partial class Gh5290 : ContentPage
{
	public static readonly BindableProperty NullableTimeProperty =
		BindableProperty.Create(nameof(NullableTime), typeof(TimeSpan?), typeof(Gh5290), default(TimeSpan?));

	public TimeSpan? NullableTime
	{
		get => (TimeSpan?)GetValue(NullableTimeProperty);
		set => SetValue(NullableTimeProperty, value);
	}

	public Gh5290() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void TwoWayBindingToNullable([Values] XamlInflator inflator)
		{
			var vm = new Gh5290VM { Time = TimeSpan.FromMinutes(42) };
			var layout = new Gh5290(inflator) { BindingContext = vm };
			Assert.That(layout.NullableTime, Is.EqualTo(TimeSpan.FromMinutes(42)));

			layout.SetValueFromRenderer(NullableTimeProperty, null);
			Assert.That(vm.Time, Is.Null);
		}
	}
}
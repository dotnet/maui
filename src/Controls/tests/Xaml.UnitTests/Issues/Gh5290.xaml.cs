using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
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
		class Tests
		{
			[Theory]
			public void TwoWayBindingToNullable([Theory]
		[InlineData(false)]
		[InlineData(true)] bool useCompiledXaml)
			{
				var vm = new Gh5290VM { Time = TimeSpan.FromMinutes(42) };
				var layout = new Gh5290(useCompiledXaml) { BindingContext = vm };
				Assert.Equal(TimeSpan.FromMinutes(42), layout.NullableTime);

				layout.SetValueFromRenderer(NullableTimeProperty, null);
				Assert.Null(vm.Time);
			}
		}
	}
}
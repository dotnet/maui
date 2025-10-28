using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Unreported006 : ContentPage
	{
		public Unreported006()
		{
			InitializeComponent();
		}

		public Unreported006(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		public Controls.Compatibility.Layout<View> GenericProperty
		{
			get { return (Controls.Compatibility.Layout<View>)GetValue(GenericPropertyProperty); }
			set { SetValue(GenericPropertyProperty, value); }
		}

		public static readonly BindableProperty GenericPropertyProperty =
			BindableProperty.Create(nameof(GenericProperty), typeof(Controls.Compatibility.Layout<View>), typeof(Unreported006));		class Tests
		{
			[InlineData(true), TestCase(false)]
			public void CanAssignGenericBP(bool useCompiledXaml)
			{
				var page = new Unreported006();
				Assert.NotNull(page.GenericProperty);
				Assert.IsType<Compatibility.StackLayout>(page.GenericProperty);
			}
		}
	}
}
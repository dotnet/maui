using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui6944 : ContentPage
	{
		public Maui6944() => InitializeComponent();
		public Maui6944(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		// [TestFixture] - removed for xUnit
		class Test
		{
			[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
			[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

			[Fact]
			public void ContentPropertyAttributeOnLayoutSubclass([Values(false, true)] bool useCompiledXaml)
			{
				var page = new Maui6944(useCompiledXaml);
				Assert.NotNull(page.layout);
				Assert.That(page.layout, Is.TypeOf<Maui6944Layout>());
				Assert.Equal(page.label, page.layout.ChildContent);
			}
		}
	}

	public class Maui6944Base : Grid
	{
	}

	[ContentProperty("ChildContent")]
	public class Maui6944Layout : Maui6944Base
	{
		public static readonly BindableProperty ChildContentProperty =
			BindableProperty.Create(
				nameof(ChildContent),
				typeof(View), typeof(Maui6944Layout),
				defaultValue: null);

		public View ChildContent
		{
			get => (View)GetValue(ChildContentProperty);
			set => SetValue(ChildContentProperty, value);
		}

	}
}
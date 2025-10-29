using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui6944 : ContentPage
{
	public Maui6944() => InitializeComponent();

	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void ContentPropertyAttributeOnLayoutSubclass([Values] XamlInflator inflator)
		{
			var page = new Maui6944(inflator);
			Assert.That(page.layout, Is.Not.Null);
			Assert.That(page.layout, Is.TypeOf<Maui6944Layout>());
			Assert.That(page.layout.ChildContent, Is.EqualTo(page.label));
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
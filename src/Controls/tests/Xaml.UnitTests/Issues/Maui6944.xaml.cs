using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui6944 : ContentPage
{
	public Maui6944() => InitializeComponent();

	public class Test
	{
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => AppInfo.SetCurrent(new MockAppInfo());

		[Theory]
		[Values]
		public void ContentPropertyAttributeOnLayoutSubclass(XamlInflator inflator)
		{
			var page = new Maui6944(inflator);
			Assert.NotNull(page.layout);
			Assert.IsType<Maui6944Layout>(page.layout);
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
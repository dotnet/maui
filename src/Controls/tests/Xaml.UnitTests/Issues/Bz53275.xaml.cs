using System;
using System.Reflection;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[RequireService([typeof(IProvideValueTarget)])]
public class TargetPropertyExtension : IMarkupExtension
{
	public object ProvideValue(IServiceProvider serviceProvider)
	{
		var targetProperty = (serviceProvider?.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget)?.TargetProperty;
		return (targetProperty as BindableProperty)?.PropertyName ?? (targetProperty as PropertyInfo)?.Name;
	}
}

public partial class Bz53275 : ContentPage
{
	public Bz53275()
	{
		InitializeComponent();
	}

	public string ANonBindableProperty { get; set; }


	public class Tests
	{
		[Theory]
		[Values]
		public void TargetPropertyIsSetOnMarkups(XamlInflator inflator)
		{
			var page = new Bz53275(inflator);
			Assert.Equal("ANonBindableProperty", page.ANonBindableProperty);
			var l0 = page.label;
			Assert.Equal("Text", l0.Text);
		}
	}
}
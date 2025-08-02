using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Xaml)]
	public class XamlTests
	{
		[Fact("Parsed XAML can use mscorlib")]
		[RequiresUnreferencedCode("XAML parsing may require unreferenced code")]
		public void Namespace_mscorlib_Parsed()
		{
			var page = new ContentPage();
			page.LoadFromXaml(
				"""
				<?xml version="1.0" encoding="UTF-8"?>
				<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
					xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
					xmlns:sys="clr-namespace:System;assembly=mscorlib">
					<RadioButton>
						<RadioButton.Value>
							<sys:Int32>1</sys:Int32>
						</RadioButton.Value>
					</RadioButton>
				</ContentPage>
				""");
			Assert.IsType<RadioButton>(page.Content);
			Assert.Equal(1, ((RadioButton)page.Content).Value);
		}

		[Fact("Compiled XAML can use mscorlib")]
		public void Namespace_mscorlib_Compiled()
		{
			var page = new RadioButtonUsing();
			Assert.IsType<RadioButton>(page.Content);
			Assert.Equal(1, ((RadioButton)page.Content).Value);
		}

		[Fact("Parsed XAML can use x:Array")]
		[RequiresUnreferencedCode("XAML parsing may require unreferenced code")]
		public void x_Array_Parsed()
		{
			var page = new ContentPage();
			page.LoadFromXaml(
				"""
				<?xml version="1.0" encoding="UTF-8"?>
				<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
					xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
					xmlns:sys="clr-namespace:System;assembly=mscorlib">
					<ContentPage.Resources>
						<ResourceDictionary>
							<x:Array Type="{x:Type x:String}" x:Key="MyArray">
								<x:String>Foo</x:String>
								<x:String>Bar</x:String>
								<x:String>Baz</x:String>
							</x:Array>
						</ResourceDictionary>
					</ContentPage.Resources>
				</ContentPage>
				""");
			string[] array = page.Resources["MyArray"] as string[];
			Assert.Equal(new[] { "Foo", "Bar", "Baz" }, array);
		}

		[Fact("Compiled XAML can use x:Array")]
		public void x_Array_Compiled()
		{
			var page = new RadioButtonUsing();
			string[] array = page.Resources["MyArray"] as string[];
			Assert.Equal(new[] { "Foo", "Bar", "Baz" }, array);
		}

		[Fact("Parsed XAML can use x:Double")]
		[RequiresUnreferencedCode()]
		public void x_Double_Parsed()
		{
			var page = new ContentPage();
			page.LoadFromXaml(
				"""
				<?xml version="1.0" encoding="UTF-8"?>
				<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
					xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
					xmlns:sys="clr-namespace:System;assembly=mscorlib">
					<ContentPage.Resources>
						<ResourceDictionary>
							<x:Double x:Key="MyNumber">42</x:Double>
						</ResourceDictionary>
					</ContentPage.Resources>
				</ContentPage>
				""");
			Assert.Equal(42d, page.Resources["MyNumber"]);
		}

		[Fact("Compiled XAML can use x:Double")]
		public void x_Double_Compiled()
		{
			var page = new RadioButtonUsing();
			Assert.Equal(42d, page.Resources["MyNumber"]);
		}
	}
}

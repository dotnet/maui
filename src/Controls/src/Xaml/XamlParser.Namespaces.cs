using System;

namespace Microsoft.Maui.Controls.Xaml
{
	static partial class XamlParser
	{
		[Obsolete("Should not be used except for migration/error message purposes")]
		public const string FormsUri = "http://xamarin.com/schemas/2014/forms";
		public const string DefaultImplicitUri = MauiGlobalUri;
		public const string MauiGlobalUri = "http://schemas.microsoft.com/dotnet/maui/global";
		public const string MauiUri = "http://schemas.microsoft.com/dotnet/2021/maui";
		public const string MauiDesignUri = "http://schemas.microsoft.com/dotnet/2021/maui/design";
		public const string X2006Uri = "http://schemas.microsoft.com/winfx/2006/xaml";
		public const string X2009Uri = "http://schemas.microsoft.com/winfx/2009/xaml";
		public const string McUri = "http://schemas.openxmlformats.org/markup-compatibility/2006";
	}
}
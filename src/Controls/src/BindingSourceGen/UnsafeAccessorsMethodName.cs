namespace Microsoft.Maui.Controls.BindingSourceGen;

internal static class UnsafeAccessorsMethodName
{
	internal static string CreateUnsafeFieldAccessorMethodName(string fieldName) => $"GetUnsafeField_{fieldName}";
	internal static string CreateUnsafePropertyAccessorGetMethodName(string propertyName) => $"GetUnsafeProperty_{propertyName}";
	internal static string CreateUnsafePropertyAccessorSetMethodName(string propertyName) => $"SetUnsafeProperty_{propertyName}";
}

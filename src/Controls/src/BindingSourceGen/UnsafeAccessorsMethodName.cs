namespace Microsoft.Maui.Controls.BindingSourceGen;

internal static class UnsafeAccessorsMethodName
{
	internal static string CreateUnsafeFieldAccessorMethodName(uint bindingId, string fieldName) => $"GetUnsafeField{bindingId}{fieldName}";
	internal static string CreateUnsafePropertyAccessorGetMethodName(uint bindingId, string propertyName) => $"GetUnsafeProperty{bindingId}{propertyName}";
	internal static string CreateUnsafePropertyAccessorSetMethodName(uint bindingId, string propertyName) => $"SetUnsafeProperty{bindingId}{propertyName}";
}

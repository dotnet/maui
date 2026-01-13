namespace Microsoft.Maui.Controls.Xaml.UnitTests.ExternalAssembly.Maui33497;

/// <summary>
/// Enum type to test XmlnsDefinition with AssemblyName resolving types from external assemblies.
/// See https://github.com/dotnet/maui/issues/33497
/// 
/// Note: The XmlnsDefinition for this type is in the CONSUMING assembly (Xaml.UnitTests),
/// not in this external assembly. The bug is that local XmlnsDefinition attributes
/// that point to external assemblies via AssemblyName are not working for global xmlns.
/// </summary>
public enum AlertType
{
	Info,
	Warning,
	Error
}

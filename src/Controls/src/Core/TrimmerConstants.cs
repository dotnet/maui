namespace Microsoft.Maui.Controls;

class TrimmerConstants
{
	// https://github.com/dotnet/runtime/blob/f130138b337b57342e94dabf499b818531effed5/src/libraries/System.Private.DataContractSerialization/src/System/Runtime/Serialization/DataContract.cs#L31-L32
	internal const string SerializerTrimmerWarning = "Data Contract Serialization and Deserialization might require types that cannot be statically analyzed. Make sure all of the required types are preserved.";

	internal const string StringPathBindingWarning = "Using bindings with string paths is not trim safe. Use expression-based binding instead.";

	internal const string? ExpressionBasedBindingsDocsUrl = null; // TODO: we don't have this page yet

	internal const string XamlRuntimeParsingNotSupportedWarning = "Loading XAML at runtime might require types and members that cannot be statically analyzed. Make sure all of the required types and members are preserved.";

	internal const string SearchHandlerDisplayMemberNameNotSupportedWarning = "DisplayMemberName is not supported. Consider implementing custom ItemTemplate instead. Alternatively, enable DisplayMemberName by setting the $(MauiShellSearchResultsRendererDisplayMemberNameSupported) MSBuild property to true. Note: DisplayMemberName is not trimming-safe and it might not work as expected in NativeAOT or fully trimmed apps.";

	internal const string QueryPropertyAttributeWarning = "Using QueryPropertyAttribute is not trimming friendly and might not work correctly. Implement the IQueryAttributable interface instead.";
	internal const string QueryPropertyDocsUrl = "https://learn.microsoft.com/dotnet/maui/fundamentals/shell/navigation#process-navigation-data-using-a-single-method";
}
namespace Microsoft.Maui.Controls;

class TrimmerConstants
{
	// https://github.com/dotnet/runtime/blob/f130138b337b57342e94dabf499b818531effed5/src/libraries/System.Private.DataContractSerialization/src/System/Runtime/Serialization/DataContract.cs#L31-L32
	internal const string SerializerTrimmerWarning = "Data Contract Serialization and Deserialization might require types that cannot be statically analyzed. Make sure all of the required types are preserved.";

	internal const string NativeBindingService = "This method properly handles missing properties, and there is not a way to preserve them from this method.";

	internal const string XamlLoadingTrimmerWarning = "Loading XAML at runtime might require types that cannot be statically analyzed. Make sure all of the required types are preserved.";
}
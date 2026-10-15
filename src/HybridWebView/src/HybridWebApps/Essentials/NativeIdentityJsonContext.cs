using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls.HybridWebApps.Essentials;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	AllowDuplicateProperties = false,
	Converters = [typeof(NativeDevicePlatformConverter), typeof(NativeDeviceIdiomConverter)],
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(DevicePlatform))]
[JsonSerializable(typeof(DeviceIdiom))]
[JsonSerializable(typeof(Version))]
internal partial class NativeIdentityJsonContext : JsonSerializerContext;

internal sealed class NativeDevicePlatformConverter : JsonConverter<DevicePlatform>
{
	public override DevicePlatform Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) =>
		reader.GetString() is { } text ? text.Length == 0 ? default : DevicePlatform.Create(text) : throw new JsonException();
	public override void Write(Utf8JsonWriter writer, DevicePlatform value, JsonSerializerOptions options) =>
		writer.WriteStringValue(value.ToString());
}

internal sealed class NativeDeviceIdiomConverter : JsonConverter<DeviceIdiom>
{
	public override DeviceIdiom Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) =>
		reader.GetString() is { } text ? text.Length == 0 ? default : DeviceIdiom.Create(text) : throw new JsonException();
	public override void Write(Utf8JsonWriter writer, DeviceIdiom value, JsonSerializerOptions options) =>
		writer.WriteStringValue(value.ToString());
}

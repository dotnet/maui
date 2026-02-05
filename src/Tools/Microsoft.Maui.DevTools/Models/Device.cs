// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;

namespace Microsoft.Maui.DevTools.Models;

/// <summary>
/// Represents a device (physical device, emulator, or simulator).
/// Follows the MauiDevice schema for cross-platform device representation.
/// </summary>
public record Device
{
	/// <summary>
	/// Display name of the device (e.g., "iPhone 15 Pro", "Pixel 6").
	/// </summary>
	[JsonPropertyName("name")]
	public required string Name { get; init; }

	/// <summary>
	/// Unique identifier for the device (e.g., UDID for iOS, serial for Android).
	/// </summary>
	[JsonPropertyName("identifier")]
	public required string Id { get; init; }

	/// <summary>
	/// Emulator/AVD identifier if applicable (e.g., AVD name for Android emulators).
	/// </summary>
	[JsonPropertyName("emulator_id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? EmulatorId { get; init; }

	/// <summary>
	/// Supported platforms (e.g., ["android"], ["ios", "maccatalyst"]).
	/// </summary>
	[JsonPropertyName("platforms")]
	public required string[] Platforms { get; init; }

	/// <summary>
	/// OS version number (e.g., "35" for Android API 35, "18.5" for iOS 18.5).
	/// </summary>
	[JsonPropertyName("version")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Version { get; init; }

	/// <summary>
	/// Human-readable OS version name (e.g., "Android 15", "iOS 18.5").
	/// </summary>
	[JsonPropertyName("version_name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? VersionName { get; init; }

	/// <summary>
	/// Device manufacturer (e.g., "Google", "Samsung", "Apple").
	/// </summary>
	[JsonPropertyName("manufacturer")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Manufacturer { get; init; }

	/// <summary>
	/// Device model (e.g., "Pixel 6", "iPhone 15 Pro").
	/// </summary>
	[JsonPropertyName("model")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Model { get; init; }

	/// <summary>
	/// Device sub-model or variant (e.g., "Pro Max", "Ultra").
	/// </summary>
	[JsonPropertyName("sub_model")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? SubModel { get; init; }

	/// <summary>
	/// Device form factor/idiom (e.g., "phone", "tablet", "watch", "tv", "desktop").
	/// </summary>
	[JsonPropertyName("idiom")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Idiom { get; init; }

	/// <summary>
	/// Platform architecture (e.g., "arm64-v8a", "x86_64").
	/// </summary>
	[JsonPropertyName("platform_architecture")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? PlatformArchitecture { get; init; }

	/// <summary>
	/// .NET runtime identifiers for this device (e.g., ["android-arm64", "android-x64"]).
	/// </summary>
	[JsonPropertyName("runtime_identifiers")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string[]? RuntimeIdentifiers { get; init; }

	/// <summary>
	/// CPU architecture (e.g., "arm64", "x86_64").
	/// </summary>
	[JsonPropertyName("architecture")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Architecture { get; init; }

	/// <summary>
	/// Whether this is an emulator/simulator (true) or physical device (false).
	/// </summary>
	[JsonPropertyName("is_emulator")]
	public bool IsEmulator { get; init; }

	/// <summary>
	/// Whether the device is currently running/booted.
	/// </summary>
	[JsonPropertyName("is_running")]
	public bool IsRunning { get; init; }

	/// <summary>
	/// Connection type (e.g., "usb", "wifi", "local" for simulators).
	/// </summary>
	[JsonPropertyName("connection_type")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? ConnectionType { get; init; }

	// Legacy properties for backward compatibility
	
	/// <summary>
	/// Platform identifier (legacy, use Platforms array instead).
	/// </summary>
	[JsonPropertyName("platform")]
	public string Platform => Platforms.FirstOrDefault() ?? "unknown";

	/// <summary>
	/// Device type enum (legacy, use IsEmulator instead).
	/// </summary>
	[JsonPropertyName("type")]
	public DeviceType Type { get; init; }

	/// <summary>
	/// Device state enum (legacy, use IsRunning instead).
	/// </summary>
	[JsonPropertyName("state")]
	public DeviceState State { get; init; }

	/// <summary>
	/// OS version (legacy alias for Version).
	/// </summary>
	[JsonPropertyName("os_version")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? OsVersion => Version;

	/// <summary>
	/// Additional device-specific details.
	/// </summary>
	[JsonPropertyName("details")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public Dictionary<string, object>? Details { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DeviceType
{
	Physical,
	Emulator,
	Simulator
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DeviceState
{
	Unknown,
	Connected,
	Disconnected,
	Booted,
	Shutdown,
	Booting,
	ShuttingDown,
	Offline
}

/// <summary>
/// Device idiom/form factor.
/// </summary>
public static class DeviceIdiom
{
	public const string Phone = "phone";
	public const string Tablet = "tablet";
	public const string Watch = "watch";
	public const string TV = "tv";
	public const string Desktop = "desktop";
	public const string Unknown = "unknown";
}

/// <summary>
/// Connection types for devices.
/// </summary>
public static class ConnectionType
{
	public const string Usb = "usb";
	public const string Wifi = "wifi";
	public const string Local = "local";
	public const string Unknown = "unknown";
}

/// <summary>
/// Result of device list command.
/// </summary>
public record DeviceListResult
{
	[JsonPropertyName("devices")]
	public required List<Device> Devices { get; init; }

	[JsonPropertyName("count")]
	public int Count => Devices.Count;
}

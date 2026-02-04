// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;

namespace Microsoft.Maui.DevTools.Models;

/// <summary>
/// Represents a device (physical device, emulator, or simulator).
/// </summary>
public record Device
{
	[JsonPropertyName("id")]
	public required string Id { get; init; }

	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("platform")]
	public required string Platform { get; init; }

	[JsonPropertyName("type")]
	public required DeviceType Type { get; init; }

	[JsonPropertyName("state")]
	public required DeviceState State { get; init; }

	[JsonPropertyName("os_version")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? OsVersion { get; init; }

	[JsonPropertyName("architecture")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Architecture { get; init; }

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
/// Result of device list command.
/// </summary>
public record DeviceListResult
{
	[JsonPropertyName("devices")]
	public required List<Device> Devices { get; init; }

	[JsonPropertyName("count")]
	public int Count => Devices.Count;
}

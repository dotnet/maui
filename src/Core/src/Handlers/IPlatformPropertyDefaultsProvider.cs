namespace Microsoft.Maui;

// TODO: Potentially make public in NET10
internal interface IPlatformPropertyDefaultsProvider
{
	IPlatformPropertyDefaults? PlatformPropertyDefaults { get; }
}
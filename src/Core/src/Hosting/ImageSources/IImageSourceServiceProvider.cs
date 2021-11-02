#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui
{
	public interface IImageSourceServiceProvider : IServiceProvider
	{
		IServiceProvider HostServiceProvider { get; }

		IImageSourceService? GetImageSourceService([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type imageSource);

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
		Type GetImageSourceServiceType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type imageSource);

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
		Type GetImageSourceType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type imageSource);
	}
}
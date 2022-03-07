#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Compatibility.Hosting
{
	public static class MauiAppBuilderExtensions
	{
		public static MauiAppBuilder AddVisual<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TVisualType>(this MauiAppBuilder builder)
		=> AddVisual(builder, typeof(TVisualType));

		public static MauiAppBuilder AddVisual(this MauiAppBuilder builder, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type TVisualType)
		{
			Internals.Registrar.Registered.RegisterVisual(TVisualType);
			return builder;
		}
	}
}

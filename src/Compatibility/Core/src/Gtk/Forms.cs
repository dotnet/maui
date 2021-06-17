using System;
using System.Reflection;

namespace Microsoft.Maui.Controls.Compatibility
{

	public static class Forms
	{

		public static void Init(IActivationState state)
		{
			var gtkServices = new GtkPlatformServices();
			Device.PlatformServices = gtkServices;
		}

		internal static void RegisterCompatRenderers(
			Assembly[] assemblies,
			Assembly defaultRendererAssembly,
			Action<Type> viewRegistered)
		{
			;
		}

	}

}

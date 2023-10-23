using System;
using System.Reflection;
using Microsoft.Maui.Controls.Compatibility.Platform.Gtk;

namespace Microsoft.Maui.Controls.Compatibility
{
	public static class Forms
	{
		public static void Init(IActivationState state)
		{
			DependencyService.Register<ResourcesProvider>();
		}
	}
}
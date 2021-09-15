using System.ComponentModel;
using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.Controls
{
	public static class VisualMarker
	{
		static bool _isMaterialRegistered = false;
		static bool _warnedAboutMaterial = false;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IVisual MatchParent { get; } = new MatchParentVisual();
		public static IVisual Default { get; } = new DefaultVisual();
		public static IVisual Material { get; } = new MaterialVisual();

		internal static void RegisterMaterial() => _isMaterialRegistered = true;
		internal static void MaterialCheck()
		{
			if (_isMaterialRegistered || _warnedAboutMaterial)
				return;

			_warnedAboutMaterial = true;
			if (DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.Tizen)
				Internals.Log.Warning("Visual", $"Material needs to be registered on {DeviceInfo.Platform} by calling FormsMaterial.Init() after the Microsoft.Maui.Controls.Forms.Init method call.");
			else
				Internals.Log.Warning("Visual", $"Material is currently not support on {DeviceInfo.Platform}.");
		}

		public sealed class MaterialVisual : IVisual { public MaterialVisual() { } }
		public sealed class DefaultVisual : IVisual { public DefaultVisual() { } }
		internal sealed class MatchParentVisual : IVisual { public MatchParentVisual() { } }
	}
}
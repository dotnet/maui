#nullable disable
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides static marker instances for built-in <see cref="IVisual"/> types.
	/// </summary>
	public static class VisualMarker
	{
		static bool _isMaterialRegistered = false;
		static bool _warnedAboutMaterial = false;

		/// <summary>
		/// Gets a visual marker that indicates the control should inherit its visual from its parent.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IVisual MatchParent { get; } = new MatchParentVisual();

		/// <summary>
		/// Gets the default visual marker.
		/// </summary>
		public static IVisual Default { get; } = new DefaultVisual();

		internal static IVisual Material { get; } = new MaterialVisual();

		internal static void RegisterMaterial() => _isMaterialRegistered = true;
		internal static void MaterialCheck()
		{
			if (_isMaterialRegistered || _warnedAboutMaterial)
				return;

			var logger = Application.Current?.FindMauiContext()?.CreateLogger<IVisual>();
			_warnedAboutMaterial = true;
			if (DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.Tizen)
				logger?.LogWarning("Material needs to be registered on {RuntimePlatform} by calling FormsMaterial.Init() after the Microsoft.Maui.Controls.Forms.Init method call.", DeviceInfo.Platform);
			else
				logger?.LogWarning("Material is currently not support on {RuntimePlatform}.", DeviceInfo.Platform);
		}

		internal sealed class MaterialVisual : IVisual { public MaterialVisual() { } }
		public sealed class DefaultVisual : IVisual { public DefaultVisual() { } }
		internal sealed class MatchParentVisual : IVisual { public MatchParentVisual() { } }
	}
}
namespace Xamarin.Forms
{
	public static class VisualMarker
	{
		static bool _isMaterialRegistered = false;
		static bool _warnedAboutMaterial = false;

		public static IVisual MatchParent { get; } = new MatchParentVisual();
		public static IVisual Default { get; } = new DefaultVisual();
		public static IVisual Material { get; } = new MaterialVisual();

		internal static void RegisterMaterial() => _isMaterialRegistered = true;
		internal static void MaterialCheck()
		{
			if (_isMaterialRegistered || _warnedAboutMaterial)
				return;

			_warnedAboutMaterial = true;
			if (Device.RuntimePlatform == Device.iOS)
				Internals.Log.Warning("Visual", "Material needs to be registered on iOS by calling FormsMaterial.Init() after the Xamarin.Forms.Forms.Init method call.");
			else if (Device.RuntimePlatform != Device.Android)
				Internals.Log.Warning("Visual", $"Material is currently not support on {Device.RuntimePlatform}.");
		}


		public sealed class MaterialVisual : IVisual { public MaterialVisual() { } }
		public sealed class DefaultVisual : IVisual { public DefaultVisual() { } }
		internal sealed class MatchParentVisual : IVisual { public MatchParentVisual() { } }
	}
}
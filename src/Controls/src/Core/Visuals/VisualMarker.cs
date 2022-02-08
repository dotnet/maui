using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/VisualMarker.xml" path="Type[@FullName='Microsoft.Maui.Controls.VisualMarker']/Docs" />
	public static class VisualMarker
	{
		static bool _isMaterialRegistered = false;
		static bool _warnedAboutMaterial = false;

		/// <include file="../../../docs/Microsoft.Maui.Controls/VisualMarker.xml" path="//Member[@MemberName='MatchParent']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IVisual MatchParent { get; } = new MatchParentVisual();
		/// <include file="../../../docs/Microsoft.Maui.Controls/VisualMarker.xml" path="//Member[@MemberName='Default']/Docs" />
		public static IVisual Default { get; } = new DefaultVisual();
		/// <include file="../../../docs/Microsoft.Maui.Controls/VisualMarker.xml" path="//Member[@MemberName='Material']/Docs" />
		public static IVisual Material { get; } = new MaterialVisual();

		internal static void RegisterMaterial() => _isMaterialRegistered = true;
		internal static void MaterialCheck()
		{
			if (_isMaterialRegistered || _warnedAboutMaterial)
				return;

			var logger = Application.Current?.FindMauiContext()?.CreateLogger<IVisual>();
			_warnedAboutMaterial = true;
			if (Device.RuntimePlatform == Device.iOS || Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.Tizen)
				logger?.LogWarning("Material needs to be registered on {RuntimePlatform} by calling FormsMaterial.Init() after the Microsoft.Maui.Controls.Forms.Init method call.", Device.RuntimePlatform);
			else
				logger?.LogWarning("Material is currently not support on {RuntimePlatform}.", Device.RuntimePlatform);
		}

		public sealed class MaterialVisual : IVisual { public MaterialVisual() { } }
		public sealed class DefaultVisual : IVisual { public DefaultVisual() { } }
		internal sealed class MatchParentVisual : IVisual { public MatchParentVisual() { } }
	}
}
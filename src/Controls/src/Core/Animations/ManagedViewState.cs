#nullable enable
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// The managed values of an element whose native view a transition poses or hides. A transition puts back the native
	/// state it captured, which is only right while the managed values that map onto it are unchanged: whatever changed
	/// meanwhile (a binding updating Opacity mid-flight, say) is applied again through the element's handler, so the
	/// native view ends up showing the element's current values rather than the ones it had when the transition began.
	/// </summary>
	internal readonly struct ManagedViewState
	{
		ManagedViewState(VisualElement element)
		{
			Element = element;
			Opacity = element.Opacity;
			IsVisible = element.IsVisible;
			TranslationX = element.TranslationX;
			TranslationY = element.TranslationY;
			Scale = element.Scale;
			ScaleX = element.ScaleX;
			ScaleY = element.ScaleY;
			Rotation = element.Rotation;
			RotationX = element.RotationX;
			RotationY = element.RotationY;
			AnchorX = element.AnchorX;
			AnchorY = element.AnchorY;
		}

		public VisualElement? Element { get; }

		public double Opacity { get; }

		public bool IsVisible { get; }

		public double TranslationX { get; }

		public double TranslationY { get; }

		public double Scale { get; }

		public double ScaleX { get; }

		public double ScaleY { get; }

		public double Rotation { get; }

		public double RotationX { get; }

		public double RotationY { get; }

		public double AnchorX { get; }

		public double AnchorY { get; }

		public static ManagedViewState Of(VisualElement? element) => element is null ? default : new ManagedViewState(element);

		public bool OpacityChanged => Element is not null && Element.Opacity != Opacity;

		public bool VisibilityChanged => Element is not null && Element.IsVisible != IsVisible;

		/// <summary>The handler properties whose managed values changed since this was captured.</summary>
		public List<string> ChangedProperties()
		{
			var changed = new List<string>();
			var element = Element;
			if (element is null)
			{
				return changed;
			}

			Add(changed, element.Opacity != Opacity, nameof(IView.Opacity));
			Add(changed, element.IsVisible != IsVisible, nameof(IView.Visibility));
			Add(changed, element.TranslationX != TranslationX, nameof(IView.TranslationX));
			Add(changed, element.TranslationY != TranslationY, nameof(IView.TranslationY));
			Add(changed, element.Scale != Scale, nameof(IView.Scale));
			Add(changed, element.ScaleX != ScaleX, nameof(IView.ScaleX));
			Add(changed, element.ScaleY != ScaleY, nameof(IView.ScaleY));
			Add(changed, element.Rotation != Rotation, nameof(IView.Rotation));
			Add(changed, element.RotationX != RotationX, nameof(IView.RotationX));
			Add(changed, element.RotationY != RotationY, nameof(IView.RotationY));
			Add(changed, element.AnchorX != AnchorX, nameof(IView.AnchorX));
			Add(changed, element.AnchorY != AnchorY, nameof(IView.AnchorY));
			return changed;
		}

		/// <summary>
		/// Call after the captured native state has been put back: maps again, through the element's handler, every
		/// managed value that changed since this was captured.
		/// </summary>
		public void ReapplyChanged()
		{
			if (Element?.Handler is not { } handler)
			{
				return;
			}

			foreach (var property in ChangedProperties())
			{
				handler.UpdateValue(property);
			}
		}

		static void Add(List<string> changed, bool condition, string property)
		{
			if (condition)
			{
				changed.Add(property);
			}
		}
	}
}

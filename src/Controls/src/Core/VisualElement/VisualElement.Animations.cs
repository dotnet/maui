#nullable enable
namespace Microsoft.Maui.Controls
{
	public partial class VisualElement
	{
		// Bits of the transform properties changed during the current animation frame.
		const int TranslationXBit = 1 << 0;
		const int TranslationYBit = 1 << 1;
		const int ScaleBit = 1 << 2;
		const int ScaleXBit = 1 << 3;
		const int ScaleYBit = 1 << 4;
		const int RotationBit = 1 << 5;
		const int RotationXBit = 1 << 6;
		const int RotationYBit = 1 << 7;
		const int AnchorXBit = 1 << 8;
		const int AnchorYBit = 1 << 9;

		int _animationFrameDepth;
		int _deferredTransforms;

		/// <summary>
		/// Starts applying one animation frame to this element. Until <see cref="EndAnimationFrame"/>, transform
		/// property changes still raise <c>PropertyChanged</c> but don't reach the handler.
		/// </summary>
		/// <remarks>
		/// On iOS, Mac Catalyst and Windows every transform mapper rebuilds the whole platform transform, so three
		/// animated transform properties would rebuild it three times per frame; the frame flushes it once instead.
		/// Android maps each transform property to its own <c>View</c> setter, so each changed property is flushed.
		/// </remarks>
		internal void BeginAnimationFrame() => _animationFrameDepth++;

		internal void EndAnimationFrame()
		{
			if (_animationFrameDepth == 0 || --_animationFrameDepth > 0)
				return;

			var deferred = _deferredTransforms;
			_deferredTransforms = 0;
			if (deferred == 0 || Handler is not IViewHandler handler)
				return;

#if IOS || MACCATALYST || WINDOWS
			// MAUI's own transform mappings each read every transform property and rebuild the platform transform, so
			// one of them is enough. A property whose mapping was replaced or appended to still gets its own update, and
			// a handler type MAUI doesn't ship (which may override UpdateValue) gets every update.
			var mapper = IsMauiHandler(handler) ? (handler as ElementHandler)?._mapper : null;
			var rebuilt = false;
			FlushCoalesced(handler, mapper, deferred, TranslationXBit, 0, ref rebuilt);
			FlushCoalesced(handler, mapper, deferred, TranslationYBit, 1, ref rebuilt);
			FlushCoalesced(handler, mapper, deferred, ScaleBit, 2, ref rebuilt);
			FlushCoalesced(handler, mapper, deferred, ScaleXBit, 3, ref rebuilt);
			FlushCoalesced(handler, mapper, deferred, ScaleYBit, 4, ref rebuilt);
			FlushCoalesced(handler, mapper, deferred, RotationBit, 5, ref rebuilt);
			FlushCoalesced(handler, mapper, deferred, RotationXBit, 6, ref rebuilt);
			FlushCoalesced(handler, mapper, deferred, RotationYBit, 7, ref rebuilt);
			FlushCoalesced(handler, mapper, deferred, AnchorXBit, 8, ref rebuilt);
			FlushCoalesced(handler, mapper, deferred, AnchorYBit, 9, ref rebuilt);
#else
			Flush(handler, deferred, TranslationXBit, nameof(IView.TranslationX));
			Flush(handler, deferred, TranslationYBit, nameof(IView.TranslationY));
			Flush(handler, deferred, ScaleBit, nameof(IView.Scale));
			Flush(handler, deferred, ScaleXBit, nameof(IView.ScaleX));
			Flush(handler, deferred, ScaleYBit, nameof(IView.ScaleY));
			Flush(handler, deferred, RotationBit, nameof(IView.Rotation));
			Flush(handler, deferred, RotationXBit, nameof(IView.RotationX));
			Flush(handler, deferred, RotationYBit, nameof(IView.RotationY));
			Flush(handler, deferred, AnchorXBit, nameof(IView.AnchorX));
			Flush(handler, deferred, AnchorYBit, nameof(IView.AnchorY));
#endif
		}

#if IOS || MACCATALYST || WINDOWS
		static void FlushCoalesced(IViewHandler handler, IPropertyMapper? mapper, int deferred, int bit, int index, ref bool rebuilt)
		{
			if ((deferred & bit) == 0)
				return;

			var property = Microsoft.Maui.Handlers.ViewHandler.TransformMappingKeys[index];
			var isBuiltIn = IsBuiltInTransformMapping(mapper, index);
			if (isBuiltIn && rebuilt)
				return;

			handler.UpdateValue(property);
			rebuilt |= isBuiltIn;
		}
#else
		static void Flush(IViewHandler handler, int deferred, int bit, string property)
		{
			if ((deferred & bit) != 0)
				handler.UpdateValue(property);
		}
#endif

		/// <summary>
		/// Whether <paramref name="mapper"/> maps the transform property at <paramref name="index"/> (of
		/// <c>ViewHandler.TransformMappingKeys</c>) to MAUI's own mapping, compared with the mapping captured when
		/// ViewMapper was created: ViewMapper is mutable, so appending to or replacing it globally changes it too.
		/// </summary>
		internal static bool IsBuiltInTransformMapping(IPropertyMapper? mapper, int index)
		{
			var builtIn = Microsoft.Maui.Handlers.ViewHandler.BuiltInTransformMappings[index];
			return builtIn is not null && mapper is not null
				&& ReferenceEquals(mapper.GetProperty(Microsoft.Maui.Handlers.ViewHandler.TransformMappingKeys[index]), builtIn);
		}

		static readonly System.Reflection.Assembly s_coreAssembly = typeof(Microsoft.Maui.Handlers.ViewHandler).Assembly;
		static readonly System.Reflection.Assembly s_controlsAssembly = typeof(VisualElement).Assembly;

		internal static bool IsMauiHandler(IViewHandler handler)
		{
			var assembly = handler.GetType().Assembly;
			return ReferenceEquals(assembly, s_coreAssembly) || ReferenceEquals(assembly, s_controlsAssembly);
		}

		bool DeferTransformUpdate(string property)
		{
			var bit = property switch
			{
				nameof(TranslationX) => TranslationXBit,
				nameof(TranslationY) => TranslationYBit,
				nameof(Scale) => ScaleBit,
				nameof(ScaleX) => ScaleXBit,
				nameof(ScaleY) => ScaleYBit,
				nameof(Rotation) => RotationBit,
				nameof(RotationX) => RotationXBit,
				nameof(RotationY) => RotationYBit,
				nameof(AnchorX) => AnchorXBit,
				nameof(AnchorY) => AnchorYBit,
				_ => 0,
			};

			if (bit == 0)
				return false;

			_deferredTransforms |= bit;
			return true;
		}
	}
}

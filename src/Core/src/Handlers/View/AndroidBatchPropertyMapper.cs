using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Handlers
{
#if ANDROID
	class AndroidBatchPropertyMapper<TVirtualView, TViewHandler> : PropertyMapper<TVirtualView, TViewHandler>
		where TVirtualView : IElement
		where TViewHandler : IElementHandler
	{
		// During mass property updates, this list of properties will be skipped
		public static HashSet<string> SkipList = new(StringComparer.Ordinal)
		{ 
			nameof(IView.Visibility),
			nameof(IView.MinimumHeight),
			nameof(IView.MinimumWidth),
			nameof(IView.IsEnabled),
			nameof(IView.Opacity),
			nameof(IView.TranslationX),
			nameof(IView.TranslationY),
			nameof(IView.Scale),
			nameof(IView.ScaleX),
			nameof(IView.ScaleY),
			nameof(IView.Rotation),
			nameof(IView.RotationX),
			nameof(IView.RotationY),
			nameof(IView.AnchorX),
			nameof(IView.AnchorY),
		};

		public AndroidBatchPropertyMapper(params IPropertyMapper[] chained) : base(chained) { }

		public override IEnumerable<string> GetKeys()
		{
			foreach (var key in _mapper.Keys)
			{
				// When reporting the key list for mass updates up the chain, ignore properties in SkipList.
				// These will be handled by ViewHandler.SetVirtualView() instead.
				if (SkipList.Contains(key))
				{
					continue;
				}

				yield return key;
			}

			if (Chained is not null)
			{
				foreach (var chain in Chained)
					foreach (var key in chain.GetKeys())
						yield return key;
			}
		}
	}
#endif
}

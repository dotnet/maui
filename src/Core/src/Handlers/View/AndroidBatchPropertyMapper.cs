using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Handlers
{
#if ANDROID
	class AndroidBatchPropertyMapper
	{
		public const string InitializeBatchedPropertiesKey = "_InitializeBatchedProperties";

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
	}

	class AndroidBatchPropertyMapper<TVirtualView, TViewHandler> : PropertyMapper<TVirtualView, TViewHandler>
		where TVirtualView : IElement
		where TViewHandler : IElementHandler
	{

		public AndroidBatchPropertyMapper(params IPropertyMapper[] chained) : base(chained) { }

		public override IEnumerable<string> GetKeys()
		{
			var skipList = AndroidBatchPropertyMapper.SkipList;

			// We want to retain the initial order of the keys to avoid race conditions
			// when a property mapping is overridden by a new instance of property mapper.
			// As an example, the container view mapper should always run first.
			// Siblings mapper should not have keys intersection.
			var chainedPropertyMappers = Chained;
			if (chainedPropertyMappers is not null)
			{
				for (int i = chainedPropertyMappers.Length - 1; i >= 0; i--)
				{
					foreach (var key in chainedPropertyMappers[i].GetKeys())
					{
						yield return key;
					}
				}
			}

			// Enqueue keys from this mapper.
			foreach (var mapper in _mapper)
			{
				var key = mapper.Key;

				// When reporting the key list for mass updates up the chain, ignore properties in SkipList.
				// These will be handled by ViewHandler.SetVirtualView() instead.
				if (!skipList.Contains(key))
				{
					yield return key;
				}
			}
		}
	}
#endif
}
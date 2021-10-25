using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		public static IPropertyMapper<ILabel, LabelHandler> ControlsLabelMapper = new ControlsRemapper<Label, LabelHandler>(LabelHandler.LabelMapper)
		{
			[nameof(TextType)] = MapTextType,
			[nameof(Text)] = MapText,
			[nameof(TextTransform)] = MapText,
#if __IOS__
			[nameof(TextDecorations)] = MapTextDecorations,
			[nameof(CharacterSpacing)] = MapCharacterSpacing,
			[nameof(LineHeight)] = MapLineHeight,
			[nameof(ILabel.Font)] = MapFont,
			[nameof(TextColor)] = MapTextColor
#endif
		};

		public static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Label legacy behaviors
			// ILabel does not include the TextType property, so we map it here to handle HTML text
			// And we map some of the other property handlers to Controls-specific versions that avoid stepping on HTML text settings


			if (Mapper is ControlsRemapper<Label, LabelHandler> cr)
			{
				// Grab all keys that the user set themselves on here and propagate them to the ViewhandlerMapper
				foreach (var kvp in cr.Mapper)
				{
					LabelHandler.LabelMapper.Add(kvp.Key, cr.Mapper[kvp.Key]);
				}
			}

			ControlsLabelMapper = LabelHandler.LabelMapper;
		}
	}



	internal class ControlsRemapper<TVirtualView, TViewHandler> : IPropertyMapper<TVirtualView, TViewHandler>
		where TVirtualView : Microsoft.Maui.IElement
		where TViewHandler : IElementHandler
	{
		IPropertyMapper _parentMapper;

		readonly Dictionary<string, Action<IElementHandler, Microsoft.Maui.IElement>> _mapper = new();

		internal Dictionary<string, Action<IElementHandler, Microsoft.Maui.IElement>> Mapper => _mapper;

		public ControlsRemapper(IPropertyMapper parentMapper)
		{
			_parentMapper = parentMapper;
		}

		public Action<TViewHandler, TVirtualView> this[string key]
		{
			get
			{
				var action = GetProperty(key) ?? throw new IndexOutOfRangeException($"Unable to find mapping for '{nameof(key)}'.");
				return new Action<TViewHandler, TVirtualView>((h, v) => action.Invoke(h, v));
			}
			set => Add(key, value);
		}


		protected virtual void SetPropertyCore(string key, Action<IElementHandler, Microsoft.Maui.IElement> action)
		{
			_mapper[key] = action;
		}

		public void Add(string key, Action<TViewHandler, TVirtualView> action) =>
			SetPropertyCore(key, (h, v) =>
			{
				if (v is TVirtualView vv)
					action?.Invoke((TViewHandler)h, vv);
			});

		public IEnumerable<string> GetKeys()
		{
			return _parentMapper.GetKeys();
		}

		public Action<IElementHandler, Maui.IElement> GetProperty(string key)
		{
			return _parentMapper.GetProperty(key);
		}

		public void UpdateProperties(IElementHandler elementHandler, Maui.IElement virtualView)
		{
			_parentMapper.UpdateProperties(elementHandler, virtualView);
		}

		public void UpdateProperty(IElementHandler elementHandler, Maui.IElement virtualView, string property)
		{
			_parentMapper.UpdateProperty(elementHandler, virtualView, property);
		}
	}
}

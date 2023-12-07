using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls.Compatibility
{
	[Obsolete("Delete this for .NET9")]
	class ControlsMapper<TVirtualView, TViewHandler> : IPropertyMapper<TVirtualView, TViewHandler>
			where TVirtualView : IElement
			where TViewHandler : IElementHandler
	{
		IPropertyMapper<IElement, IElementHandler> _propertyMapper;

		public ControlsMapper(IPropertyMapper<IElement, IElementHandler> propertyMapper) : base()
		{
			_propertyMapper = propertyMapper;
		}

		public void Add(string key, Action<TViewHandler, TVirtualView> action)
		{
			_propertyMapper.ReplaceMapping(key, action);
		}

		public IEnumerable<string> GetKeys()
		{
			return _propertyMapper.GetKeys();
		}

		public Action<IElementHandler, IElement>? GetProperty(string key)
		{
			return _propertyMapper.GetProperty(key);
		}

		public void UpdateProperties(IElementHandler elementHandler, IElement virtualView)
		{
			_propertyMapper.UpdateProperties(elementHandler, virtualView);
		}

		public void UpdateProperty(IElementHandler elementHandler, IElement virtualView, string property)
		{
			_propertyMapper.UpdateProperty(elementHandler, virtualView, property);
		}
	}
}

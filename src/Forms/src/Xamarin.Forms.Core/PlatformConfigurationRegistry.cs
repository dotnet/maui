using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Xamarin.Forms
{
	/// <summary>
	/// Helper that handles storing and lookup of platform specifics implementations
	/// </summary>
	/// <typeparam name="TElement">The Element type</typeparam>
	public class PlatformConfigurationRegistry<TElement> : IElementConfiguration<TElement>
		where TElement : Element
	{
		readonly TElement _element;
		readonly Dictionary<Type, object> _platformSpecifics = new Dictionary<Type, object>();

		public PlatformConfigurationRegistry(TElement element)
		{
			_element = element;
		}

		public IPlatformElementConfiguration<T, TElement> On<T>() where T : IConfigPlatform
		{
			if (_platformSpecifics.ContainsKey(typeof(T)))
			{
				return (IPlatformElementConfiguration<T, TElement>)_platformSpecifics[typeof(T)];
			}

			var emptyConfig = Configuration<T, TElement>.Create(_element);

			_platformSpecifics.Add(typeof(T), emptyConfig);

			return emptyConfig;
		}
	}
}

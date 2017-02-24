using System;
using System.Collections;
using System.Collections.Generic;

namespace Xamarin.Forms.Xaml
{
	[ContentProperty("Items")]
	[AcceptEmptyServiceProvider]
	public class ArrayExtension : IMarkupExtension<Array>
	{
		public ArrayExtension()
		{
			Items = new List<object>();
		}

		public IList Items { get; }

		public Type Type { get; set; }

		public Array ProvideValue(IServiceProvider serviceProvider)
		{
			if (Type == null)
				throw new InvalidOperationException("Type argument mandatory for x:Array extension");

			if (Items == null)
				return null;

			var array = Array.CreateInstance(Type, Items.Count);
			for (var i = 0; i < Items.Count; i++)
				((IList)array)[i] = Items[i];

			return array;
		}

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
		{
			return (this as IMarkupExtension<Array>).ProvideValue(serviceProvider);
		}
	}
}
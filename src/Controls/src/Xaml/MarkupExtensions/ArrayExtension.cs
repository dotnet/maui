using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls.Xaml
{
	/// <summary>
	/// Provides a XAML markup extension that creates an array of objects.
	/// </summary>
	[ContentProperty(nameof(Items))]
	[AcceptEmptyServiceProvider]
#if !NETSTANDARD
	[RequiresDynamicCode("ArrayExtension is not AOT safe.")]
#endif
	public class ArrayExtension : IMarkupExtension<Array>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ArrayExtension"/> class.
		/// </summary>
		public ArrayExtension()
		{
			Items = new List<object>();
		}

		/// <summary>
		/// Gets the list of items to include in the array.
		/// </summary>
		public IList Items { get; }

		/// <summary>
		/// Gets or sets the type of elements in the array.
		/// </summary>
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
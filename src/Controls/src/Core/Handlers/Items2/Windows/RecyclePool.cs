#nullable disable
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal class RecyclePool
	{
		private class ElementInfo(UIElement element, ItemsRepeater owner)
		{
			public UIElement Element { get; } = element;
			public ItemsRepeater Owner { get; } = owner;
		}

		internal static readonly BindableProperty ReuseKeyProperty =
			BindableProperty.CreateAttached(
				"ReuseKey", typeof(string), typeof(RecyclePool), string.Empty);

		public static string GetReuseKey(BindableObject bindable)
		{
			return (string)bindable.GetValue(ReuseKeyProperty);
		}

		public static void SetReuseKey(BindableObject bindable, string value)
		{
			bindable.SetValue(ReuseKeyProperty, value);
		}

		private static readonly BindableProperty PoolInstanceProperty =
			BindableProperty.CreateAttached("PoolInstance", typeof(RecyclePool), typeof(Element), null);

		public static RecyclePool GetPoolInstance(Microsoft.Maui.Controls.DataTemplate dataTemplate)
		{
			dataTemplate.Values.TryGetValue(RecyclePool.PoolInstanceProperty, out var poolInstance);
			return (RecyclePool)poolInstance;
		}

		public static void SetPoolInstance(Microsoft.Maui.Controls.DataTemplate dataTemplate, RecyclePool value)
		{
			dataTemplate.SetValue(RecyclePool.PoolInstanceProperty, value);
		}

		internal static readonly BindableProperty OriginTemplateProperty =
			BindableProperty.CreateAttached(
				"OriginTemplate", typeof(Microsoft.Maui.Controls.DataTemplate), typeof(RecyclePool), null);

		private readonly Dictionary<string, List<ElementInfo>> _elements = [];

		public void PutElement(UIElement element, string key)
		{
			PutElementCore(element, key, null);
		}

		public void PutElement(UIElement element, string key, UIElement owner)
		{
			PutElementCore(element, key, owner);
		}

		public UIElement TryGetElement(string key)
		{
			return TryGetElementCore(key, null);
		}

		public UIElement TryGetElement(string key, UIElement owner)
		{
			return TryGetElementCore(key, owner);
		}

		protected virtual void PutElementCore(UIElement element, string key, UIElement owner)
		{
			var winrtKey = key;
			var winrtOwner = owner;
			var winrtOwnerAsPanel = winrtOwner as ItemsRepeater;

			ElementInfo elementInfo = new(element, winrtOwnerAsPanel);

			if (_elements.TryGetValue(winrtKey, out var elements))
			{
				elements.Add(elementInfo);
			}
			else
			{
				List<ElementInfo> pool = [elementInfo];
				_elements.Add(winrtKey, pool);
			}
		}

		protected virtual UIElement TryGetElementCore(
			string key,
			UIElement owner)
		{
			if (_elements.TryGetValue(key, out var elements))
			{
				if (elements.Count > 0)
				{
					ElementInfo elementInfo = new(null, null);

					// Prefer an element from the same owner or with no owner so that we don't incur
					// the enter/leave cost during recycling.
					// TODO: prioritize elements with the same owner to those without an owner.
					var winrtOwner = owner;
					var index = elements.FindIndex(elemInfo => elemInfo.Owner == winrtOwner || elemInfo.Owner == null);

					if (index >= 0)
					{
						elementInfo = elements[index];
						elements.RemoveAt(index); // elements.erase(iter);
					}
					else
					{
						elementInfo = elements.Last();
						elements.RemoveAt(elements.Count - 1);
					}

					return elementInfo.Element;
				}
			}

			return null;
		}
	}
}

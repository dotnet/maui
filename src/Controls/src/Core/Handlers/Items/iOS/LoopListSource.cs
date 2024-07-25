#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal class LoopListSource : ListSource, ILoopItemsViewSource
	{
		public LoopListSource(IEnumerable<object> enumerable, bool loop) : base(enumerable)
		{
			Loop = loop;
		}

		public LoopListSource(IEnumerable enumerable, bool loop)
		{
			Loop = loop;

			foreach (object item in enumerable)
			{
				Add(item);
			}
		}

		public bool Loop { get; set; }

		public int LoopCount => Loop ? Count + 2 : Count;
	}
}

#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal class LoopListSource2 : Items.ListSource, Items.ILoopItemsViewSource
	{
		public LoopListSource2(IEnumerable<object> enumerable, bool loop) : base(enumerable)
		{
			Loop = loop;
		}

		public LoopListSource2(IEnumerable enumerable, bool loop)
		{
			Loop = loop;

			foreach (object item in enumerable)
			{
				Add(item);
			}
		}

		public bool Loop { get; set; }

		public int LoopCount => Loop && Count > 0 ? Count + 2 : Count;
	}
}

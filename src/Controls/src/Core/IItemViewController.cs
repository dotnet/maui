// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
namespace Microsoft.Maui.Controls
{
	public interface IItemViewController
	{
		void BindView(View view, object item);
		View CreateView(object itemType);
		object GetItem(int index);
		object GetItemType(object item);
		int Count
		{
			get;
		}
	}
}
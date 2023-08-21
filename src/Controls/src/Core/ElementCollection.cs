// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	internal class ElementCollection<T> : ObservableWrapper<Element, T> where T : Element
	{
		public ElementCollection(ObservableCollection<Element> list) : base(list)
		{
		}
	}
}
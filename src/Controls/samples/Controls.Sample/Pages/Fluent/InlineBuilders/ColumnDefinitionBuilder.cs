#nullable enable

using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.Fluent
{
	public class ColumnDefinitionBuilder : IEnumerable<ColumnDefinition>
	{
		List<ColumnDefinition> items = new List<ColumnDefinition>();

		public IEnumerator<ColumnDefinition> GetEnumerator() => items.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();

		public ColumnDefinitionBuilder Auto()
		{
			items.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
			return this;
		}

		public ColumnDefinitionBuilder Star(double width = 1)
		{
			items.Add(new ColumnDefinition { Width = new GridLength(width, GridUnitType.Star) });
			return this;
		}

		public ColumnDefinitionBuilder Absolute(double width)
		{
			items.Add(new ColumnDefinition { Width = new GridLength(width, GridUnitType.Absolute) });
			return this;
		}
	}
}

#nullable restore
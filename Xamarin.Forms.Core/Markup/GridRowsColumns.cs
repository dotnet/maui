using System;
using System.Globalization;
using static Xamarin.Forms.Core.Markup.Markup;

namespace Xamarin.Forms.Markup
{
	public static class GridRowsColumns
	{
		public static GridLength Auto => GridLength.Auto;

		public static GridLength Star => GridLength.Star;

		public static class Columns
		{
			public static ColumnDefinitionCollection Define(params GridLength[] widths)
			{
				VerifyExperimental();
				var columnDefinitions = new ColumnDefinitionCollection();

				for (int i = 0; i < widths.Length; i++)
					columnDefinitions.Add(new ColumnDefinition { Width = widths[i] });

				return columnDefinitions;
			}

			public static ColumnDefinitionCollection Define<TEnum>(params (TEnum name, GridLength width)[] columns) where TEnum : Enum
			{
				VerifyExperimental();
				var columnDefinitions = new ColumnDefinitionCollection();
				for (int i = 0; i < columns.Length; i++)
				{
					if (i != columns[i].name.ToInt())
						throw new ArgumentException(
							$"Value of column name { columns[i].name } is not { i }. " +
							"Columns must be defined with enum names whose values form the sequence 0,1,2,..."
						);
					columnDefinitions.Add(new ColumnDefinition { Width = columns[i].width });
				}
				return columnDefinitions;
			}
		}

		public static class Rows
		{
			public static RowDefinitionCollection Define(params GridLength[] heights)
			{
				VerifyExperimental();
				var rowDefinitions = new RowDefinitionCollection();

				for (int i = 0; i < heights.Length; i++)
					rowDefinitions.Add(new RowDefinition { Height = heights[i] });

				return rowDefinitions;
			}

			public static RowDefinitionCollection Define<TEnum>(params (TEnum name, GridLength height)[] rows) where TEnum : Enum
			{
				VerifyExperimental();
				var rowDefinitions = new RowDefinitionCollection();
				for (int i = 0; i < rows.Length; i++)
				{
					if (i != rows[i].name.ToInt())
						throw new ArgumentException(
							$"Value of row name { rows[i].name } is not { i }. " +
							"Rows must be defined with enum names whose values form the sequence 0,1,2,..."
						);
					rowDefinitions.Add(new RowDefinition { Height = rows[i].height });
				}
				return rowDefinitions;
			}
		}

		public static int All<TEnum>() where TEnum : Enum
		{
			VerifyExperimental();
			var values = Enum.GetValues(typeof(TEnum));
			int span = (int)values.GetValue(values.Length - 1) + 1;
			return span;
		}

		public static int Last<TEnum>() where TEnum : Enum
		{
			VerifyExperimental();
			var values = Enum.GetValues(typeof(TEnum));
			int last = (int)values.GetValue(values.Length - 1);
			return last;
		}

		static int ToInt(this Enum enumValue) => Convert.ToInt32(enumValue, CultureInfo.InvariantCulture);
	}
}
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Controls.Sample.UITests.Elements
{
    public partial class ScrollViewLayout : ContentPage
    {
        public ScrollViewLayout()
        {
            InitializeComponent();
        }
	}

	public class NamedColor : IEquatable<NamedColor>, IComparable<NamedColor>
	{
		public static IList<NamedColor> All { get; private set; }

		public string Name { get; private set; }

		public string FriendlyName { get; private set; }

		public Color Color { private set; get; }

		public bool Equals(NamedColor other)
		{
			return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
		}

		public int CompareTo(NamedColor other)
		{
			return Name.CompareTo(other.Name);
		}

		// Static members
		static NamedColor()
		{
			List<NamedColor> all = new List<NamedColor>();
			StringBuilder stringBuilder = new StringBuilder();

			// Loop through the public static fields of the Color structure.
			foreach (FieldInfo fieldInfo in typeof(Color).GetRuntimeFields())
			{
				if (fieldInfo.IsPublic &&
					fieldInfo.IsStatic &&
					fieldInfo.FieldType == typeof(Color))
				{
					// Convert the name to a friendly name.
					string name = fieldInfo.Name;
					stringBuilder.Clear();
					int index = 0;

					foreach (char ch in name)
					{
						if (index != 0 && Char.IsUpper(ch))
						{
							stringBuilder.Append(' ');
						}
						stringBuilder.Append(ch);
						index++;
					}

					// Instantiate a NamedColor object.
					Color color = (Color)fieldInfo.GetValue(null);

					NamedColor namedColor = new NamedColor
					{
						Name = name,
						FriendlyName = stringBuilder.ToString(),
						Color = color
					};

					// Add it to the collection.
					all.Add(namedColor);
				}
			}
			all.TrimExcess();
			all.Sort();
			All = all;
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh3539ViewModel : INotifyPropertyChanged
	{
		Color color;
		string name;

		public event PropertyChangedEventHandler PropertyChanged;

		public double Hue
		{
			set
			{
				if (color.GetHue() != value)
				{
					Color = Color.FromHsla(value, color.GetSaturation(), color.GetLuminosity());
				}
			}
			get
			{
				return color.GetHue();
			}
		}

		public double Saturation
		{
			set
			{
				if (color.GetSaturation() != value)
				{
					Color = Color.FromHsla(color.GetHue(), value, color.GetLuminosity());
				}
			}
			get
			{
				return color.GetSaturation();
			}
		}

		public double Luminosity
		{
			set
			{
				if (color.GetLuminosity() != value)
				{
					Color = Color.FromHsla(color.GetHue(), color.GetSaturation(), value);
				}
			}
			get
			{
				return color.GetLuminosity();
			}
		}

		public Color Color
		{
			set
			{
				if (color != value)
				{
					color = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Hue"));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Saturation"));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Luminosity"));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Color"));

					Name = Gh3539NamedColor.GetNearestColorName(color);
				}
			}
			get
			{
				return color;
			}
		}

		public string Name
		{
			private set
			{
				if (name != value)
				{
					name = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
				}
			}
			get
			{
				return name;
			}
		}
	}

	public class Gh3539NamedColor : IEquatable<Gh3539NamedColor>, IComparable<Gh3539NamedColor>
	{
		// Instance members
		private Gh3539NamedColor()
		{
		}

		public string Name { private set; get; }

		public string FriendlyName { private set; get; }

		public Color Color { private set; get; }

		public string RgbDisplay { private set; get; }

		public bool Equals(Gh3539NamedColor other)
		{
			return Name.Equals(other.Name);
		}

		public int CompareTo(Gh3539NamedColor other)
		{
			return Name.CompareTo(other.Name);
		}

		// Static members
		static Gh3539NamedColor()
		{
			List<Gh3539NamedColor> all = new List<Gh3539NamedColor>();
			StringBuilder stringBuilder = new StringBuilder();

			// Loop through the public static fields of the Color structure.
			foreach (FieldInfo fieldInfo in typeof(Colors).GetRuntimeFields())
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

					Gh3539NamedColor namedColor = new Gh3539NamedColor
					{
						Name = name,
						FriendlyName = stringBuilder.ToString(),
						Color = color,
						RgbDisplay = String.Format("{0:X2}-{1:X2}-{2:X2}",
												   (int)(255 * color.Red),
												   (int)(255 * color.Green),
												   (int)(255 * color.Blue))
					};

					// Add it to the collection.
					all.Add(namedColor);
				}
			}
			all.TrimExcess();
			all.Sort();
			All = all;
		}

		public static IList<Gh3539NamedColor> All { private set; get; }

		public static Gh3539NamedColor Find(string name)
		{
			return ((List<Gh3539NamedColor>)All).Find(nc => nc.Name == name);
		}

		public static string GetNearestColorName(Color color)
		{
			double shortestDistance = 1000;
			Gh3539NamedColor closestColor = null;

			foreach (Gh3539NamedColor namedColor in Gh3539NamedColor.All)
			{
				double distance = Math.Sqrt(Math.Pow(color.Red - namedColor.Color.Red, 2) +
											Math.Pow(color.Green - namedColor.Color.Green, 2) +
											Math.Pow(color.Blue - namedColor.Color.Blue, 2));

				if (distance < shortestDistance)
				{
					shortestDistance = distance;
					closestColor = namedColor;
				}
			}
			return closestColor.Name;
		}
	}

	public partial class Gh3539 : ContentPage
	{
		public Gh3539()
		{
			InitializeComponent();
		}

		public Gh3539(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}


		[TestFixture]
		class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(true)]
			public void CompiledBindingCodeIsValid(bool useCompiledXaml)
			{
				var layout = new Gh3539(useCompiledXaml);
			}
		}
	}
}

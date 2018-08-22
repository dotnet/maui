using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Gh3539ViewModel : INotifyPropertyChanged
	{
		Color color;
		string name;

		public event PropertyChangedEventHandler PropertyChanged;

		public double Hue {
			set {
				if (color.Hue != value) {
					Color = Color.FromHsla(value, color.Saturation, color.Luminosity);
				}
			}
			get {
				return color.Hue;
			}
		}

		public double Saturation {
			set {
				if (color.Saturation != value) {
					Color = Color.FromHsla(color.Hue, value, color.Luminosity);
				}
			}
			get {
				return color.Saturation;
			}
		}

		public double Luminosity {
			set {
				if (color.Luminosity != value) {
					Color = Color.FromHsla(color.Hue, color.Saturation, value);
				}
			}
			get {
				return color.Luminosity;
			}
		}

		public Color Color {
			set {
				if (color != value) {
					color = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Hue"));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Saturation"));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Luminosity"));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Color"));

					Name = Gh3539NamedColor.GetNearestColorName(color);
				}
			}
			get {
				return color;
			}
		}

		public string Name {
			private set {
				if (name != value) {
					name = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
				}
			}
			get {
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
			foreach (FieldInfo fieldInfo in typeof(Color).GetRuntimeFields()) {
				if (fieldInfo.IsPublic &&
					fieldInfo.IsStatic &&
					fieldInfo.FieldType == typeof(Color)) {
					// Convert the name to a friendly name.
					string name = fieldInfo.Name;
					stringBuilder.Clear();
					int index = 0;

					foreach (char ch in name) {
						if (index != 0 && Char.IsUpper(ch)) {
							stringBuilder.Append(' ');
						}
						stringBuilder.Append(ch);
						index++;
					}

					// Instantiate a NamedColor object.
					Color color = (Color)fieldInfo.GetValue(null);

					Gh3539NamedColor namedColor = new Gh3539NamedColor {
						Name = name,
						FriendlyName = stringBuilder.ToString(),
						Color = color,
						RgbDisplay = String.Format("{0:X2}-{1:X2}-{2:X2}",
												   (int)(255 * color.R),
												   (int)(255 * color.G),
												   (int)(255 * color.B))
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

			foreach (Gh3539NamedColor namedColor in Gh3539NamedColor.All) {
				double distance = Math.Sqrt(Math.Pow(color.R - namedColor.Color.R, 2) +
											Math.Pow(color.G - namedColor.Color.G, 2) +
											Math.Pow(color.B - namedColor.Color.B, 2));

				if (distance < shortestDistance) {
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

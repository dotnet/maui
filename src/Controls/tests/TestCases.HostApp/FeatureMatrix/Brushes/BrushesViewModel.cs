using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
namespace Maui.Controls.Sample;

public class BrushesViewModel : INotifyPropertyChanged
{
	private Brush brushTarget = new SolidColorBrush(Colors.Transparent);
	private Brush strokeBrush = new SolidColorBrush(Colors.Transparent);
	private Shadow shadowBrush;
	private double brushOpacity = 1.0;
	private double linearStartX = 0.0;
	private double linearStartY = 0.0;
	private double linearEndX = 1.0;
	private double linearEndY = 0.0;
	private double radialCenterX = 0.5;
	private double radialCenterY = 0.5;
	private double radialRadius = 0.5;
	public ICommand ApplySolidBrushCommand { get; }
	public ICommand ApplyLinearGradientCommand { get; }
	public ICommand ApplyRadialGradientCommand { get; }
	public ICommand ApplyNullBrushCommand { get; }
	public ICommand ApplySolidShadowCommand { get; }
	public ICommand ApplyLinearShadowCommand { get; }
	public ICommand ApplyRadialShadowCommand { get; }
	public ICommand ApplySolidStrokeCommand { get; }
	public ICommand ApplyLinearStrokeCommand { get; }
	public ICommand ApplyRadialStrokeCommand { get; }
	public ICommand ApplyNullStrokeCommand { get; }
	public ICommand ApplyAltSolidBrushCommand { get; }
	public ICommand ApplyAltLinearBrushCommand { get; }
	public ICommand ApplyAltRadialBrushCommand { get; }
	public ICommand CompareBrushesCommand { get; }

	private string selectedColorName1 = "None";
	private string selectedColorName2 = "None";
	private string compareResult;

	public string SelectedColorName1 { get => selectedColorName1; set { if (selectedColorName1 == value) return; selectedColorName1 = value; OnPropertyChanged(); } }
	public string SelectedColorName2 { get => selectedColorName2; set { if (selectedColorName2 == value) return; selectedColorName2 = value; OnPropertyChanged(); } }
	public string CompareResult { get => compareResult; private set { if (compareResult == value) return; compareResult = value; OnPropertyChanged(); } }
	public event PropertyChangedEventHandler PropertyChanged;

	public Brush BrushTarget
	{
		get => brushTarget;
		set
		{
			if (brushTarget == value)
				return;
			brushTarget = value;
			OnPropertyChanged();
			UpdateDerivedProperties();
		}
	}


	public Brush StrokeBrush
	{
		get => strokeBrush;
		private set { if (strokeBrush == value) return; strokeBrush = value; OnPropertyChanged(); }
	}

	public Shadow ShadowBrush
	{
		get => shadowBrush;
		set
		{
			if (shadowBrush == value)
				return;
			shadowBrush = value;
			OnPropertyChanged();
		}
	}

	public double BrushOpacity
	{
		get => brushOpacity;
		set
		{
			var clamped = Math.Clamp(value, 0.0, 1.0);
			if (Math.Abs(brushOpacity - clamped) < double.Epsilon)
				return;
			brushOpacity = clamped;
			RebuildBrushWithOpacity();
			OnPropertyChanged();
		}
	}

	public double LinearStartX { get => linearStartX; set { if (Math.Abs(linearStartX - value) < double.Epsilon) return; linearStartX = value; OnPropertyChanged(); ApplyPointChanges(); } }
	public double LinearStartY { get => linearStartY; set { if (Math.Abs(linearStartY - value) < double.Epsilon) return; linearStartY = value; OnPropertyChanged(); ApplyPointChanges(); } }
	public double LinearEndX { get => linearEndX; set { if (Math.Abs(linearEndX - value) < double.Epsilon) return; linearEndX = value; OnPropertyChanged(); ApplyPointChanges(); } }
	public double LinearEndY { get => linearEndY; set { if (Math.Abs(linearEndY - value) < double.Epsilon) return; linearEndY = value; OnPropertyChanged(); ApplyPointChanges(); } }
	public double RadialCenterX { get => radialCenterX; set { if (Math.Abs(radialCenterX - value) < double.Epsilon) return; radialCenterX = value; OnPropertyChanged(); ApplyPointChanges(); } }
	public double RadialCenterY { get => radialCenterY; set { if (Math.Abs(radialCenterY - value) < double.Epsilon) return; radialCenterY = value; OnPropertyChanged(); ApplyPointChanges(); } }
	public double RadialRadius { get => radialRadius; set { var clamped = Math.Clamp(value, 0.0, 1.0); if (Math.Abs(radialRadius - clamped) < double.Epsilon) return; radialRadius = clamped; OnPropertyChanged(); ApplyPointChanges(); } }

	public BrushesViewModel()
	{
		ShadowBrush = BuildShadow(new SolidColorBrush(Colors.Transparent));

		ApplySolidBrushCommand = new Command(() =>
		{
			BrushTarget = BuildSolid(Colors.LightSalmon);
		});

		ApplyLinearGradientCommand = new Command(() =>
		{
			var start = new Point(0, 0);
			var end = new Point(1, 0);
			BrushTarget = BuildLinear(
				new GradientStopCollection
				{
					new GradientStop(Colors.SkyBlue, 0.0f),
					new GradientStop(Colors.LightGreen, 1.0f)
				},
				start,
				end);
			LinearStartX = start.X;
			LinearStartY = start.Y;
			LinearEndX = end.X;
			LinearEndY = end.Y;
		});

		ApplyRadialGradientCommand = new Command(() =>
		{
			var center = new Point(0.5, 0.5);
			var radius = 0.5;
			BrushTarget = BuildRadial(
				new GradientStopCollection
				{
					new GradientStop(Colors.IndianRed, 0.1f),
					new GradientStop(Colors.LightPink, 1.0f)
				},
				center,
				radius);
			RadialCenterX = center.X;
			RadialCenterY = center.Y;
			RadialRadius = radius;
		});

		ApplyNullBrushCommand = new Command(() => BrushTarget = null);

		ApplySolidShadowCommand = new Command(() =>
{
	ShadowBrush = new Shadow
	{
		Brush = BuildSolid(Colors.DarkRed),
		Radius = 40,
		Opacity = 1.0f,
		Offset = new Point(0, 6)
	};
});

		ApplyLinearShadowCommand = new Command(() =>
		{
			var start = new Point(0, 0);
			var end = new Point(1, 0);

			var linear = BuildLinear(
				new GradientStopCollection
				{
			new GradientStop(Colors.DarkOrange, 0.45f),
			new GradientStop(Colors.DarkSlateBlue, 0.55f)
				},
				start,
				end);

			ShadowBrush = new Shadow
			{
				Brush = linear,
				Radius = 40,
				Opacity = 1.0f,
				Offset = new Point(0, 6)
			};

			LinearStartX = start.X;
			LinearStartY = start.Y;
			LinearEndX = end.X;
			LinearEndY = end.Y;
		});


		ApplyRadialShadowCommand = new Command(() =>
			{
				var center = new Point(0.5, 0.5);
				var radius = 1.0;

				var stops = new GradientStopCollection
				{
					new GradientStop(Colors.Red, 0.5f),
					new GradientStop(Colors.Blue, 0.5f),
				};

				var radial = BuildRadial(stops, center, radius);

				ShadowBrush = new Shadow
				{
					Brush = radial,
					Radius = 40,
					Opacity = 1.0f,
					Offset = new Point(0, 6)
				};

				RadialCenterX = center.X;
				RadialCenterY = center.Y;
				RadialRadius = radius;
			});

		ApplySolidStrokeCommand = new Command(() =>
		{
			StrokeBrush = BuildSolid(Colors.DarkSlateGray);
		});
		ApplyLinearStrokeCommand = new Command(() =>
		{
			var start = new Point(0, 0);
			var end = new Point(1, 0);
			StrokeBrush = BuildLinear(
				new GradientStopCollection
				{
					new GradientStop(Colors.Gold, 0.0f),
					new GradientStop(Colors.OrangeRed, 1.0f)
				},
				start,
				end);
			LinearStartX = start.X;
			LinearStartY = start.Y;
			LinearEndX = end.X;
			LinearEndY = end.Y;
		});

		ApplyRadialStrokeCommand = new Command(() =>
		{
			var center = new Point(0.1, 0.5);
			var radius = 0.5;
			StrokeBrush = BuildRadial(
				new GradientStopCollection
				{
					new GradientStop(Colors.AliceBlue, 0.0f),
					new GradientStop(Colors.LightSkyBlue, 0.4f),
					new GradientStop(Colors.DeepSkyBlue, 0.75f)
				},
				center,
				radius);
			RadialCenterX = center.X;
			RadialCenterY = center.Y;
			RadialRadius = radius;
		});
		ApplyNullStrokeCommand = new Command(() => StrokeBrush = null);
		ApplyAltSolidBrushCommand = new Command(() =>
		{
			BrushTarget = BuildSolid(Colors.LightSteelBlue);
		});

		ApplyAltLinearBrushCommand = new Command(() =>
		{
			var start = new Point(0, 0);
			var end = new Point(1, 0);
			BrushTarget = BuildLinear(
				new GradientStopCollection
				{
					new GradientStop(Colors.LightSkyBlue, 0.35f),
					new GradientStop(Colors.SkyBlue, 0.7f),
					new GradientStop(Colors.AliceBlue, 1.0f)
				},
				start,
				end);
			LinearStartX = start.X;
			LinearStartY = start.Y;
			LinearEndX = end.X;
			LinearEndY = end.Y;
		});

		ApplyAltRadialBrushCommand = new Command(() =>
		{
			var center = new Point(0.5, 0.5);
			var radius = 0.55;
			BrushTarget = BuildRadial(
				new GradientStopCollection
				{
					new GradientStop(Colors.AliceBlue, 0.0f),
					new GradientStop(Colors.LightSkyBlue, 0.5f),
					new GradientStop(Colors.SkyBlue, 1.0f)
				},
				center,
				radius);
			RadialCenterX = center.X;
			RadialCenterY = center.Y;
			RadialRadius = radius;
		});

		CompareBrushesCommand = new Command(() =>
		{
			var brush1 = BuildSolid(ColorFromName(SelectedColorName1) ?? Colors.Transparent) as Brush;
			var brush2 = BuildSolid(ColorFromName(SelectedColorName2) ?? Colors.Transparent) as Brush;
			bool equal = AreBrushesEqual(brush1, brush2);
			int hash1 = GetBrushHashCode(brush1);
			int hash2 = GetBrushHashCode(brush2);
			bool equalHash = hash1 == hash2;
			CompareResult = equal && equalHash
				? $"True"
				: $"False";
		});
	}

	void ApplyPointChanges()
	{
		if (BrushTarget is LinearGradientBrush lgb)
		{
			BrushTarget = BuildLinear(lgb.GradientStops, new Point(LinearStartX, LinearStartY), new Point(LinearEndX, LinearEndY));
		}
		else if (BrushTarget is RadialGradientBrush rgb)
		{
			BrushTarget = BuildRadial(rgb.GradientStops, new Point(RadialCenterX, RadialCenterY), RadialRadius);
		}

		if (StrokeBrush is LinearGradientBrush slgb)
		{
			StrokeBrush = BuildLinear(slgb.GradientStops, new Point(LinearStartX, LinearStartY), new Point(LinearEndX, LinearEndY));
		}
		else if (StrokeBrush is RadialGradientBrush srgb)
		{
			StrokeBrush = BuildRadial(srgb.GradientStops, new Point(RadialCenterX, RadialCenterY), RadialRadius);
		}

		if (ShadowBrush?.Brush is LinearGradientBrush shlgb)
		{
			ShadowBrush = RebuildShadow(BuildLinear(shlgb.GradientStops, new Point(LinearStartX, LinearStartY), new Point(LinearEndX, LinearEndY)));
		}
		else if (ShadowBrush?.Brush is RadialGradientBrush shrgb)
		{
			ShadowBrush = RebuildShadow(BuildRadial(shrgb.GradientStops, new Point(RadialCenterX, RadialCenterY), RadialRadius));
		}
	}

	void RebuildBrushWithOpacity()
	{
		if (BrushTarget == null || Microsoft.Maui.Controls.Brush.IsNullOrEmpty(BrushTarget))
			return;

		switch (BrushTarget)
		{
			case SolidColorBrush scb:
				BrushTarget = BuildSolid(scb.Color);
				break;
			case LinearGradientBrush lgb:
				BrushTarget = BuildLinear(lgb.GradientStops, lgb.StartPoint, lgb.EndPoint);
				break;
			case RadialGradientBrush rgb:
				BrushTarget = BuildRadial(rgb.GradientStops, rgb.Center, rgb.Radius);
				break;
		}
	}

	void UpdateDerivedProperties()
	{
		bool nowEmpty = BrushTarget == null || Microsoft.Maui.Controls.Brush.IsNullOrEmpty(BrushTarget);
		if (nowEmpty)
		{
			StrokeBrush = new SolidColorBrush(Colors.Transparent);
			ShadowBrush = BuildShadow(new SolidColorBrush(Colors.Transparent));
		}
	}

	SolidColorBrush BuildSolid(Color color) => new(color.WithAlpha((float)brushOpacity));

	LinearGradientBrush BuildLinear(GradientStopCollection stops, Point startPoint, Point endPoint)
	{
		var newStops = new GradientStopCollection();
		foreach (var gs in stops)
			newStops.Add(new GradientStop(gs.Color.WithAlpha((float)brushOpacity), gs.Offset));
		return new LinearGradientBrush(newStops, startPoint, endPoint);
	}

	RadialGradientBrush BuildRadial(GradientStopCollection stops, Point center, double radius)
	{
		var newStops = new GradientStopCollection();
		foreach (var gs in stops)
			newStops.Add(new GradientStop(gs.Color.WithAlpha((float)brushOpacity), gs.Offset));
		var r = Math.Clamp(radius, 0.0, 1.0);
		return new RadialGradientBrush(newStops, center, (float)r);
	}

	Shadow BuildShadow(Brush source) => new Shadow
	{
		Brush = source,
		Radius = 50,
		Opacity = 0.8f,
		Offset = new Point(10, 10)
	};

	Shadow RebuildShadow(Brush brush) =>
		new Shadow
		{
			Brush = brush,
			Radius = ShadowBrush?.Radius ?? 40,
			Opacity = ShadowBrush?.Opacity ?? 1.0f,
			Offset = ShadowBrush?.Offset ?? new Point(0, 6)
		};

	bool AreBrushesEqual(Brush a, Brush b)
	{
		if (ReferenceEquals(a, b))
			return true;
		if (a == null || b == null)
			return false;
		if (a.GetType() != b.GetType())
			return false;

		if (a is SolidColorBrush sa && b is SolidColorBrush sb)
			return sa.Color == sb.Color;

		if (a is LinearGradientBrush la && b is LinearGradientBrush lb)
		{
			if (!la.StartPoint.Equals(lb.StartPoint) || !la.EndPoint.Equals(lb.EndPoint))
				return false;
			var s1 = la.GradientStops;
			var s2 = lb.GradientStops;
			if (s1.Count != s2.Count)
				return false;
			for (int i = 0; i < s1.Count; i++)
				if (!s1[i].Equals(s2[i]))
					return false;
			return true;
		}

		if (a is RadialGradientBrush ra && b is RadialGradientBrush rb)
		{
			if (!ra.Center.Equals(rb.Center))
				return false;
			if (Math.Abs(ra.Radius - rb.Radius) > 1e-6)
				return false;
			var s1 = ra.GradientStops;
			var s2 = rb.GradientStops;
			if (s1.Count != s2.Count)
				return false;
			for (int i = 0; i < s1.Count; i++)
				if (!s1[i].Equals(s2[i]))
					return false;
			return true;
		}

		return a.Equals(b);
	}

	int GetBrushHashCode(Brush b)
	{
		if (b == null)
			return 0;
		unchecked
		{
			int hash = 17;
			hash = hash * 31 + b.GetType().GetHashCode();
			if (b is SolidColorBrush scb)
				hash = hash * 31 + (scb.Color.GetHashCode());
			else if (b is LinearGradientBrush lgb)
			{
				hash = hash * 31 + lgb.StartPoint.GetHashCode();
				hash = hash * 31 + lgb.EndPoint.GetHashCode();
				foreach (var gs in lgb.GradientStops)
					hash = hash * 31 + (gs?.GetHashCode() ?? 0);
			}
			else if (b is RadialGradientBrush rgb)
			{
				hash = hash * 31 + rgb.Center.GetHashCode();
				hash = hash * 31 + rgb.Radius.GetHashCode();
				foreach (var gs in rgb.GradientStops)
					hash = hash * 31 + (gs?.GetHashCode() ?? 0);
			}
			else
				hash = hash * 31 + b.GetHashCode();
			return hash;
		}
	}

	Color ColorFromName(string name)
	{
		if (string.IsNullOrEmpty(name))
			return Colors.Transparent;
		switch (name.Trim().ToLowerInvariant())
		{
			case "red":
				return Colors.Red;
			case "green":
				return Colors.Green;
			case "blue":
				return Colors.Blue;
			default:
				return Colors.Transparent;
		}
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class SafeAreaViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	private SafeAreaRegions _leftEdge = SafeAreaRegions.None;
	private SafeAreaRegions _topEdge = SafeAreaRegions.None;
	private SafeAreaRegions _rightEdge = SafeAreaRegions.None;
	private SafeAreaRegions _bottomEdge = SafeAreaRegions.None;
	private Thickness _padding = new Thickness(0);
	private Brush _background = Brush.White;

	public SafeAreaRegions LeftEdge
	{
		get => _leftEdge;
		set
		{
			if (_leftEdge != value)
			{
				_leftEdge = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(EffectiveSafeAreaEdges));
				OnPropertyChanged(nameof(DisplayString));
			}
		}
	}

	public SafeAreaRegions TopEdge
	{
		get => _topEdge;
		set
		{
			if (_topEdge != value)
			{
				_topEdge = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(EffectiveSafeAreaEdges));
				OnPropertyChanged(nameof(DisplayString));
			}
		}
	}

	public SafeAreaRegions RightEdge
	{
		get => _rightEdge;
		set
		{
			if (_rightEdge != value)
			{
				_rightEdge = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(EffectiveSafeAreaEdges));
				OnPropertyChanged(nameof(DisplayString));
			}
		}
	}

	public SafeAreaRegions BottomEdge
	{
		get => _bottomEdge;
		set
		{
			if (_bottomEdge != value)
			{
				_bottomEdge = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(EffectiveSafeAreaEdges));
				OnPropertyChanged(nameof(DisplayString));
			}
		}
	}

	public Thickness Padding
	{
		get => _padding;
		set
		{
			if (_padding != value)
			{
				_padding = value;
				OnPropertyChanged();
			}
		}
	}

	public Brush Background
	{
		get => _background;
		set
		{
			if (_background != value)
			{
				_background = value;
				OnPropertyChanged();
			}
		}
	}

	/// <summary>
	/// Computed property: the effective SafeAreaEdges based on uniform vs per-edge mode.
	/// Bind ContentPage.SafeAreaEdges to this property.
	/// </summary>
	public SafeAreaEdges EffectiveSafeAreaEdges
	{
		get => new SafeAreaEdges(LeftEdge, TopEdge, RightEdge, BottomEdge);
	}

	/// <summary>
	/// Computed property: display string describing the current SafeAreaEdges configuration.
	/// </summary>
	public string DisplayString
	{
		get
		{
			if (LeftEdge == TopEdge && TopEdge == RightEdge && RightEdge == BottomEdge)
				return $"{LeftEdge}";

			return $"L:{LeftEdge}, T:{TopEdge}, R:{RightEdge}, B:{BottomEdge}";
		}
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

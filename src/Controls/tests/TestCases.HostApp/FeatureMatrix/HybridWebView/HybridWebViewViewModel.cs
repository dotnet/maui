using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public class HybridWebViewViewModel : INotifyPropertyChanged
{
	private string _defaultFile = "index.html";
	private string _hybridRoot = "HybridWebView1";
	private string _status = "Ready";
	private bool _isVisible = true;
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;
	private Shadow _shadow = null;
	private bool _hasShadow = false;
	private bool _isLeftToRight = true;


	public HybridWebViewViewModel()
	{
		DefaultFile = "index.html";
		HybridRoot = "HybridWebView1";
		Status = "Ready";
		IsVisible = true;
		HasShadow = false;
		IsLeftToRight = true;
	}
	public string DefaultFile
	{
		get => _defaultFile;
		set
		{
			if (_defaultFile != value)
			{
				_defaultFile = value;
				OnPropertyChanged();
			}
		}
	}

	public string HybridRoot
	{
		get => _hybridRoot;
		set
		{
			if (_hybridRoot != value)
			{
				_hybridRoot = value;
				OnPropertyChanged();
			}
		}
	}

	public FlowDirection FlowDirection
	{
		get => _flowDirection;
		set
		{
			if (_flowDirection != value)
			{
				_flowDirection = value;
				OnPropertyChanged();
			}
		}
	}


	public bool IsLeftToRight
	{
		get => _isLeftToRight;
		set
		{
			if (_isLeftToRight != value)
			{
				_isLeftToRight = value;
				OnPropertyChanged(nameof(IsLeftToRight));
			}
		}
	}
	public string Status
	{
		get => _status;
		set
		{
			if (_status != value)
			{
				_status = value;
				OnPropertyChanged();
			}
		}
	}

	public bool IsVisible
	{
		get => _isVisible;
		set
		{
			if (_isVisible != value)
			{
				_isVisible = value;
				OnPropertyChanged();
			}
		}
	}

	public Shadow Shadow
	{
		get => _shadow;
		set
		{
			if (_shadow != value)
			{
				_shadow = value;
				OnPropertyChanged();
			}
		}
	}

	public bool HasShadow
	{
		get => _hasShadow;
		set
		{
			if (_hasShadow != value)
			{
				_hasShadow = value;
				Shadow = value
					? new Shadow
					{
						Radius = 10,
						Opacity = 1.0f,
						Brush = Colors.Black.AsPaint(),
						Offset = new Point(5, 5)
					}
					: null;
				OnPropertyChanged(nameof(HasShadow));
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
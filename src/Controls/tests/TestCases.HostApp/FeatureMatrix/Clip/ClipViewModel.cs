using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;
namespace Maui.Controls.Sample;

public class ClipViewModel : INotifyPropertyChanged
{
	private Geometry _clip = null;
	private string _selectedControl = "Image";
	private string _selectedClip = "null";

	public Geometry Clip
	{
		get => _clip;
		set
		{
			if (_clip != value)
			{
				_clip = value;
				OnPropertyChanged();
			}
		}
	}
	public string SelectedControl
	{
		get => _selectedControl;
		set
		{
			if (_selectedControl != value)
			{
				_selectedControl = value;
				OnPropertyChanged();
			}
		}
	}

	public string SelectedClip
	{
		get => _selectedClip;
		set
		{
			if (_selectedClip != value)
			{
				_selectedClip = value;
				OnPropertyChanged();
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
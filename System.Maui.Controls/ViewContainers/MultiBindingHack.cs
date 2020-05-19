using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers = true)]
	internal class MultiBindingHack : INotifyPropertyChanged
	{
		string _labelWithBounds;

		public MultiBindingHack (VisualElement element)
		{
			LabelWithBounds = string.Format("{{X={0:0.00} Y={1:0.00} Width={2:0.00} Height={3:0.00}}}", element.X, element.Y, element.Width, element.Height);

			element.PropertyChanged += (sender, args) => {
				if (args.PropertyName == "X" || 
					args.PropertyName == "Y" || 
					args.PropertyName == "Width" || 
					args.PropertyName == "Height" || 
					args.PropertyName == "Rotation") {
					LabelWithBounds = string.Format("{{X={0:0.00} Y={1:0.00} Width={2:0.00} Height={3:0.00}}}", element.X, element.Y, element.Width, element.Height); // super hack
				}
			};
		}

		public string LabelWithBounds
		{
			get { return _labelWithBounds; }
			set
			{
				if (_labelWithBounds == value)
					return;
				_labelWithBounds = value;
				OnPropertyChanged ();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged ([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler (this, new PropertyChangedEventArgs (propertyName));
		}
	}
}
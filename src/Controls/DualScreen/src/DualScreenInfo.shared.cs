using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.DualScreen
{
	public class HingeAngleChangedEventArgs : EventArgs
	{
		public HingeAngleChangedEventArgs(double hingeAngleInDegrees)
		{
			HingeAngleInDegrees = hingeAngleInDegrees;
		}

		public double HingeAngleInDegrees { get; }
	}

	public partial class DualScreenInfo : INotifyPropertyChanged
	{
		Rectangle[] _spanningBounds;
		Rectangle _hingeBounds;
		bool _isLandscape;
		TwoPaneViewMode _spanMode;
		TwoPaneViewLayoutGuide _twoPaneViewLayoutGuide;
		IDualScreenService _dualScreenService;		
		IDualScreenService DualScreenService =>
			_dualScreenService ?? DependencyService.Get<IDualScreenService>() ?? NoDualScreenServiceImpl.Instance;

		internal VisualElement Element { get; }

		static Lazy<DualScreenInfo> _dualScreenInfo { get; } = new Lazy<DualScreenInfo>(OnCreate);

		public static DualScreenInfo Current => _dualScreenInfo.Value;
		public event PropertyChangedEventHandler PropertyChanged;

		public DualScreenInfo(VisualElement element) : this(element, null)
		{
		}

		internal DualScreenInfo(VisualElement element, IDualScreenService dualScreenService)
		{
			_spanningBounds = new Rectangle[0];
			Element = element;
			_dualScreenService = dualScreenService;

			if (element == null)
			{
				_twoPaneViewLayoutGuide = TwoPaneViewLayoutGuide.Instance;
			}
			else
			{
				_twoPaneViewLayoutGuide = new TwoPaneViewLayoutGuide(element, dualScreenService);
				_twoPaneViewLayoutGuide.PropertyChanged += OnTwoPaneViewLayoutGuideChanged;				
			}
		}


		EventHandler<HingeAngleChangedEventArgs> _hingeAngleChanged;
		int subscriberCount = 0;
		public event EventHandler<HingeAngleChangedEventArgs> HingeAngleChanged
		{
			add
			{
				ProcessHingeAngleSubscriberCount(Interlocked.Increment(ref subscriberCount));
				_hingeAngleChanged += value;
			}
			remove
			{
				ProcessHingeAngleSubscriberCount(Interlocked.Decrement(ref subscriberCount));
				_hingeAngleChanged -= value;
			}
		}


		public Rectangle[] SpanningBounds
		{
			get => GetSpanningBounds();
			set
			{
				if (_spanningBounds == null && value == null)
					return;

				if (_spanningBounds == null ||
					value == null ||
					!Enumerable.SequenceEqual(_spanningBounds, value))
				{
					SetProperty(ref _spanningBounds, value);
				}
			}
		}

		public Rectangle HingeBounds
		{
			get => GetHingeBounds();
			set
			{
				SetProperty(ref _hingeBounds, value);
			}
		}

		public bool IsLandscape
		{
			get => GetIsLandscape();
			set
			{
				SetProperty(ref _isLandscape, value);
			}
		}

		public TwoPaneViewMode SpanMode
		{
			get => GetSpanMode();
			set
			{
				SetProperty(ref _spanMode, value);
			}
		}

		Rectangle[] GetSpanningBounds()
		{
			var guide = _twoPaneViewLayoutGuide;
			var hinge = guide.Hinge;

			if (hinge == Rectangle.Zero)
				return new Rectangle[0];

			if (guide.Pane2 == Rectangle.Zero)
				return new Rectangle[0];

			if(IsLandscape)
				return new[] { guide.Pane1, new Rectangle(0, hinge.Height + guide.Pane1.Height, guide.Pane2.Width, guide.Pane2.Height) };
			else
				return new[] { guide.Pane1, new Rectangle(hinge.Width + guide.Pane1.Width, 0, guide.Pane2.Width, guide.Pane2.Height) };
		}

		Rectangle GetHingeBounds()
		{
			var guide = _twoPaneViewLayoutGuide;
			return guide.Hinge;
		}

		bool GetIsLandscape() => _twoPaneViewLayoutGuide.IsLandscape;

		TwoPaneViewMode GetSpanMode() => _twoPaneViewLayoutGuide.Mode;

		static DualScreenInfo OnCreate()
		{
			DualScreenInfo dualScreenInfo = new DualScreenInfo(null);
			dualScreenInfo._twoPaneViewLayoutGuide.WatchForChanges();
			dualScreenInfo._twoPaneViewLayoutGuide.PropertyChanged += dualScreenInfo.OnTwoPaneViewLayoutGuideChanged;
			return dualScreenInfo;
		}

		void OnTwoPaneViewLayoutGuideChanged(object sender, PropertyChangedEventArgs e)
		{
			SpanningBounds = GetSpanningBounds();
			IsLandscape = GetIsLandscape();
			HingeBounds = GetHingeBounds();
			SpanMode = GetSpanMode();
		}

		bool SetProperty<T>(ref T backingStore, T value,
			[CallerMemberName]string propertyName = "",
			Action onChanged = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return false;

			backingStore = value;
			onChanged?.Invoke();
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			return true;
		}
	}
}

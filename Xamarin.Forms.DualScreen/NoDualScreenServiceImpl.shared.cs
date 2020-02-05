using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.DualScreen
{
    internal class NoDualScreenServiceImpl : IDualScreenService
    {
        static Lazy<NoDualScreenServiceImpl> _Instance = new Lazy<NoDualScreenServiceImpl>(() => new NoDualScreenServiceImpl());
        public static NoDualScreenServiceImpl Instance => _Instance.Value;

        public NoDualScreenServiceImpl()
        {
        }

        public bool IsSpanned => false;

        public bool IsLandscape => Device.info.CurrentOrientation.IsLandscape();

		public DeviceInfo DeviceInfo => Device.info;

		public event EventHandler OnScreenChanged
        {
            add
            {

            }
            remove
            {

            }
        }

        public void Dispose()
        {
        }

        public Rectangle GetHinge()
        {
            return Rectangle.Zero;
        }

        public Point? GetLocationOnScreen(VisualElement visualElement)
        {
            return null;
        }
    }
}

using Xamarin.Forms.Platform.GTK.Packagers;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
    public class PageRenderer : AbstractPageRenderer<Controls.Page, Page>
    {
        private PageElementPackager _packager;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_packager != null)
            {
                _packager.Dispose();
                _packager = null;
            }
        }

        protected override void OnShown()
        {
            base.OnShown();

            if (_packager == null)
            {
                _packager = new PageElementPackager(this);
            }

            _packager.Load();
        }

        protected override void OnSizeAllocated(Gdk.Rectangle allocation)
        {
            if (!Sensitive)
                return;

            base.OnSizeAllocated(allocation);
        }
    }
}
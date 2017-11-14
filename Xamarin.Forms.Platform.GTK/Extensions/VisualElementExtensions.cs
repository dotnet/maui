using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Platform.GTK.Extensions
{
    public static class VisualElementExtensions
    {
        public static IVisualElementRenderer GetOrCreateRenderer(this VisualElement self)
        {
            if (self == null)
                throw new ArgumentNullException("self");

            IVisualElementRenderer renderer = Platform.GetRenderer(self);

            if (renderer == null)
            {
                renderer = RendererFactory.CreateRenderer(self);
                Platform.SetRenderer(self, renderer);
            }

            return renderer;
        }

        internal static IEnumerable<Element> GetParentsPath(this VisualElement self)
        {
            Element current = self;

            while (!Application.IsApplicationOrNull(current.RealParent))
            {
                current = current.RealParent;
                yield return current;
            }
        }

        internal static void Cleanup(this VisualElement self)
        {
            if (self == null)
                throw new ArgumentNullException("self");

            IVisualElementRenderer renderer = Platform.GetRenderer(self);

            foreach (Element element in self.Descendants())
            {
                var visual = element as VisualElement;
                if (visual == null)
                    continue;

                IVisualElementRenderer childRenderer = Platform.GetRenderer(visual);
                if (childRenderer != null)
                {
                    childRenderer.Dispose();
                    Platform.SetRenderer(visual, null);
                }
            }

            if (renderer != null)
            {
                renderer.Dispose();
                Platform.SetRenderer(self, null);
            }
        }
    }
}
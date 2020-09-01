using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class AppActions_Tests
    {
        [Fact]
        public void AppActions_SetActions() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => AppActions.Actions = new List<AppAction>());

        [Fact]
        public void AppActions_GetActions() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => AppActions.Actions);

        [Fact]
        public void AppActions_IsSupported() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => AppActions.IsSupported);
    }
}

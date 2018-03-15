using Xunit;

namespace Microsoft.Caboodle.Tests
{
    public class Preferences_Tests
    {
        [Fact]
        public void Preferences_Fail_On_NetStandard()
        {
            var p = new Preferences();
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => p.Set("anything", "fails"));
        }
    }
}

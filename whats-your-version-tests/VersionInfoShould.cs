using StructuralEqualityAssessor;
using Xunit;

namespace WhatsYourVersion.Tests
{
    public class VersionInfoShould
    {
        [Fact]
        public void SupportStructuralEquality()
        {
            var hasEquality = HasStructural.Equality(typeof(VersionInfo));
            Assert.True(hasEquality);
        }
    }
}
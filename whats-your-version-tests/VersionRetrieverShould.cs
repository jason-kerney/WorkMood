using System;
using Moq;
using Xunit;

namespace WhatsYourVersion.Tests
{
    public class VersionRetrieverShould
    {
        [Fact]
        public void GetVersionInfoFromAssembly()
        {
            var fakeWrapper = new Mock<IAssemblyWrapper>();
            const string expectedVersionString = "12.23.34.45";
            var expectedBuildDate = DateTime.Parse("3/14/1592 6:53:58.97");

            fakeWrapper
                .SetupGet(wrapper => wrapper.VersionString)
                .Returns(expectedVersionString);
            fakeWrapper
                .SetupGet(wrapper => wrapper.BuildDate)
                .Returns(expectedBuildDate);
            
            var sut = new VersionRetriever(fakeWrapper.Object);
            var result = sut.GetVersion();
            
            Assert.Equal(expectedVersionString, result.Version);
            Assert.Equal(expectedBuildDate.ToString("dd MMM yyy HH':'mm':'ss 'UTC'"), result.BuildDateUtc);
        }
        [Fact]
        public void GetDiffVersionInfoFromAssembly()
        {
            var fakeWrapper = new Mock<IAssemblyWrapper>();
            const string expectedVersionString = "11.22.33.44";
            var expectedBuildDate = DateTime.Parse("6/28/3185 3:07:17.95");

            fakeWrapper
                .SetupGet(wrapper => wrapper.VersionString)
                .Returns(expectedVersionString);
            fakeWrapper
                .SetupGet(wrapper => wrapper.BuildDate)
                .Returns(expectedBuildDate);
            
            var sut = new VersionRetriever(fakeWrapper.Object);
            var result = sut.GetVersion();
            
            Assert.Equal(expectedVersionString, result.Version);
            Assert.Equal(expectedBuildDate.ToString("dd MMM yyy HH':'mm':'ss 'UTC'"), result.BuildDateUtc);
        }
    }
}
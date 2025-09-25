using System;
using Xunit;

namespace WhatsYourVersion.Tests
{
    public class BuildDateAttributeShould
    {
        [Fact]
        public void BuildDateFromPiString()
        {
            var sut = new BuildDateAttribute("15920314065358");
            Assert.Equal(DateTime.Parse("3/14/1592 6:53:58"), sut.BuildDateTime);
        }
        
        [Fact]
        public void BuildDateFromTaoString()
        {
            var sut = new BuildDateAttribute("31850628030717");
            Assert.Equal(DateTime.Parse("6/28/3185 3:07:17"), sut.BuildDateTime);
        }
        
        [Fact]
        public void ReturnNullOnUnParsableDate()
        {
            var sut = new BuildDateAttribute("bad-date");
            Assert.Null(sut.BuildDateTime);
        }
    }
}

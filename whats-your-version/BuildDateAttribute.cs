using System;
using System.Globalization;

namespace WhatsYourVersion
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class BuildDateAttribute : Attribute
    {
        public BuildDateAttribute(string value)
        {
            try
            {
                BuildDateTime = DateTime.ParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
            }
            catch
            {
                BuildDateTime = null;
            }
        }

        public DateTime? BuildDateTime { get; }
    }
}
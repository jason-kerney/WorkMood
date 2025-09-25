using System;

namespace WhatsYourVersion
{
    public class VersionInfo : IEquatable<VersionInfo>
    {
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Version != null ? Version.GetHashCode() : 0) * 397) ^ (BuildDateUtc != null ? BuildDateUtc.GetHashCode() : 0);
            }
        }

        public bool Equals(VersionInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Version == other.Version && BuildDateUtc == other.BuildDateUtc;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VersionInfo) obj);
        }

        public static bool operator ==(VersionInfo left, VersionInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(VersionInfo left, VersionInfo right)
        {
            return !Equals(left, right);
        }

        public string Version { get; set; }
        public string BuildDateUtc { get; set; }
    }
}
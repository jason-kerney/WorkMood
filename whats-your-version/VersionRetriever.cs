using System;
using System.Reflection;

namespace WhatsYourVersion
{
    public interface IVersionRetriever
    {
        VersionInfo GetVersion();
    }

    public class VersionRetriever : IVersionRetriever
    {
        private readonly string _version;
        private readonly DateTime? _buildDate;

        public VersionRetriever(Assembly assembly) : this(new AssemblyWrapper(assembly)) { }
        public VersionRetriever(IAssemblyWrapper assembly)
        {
            _version = assembly.VersionString;
            _buildDate = assembly.BuildDate;
        }

        public virtual VersionInfo GetVersion() => new VersionInfo { Version = _version, BuildDateUtc = _buildDate?.ToString("dd MMM yyy HH':'mm':'ss 'UTC'") };
    }
}

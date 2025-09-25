using System;
using System.Reflection;

namespace WhatsYourVersion
{
    public interface IAssemblyWrapper
    {
        string VersionString { get; }
        DateTime? BuildDate { get; }
    }

    public class AssemblyWrapper : IAssemblyWrapper
    {
        private readonly Assembly _assembly;
        private readonly BuildDateAttribute _attribute;

        public AssemblyWrapper(Assembly assembly)
        {
            _assembly = assembly;
            _attribute = _assembly.GetCustomAttribute<BuildDateAttribute>();
        }
        
        public virtual string VersionString => _assembly.GetName().Version + string.Empty;
        public virtual DateTime? BuildDate => _attribute?.BuildDateTime;

        public static IAssemblyWrapper From<T>() => new AssemblyWrapper(typeof(T).Assembly);
    }
}
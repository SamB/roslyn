using Microsoft.CodeAnalysis.Host.Mef;
using Xunit;

namespace Microsoft.CodeAnalysis.UnitTests
{
    public class MefHostServicesTests
    {
        /// <summary>
        /// Based on https://github.com/OmniSharp/omnisharp-roslyn/blob/v1.6.3/src/OmniSharp.Roslyn/HostServicesBuilder.cs
        /// in the case where no <see cref="ICodeActionProvider"/>s
        /// are passed in.
        /// </summary>
        // FIXME: Document what is required to avoid System.Reflection.ReflectionTypeLoadException here
        [Fact(Skip = "Is this not supposed to be done? The documentation for MefHostServices leaves something to be desired.")]
        public void SimpleCreateTest()
        {
            var mhs = MefHostServices.Create(MefHostServices.DefaultAssemblies);
            // If this hasn't thrown, we've passed.
        }
    }
}
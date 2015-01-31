#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.Weavisor.Weaver
{
    /// <summary>
    /// Bindings ArxOne.Weavisor assembly.
    /// Since Fody forbids referencing other assemblies, we had to hardcode the names
    /// </summary>
    public static class Binding
    {
        public const string AdviceMarkerName = "ArxOne.Weavisor.IAdvice";
        public const string InvocationProceedTypeName = "ArxOne.Weavisor.Invocation";
        public const string InvocationProceedMethodMethodName = "ProceedMethod";
    }
}

#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.Weavisor.Weaver
{
    /// <summary>
    /// Bindings ArxOne.Weavisor assembly.
    /// Since Fody forbids referencing other assemblies, we had to hardcode the names
    /// </summary>
    public static class Binding
    {
        public const string AdviceInterfaceName = "ArxOne.Weavisor.Advice.IAdvice";
        public const string InfoAdviceInterfaceName = "ArxOne.Weavisor.Advice.IInfoAdvice";
        public const string IntroducedFieldTypeName = "ArxOne.Weavisor.Introduction.IntroducedField`1";
        public const string InvocationTypeName = "ArxOne.Weavisor.Invocation";
        public const string InvocationProceedAdviceMethodName = "ProceedAdvice";
        public const string InvocationProcessInfoAdvicesMethodName = "ProcessInfoAdvices";
    }
}

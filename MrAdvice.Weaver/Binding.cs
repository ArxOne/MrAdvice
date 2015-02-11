#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Weaver
{
    /// <summary>
    /// Bindings ArxOne.MrAdvice assembly.
    /// Since Fody forbids referencing other assemblies, we had to hardcode the names
    /// </summary>
    public static class Binding
    {
        public const string AdviceInterfaceName = "ArxOne.MrAdvice.Advice.IAdvice";
        public const string InfoAdviceInterfaceName = "ArxOne.MrAdvice.Advice.IInfoAdvice";
        public const string IntroducedFieldTypeName = "ArxOne.MrAdvice.Introduction.IntroducedField`1";
        public const string InvocationTypeName = "ArxOne.MrAdvice.Invocation";
        public const string InvocationProceedAdviceMethodName = "ProceedAdvice";
        public const string InvocationProcessInfoAdvicesMethodName = "ProcessInfoAdvices";
    }
}

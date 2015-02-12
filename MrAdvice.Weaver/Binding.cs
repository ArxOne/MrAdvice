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
        /// <summary>
        /// The advice interface name.
        /// This is used to identify attributes that advice at run-time.
        /// </summary>
        public const string AdviceInterfaceName = "ArxOne.MrAdvice.Advice.IAdvice";
        /// <summary>
        /// The information advice interface name.
        /// This is used to identify attributes that advice at load-time (in type .cctor).
        /// </summary>
        public const string InfoAdviceInterfaceName = "ArxOne.MrAdvice.Advice.IInfoAdvice";

        /// <summary>
        /// The introduced field type name.
        /// This is used to identify fields in advices that will introduce a new member.
        /// All fields with use IntroducedField
        /// </summary>
        public const string IntroducedFieldTypeName = "ArxOne.MrAdvice.Introduction.IntroducedField`1";
        /// <summary>
        /// The introduced field attribute name
        /// </summary>
        public const string IntroducedFieldAttributeName = "ArxOne.MrAdvice.Introduction.IntroducedFieldAttribute";

        /// <summary>
        /// The invocation type name
        /// This is the base type whose methods are called from advice stubs (see the two methods below)
        /// </summary>
        public const string InvocationTypeName = "ArxOne.MrAdvice.Invocation";
        /// <summary>
        /// The invocation proceed advice method name
        /// This method is called by advice stubs
        /// </summary>
        public const string InvocationProceedAdviceMethodName = "ProceedAdvice";
        /// <summary>
        /// The invocation process information advices method name
        /// This methods is called by info advice stubs
        /// </summary>
        public const string InvocationProcessInfoAdvicesMethodName = "ProcessInfoAdvices";
    }
}

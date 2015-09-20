#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
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
        /// The weaving advice interface name.
        /// This is used to dynamically weave interfaces
        /// </summary>
        public const string WeavingAdviceInterfaceName = "ArxOne.MrAdvice.Advice.IWeavingAdvice";

        /// <summary>
        /// Full name of AdviceExtensions interface
        /// </summary>
        public const string AdviceExtensionsTypeName = "ArxOne.MrAdvice.Advice.AdviceExtensions";
        /// <summary>
        /// The advice handle method name
        /// </summary>
        public const string AdviceHandleMethodName = "Handle";

        public const string AdvisedInterfaceTypeName = "ArxOne.MrAdvice.Advice.AdvisedInterface";

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

        /// <summary>
        /// The priority type name
        /// </summary>
        public const string PriorityAttributeTypeName = "ArxOne.MrAdvice.Annotation.PriorityAttribute";
        /// <summary>
        /// The abstract target attribute type name
        /// </summary>
        public const string AbstractTargetAttributeTypeName = "ArxOne.MrAdvice.Annotation.AbstractTargetAttribute";
    }
}

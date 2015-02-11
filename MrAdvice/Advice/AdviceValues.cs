#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Advice
{
    internal class AdviceValues
    {
        public object Target { get; set; }
        public object[] Parameters { get; private set; }
        public object ReturnValue { get; set; }

        public AdviceValues(object target, object[] parameters)
        {
            Target = target;
            Parameters = parameters;
        }
    }
}

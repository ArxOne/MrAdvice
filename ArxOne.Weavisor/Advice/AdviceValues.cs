#region Weavisor
// Weavisor
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.Weavisor.Advice
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

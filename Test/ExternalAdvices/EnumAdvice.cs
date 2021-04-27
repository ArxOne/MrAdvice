using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArxOne.MrAdvice.Advice;

namespace ExternalAdvices
{
    [AttributeUsage(validOn: AttributeTargets.Method)]
    public class EnumAdvice : Attribute, IMethodAdvice
    {
        public EnumAdvice()
        {
        }

        public EnumAdvice(ConsoleColor option)
        {
            Option = option;
        }

        public ConsoleColor Option { get; set; }

        public void Advise(MethodAdviceContext context)
        {
            Console.WriteLine("before");
            context.Proceed();
            Console.WriteLine("after");
        }
    }
}

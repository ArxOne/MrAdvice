
namespace MethodLevelTest.Advices
{
    using System;
    using ArxOne.Weavisor.Advice;

    public class RecordCall : Attribute, IMethodAdvice
    {
        public static int Count;

        public void Advise(Call<MethodCallContext> call)
        {
            Count++;
            call.Proceed();
        }
    }
}

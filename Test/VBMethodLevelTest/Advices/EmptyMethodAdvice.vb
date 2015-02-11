Imports ArxOne.MrAdvice.Advice

Namespace Advices
    Public Class EmptyMethodAdvice
        Inherits Attribute
        Implements IMethodAdvice

        Public Sub Advise(context As MethodAdviceContext) Implements IMethodAdvice.Advise
            context.Proceed()
        End Sub
    End Class
End Namespace
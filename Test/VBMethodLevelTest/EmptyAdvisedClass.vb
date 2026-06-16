Imports System.Reflection
Imports VBMethodLevelTest.Advices
Imports NUnit.Framework

Public Class EmptyAdvisedClass
    <EmptyMethodAdvice()>
    Public Sub MethodTest()
        Dim thisMethod = MethodBase.GetCurrentMethod()

        Assert.That(thisMethod.Name <> "MethodTest")
    End Sub
End Class
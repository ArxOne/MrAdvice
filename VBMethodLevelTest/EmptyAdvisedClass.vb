Imports System.Reflection
Imports VBMethodLevelTest.Advices

Public Class EmptyAdvisedClass
    <EmptyMethodAdvice()>
    Public Sub MethodTest()
        Dim thisMethod = MethodBase.GetCurrentMethod()
        Assert.AreNotEqual("MethodTest", thisMethod.Name)
        End Sub
End Class

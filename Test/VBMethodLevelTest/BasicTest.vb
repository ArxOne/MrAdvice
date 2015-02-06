
<TestClass()>
Public Class BasicTest

    <TestMethod()>
    <TestCategory("Weaving")>
    <TestCategory("VB")>
    Public Sub Simple()
        Dim c = New EmptyAdvisedClass()
        c.MethodTest()
    End Sub

End Class

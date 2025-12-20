Imports NUnit.Framework

<TestFixture>
<Category("Weaving")>
<Category("VB")>
Public Class BasicTest

    <Test>
    Public Sub Simple()
        Dim c = New EmptyAdvisedClass()
        ' Note: Assurez-vous que l'assertion est valide si vous voulez tester un résultat, 
        ' ou laissez tel quel pour tester simplement l'absence d'exception au tissage.
        c.MethodTest()
    End Sub

End Class
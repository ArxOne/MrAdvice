#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Utility
{
    using dnlib.DotNet;
    using dnlib.DotNet.Pdb;

    public static class MethodDefExtensions
    {
        public static void CopyScope(this MethodDef sourceMethodDef, MethodDef targetMethodDef)
        {
        }

        private static void CopyScope(MethodDef sourceMethodDef, PdbScope sourceScope, MethodDef targetMethodDef, PdbScope targetScope)
        {

        }

        //PdbScope Clone(PdbScope a, MethodDef targetMethodDef, MethodDef sourceMethodDef)
        //{
        //    var scope = new PdbScope();
        //    scope.Start = targetMethodDef.Body.Instructions[   a.Start];
        //    scope.End = a.End == null ? null : toNew[a.End];
        //    scope.Namespaces.AddRange(a.Namespaces);
        //    foreach (var l in a.Variables)
        //        scope.Variables.Add(toNew(l));
        //    foreach (var nested in a.Scopes)
        //        scope.Scopes.Add(Clone(nested, targetMethodDef));
        //    return scope;
        //}
    }
}

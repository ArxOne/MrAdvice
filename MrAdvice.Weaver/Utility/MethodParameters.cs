#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Utility
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using dnlib.DotNet;

    public class MethodParameters : IEnumerable<Parameter>
    {
        private readonly MethodDef _methodDef;
        private readonly IList<int> _reindex = new List<int>();

        public int Count => _reindex.Count;

        public Parameter this[int index]
        {
            get
            {
                var parameter = _methodDef.Parameters[_reindex[index]];
                if (parameter.ParamDef == null)
                    parameter.CreateParamDef();
                return parameter;
            }
        }

        public MethodParameters(MethodDef methodDef, bool ignoreReturn = true, bool ignoreThis = true)
        {
            _methodDef = methodDef;
            for (int parameterIndex = 0; parameterIndex < methodDef.Parameters.Count; parameterIndex++)
            {
                var parameter = methodDef.Parameters[parameterIndex];
                if (parameter.IsHiddenThisParameter && ignoreThis)
                    continue;
                if (parameter.IsReturnTypeParameter && ignoreReturn)
                    continue;
                _reindex.Add(parameterIndex);
            }
        }

        public IEnumerator<Parameter> GetEnumerator() => _reindex.Select(i => _methodDef.Parameters[i]).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void SetParamDefs(MethodDef targetMethod)
        {
            for (int parameterIndex = 0; parameterIndex < Count; parameterIndex++)
            {
                var paramDefUser = new ParamDefUser();
                paramDefUser.Set(this[parameterIndex].ParamDef);
                targetMethod.ParamDefs.Add(paramDefUser);
                //targetMethod.Parameters[parameterIndex].CreateParamDef();
                //targetMethod.Parameters[parameterIndex].ParamDef.Set(this[parameterIndex].ParamDef);
            }
        }
    }
}

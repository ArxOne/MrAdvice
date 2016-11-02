#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Utility
{
    using dnlib.DotNet;

    public static class ParamDefExtensions
    {
        public static void Set(this ParamDef paramDef, ParamDef source)
        {
            paramDef.Name = source.Name;
            paramDef.Sequence = source.Sequence;
            //paramDef.Rid = source.Rid;
            if (source.HasMarshalType)
                paramDef.MarshalType = source.MarshalType;
            if (source.HasConstant)
                paramDef.Constant = source.Constant;
            paramDef.Attributes = source.Attributes;
            foreach (var ca in source.CustomAttributes)
                paramDef.CustomAttributes.Add(ca);
        }
    }
}

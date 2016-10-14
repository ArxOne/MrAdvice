#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Reflection
{
    using dnlib.DotNet;
    using StitcherBoy.Reflection;
    public class TypeImporter : TypeRelocator
    {
        private readonly ModuleDef _moduleDef;

        protected override TypeSig TryRelocateTypeRef(TypeRef typeRef)
        {
            return _moduleDef.Import(typeRef).ToTypeSig();
        }

        public TypeImporter(ModuleDef moduleDef)
        {
            _moduleDef = moduleDef;
        }

        public static TypeSig Import(ModuleDef moduleDef, TypeSig typeSig)
        {
            var importer = new TypeImporter(moduleDef);
            var newTypeSig = importer.TryRelocateTypeSig(typeSig);
            return newTypeSig ?? typeSig;
        }
    }
}

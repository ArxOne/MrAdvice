#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ExternalAdvices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IExternalAdvisedInterface
    {
        void F(ExternalData externalData);

        void G(int? a);

        void H(IList<ExternalData> e);

        Task<ExternalData[]> Z();

        void T(ExternalEnum e);
        void T2(Task<ExternalEnum> ee);
    }
}

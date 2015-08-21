#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using Advices;

    public class OverloadedIndexerAdvisedClass
    {
        //[ChangeParameter(NewReturnValue = 100)]
        //public int Item { get; set; }

        [ChangeParameter(NewReturnValue = 10)]
        public int this[string v]
        {
            get { return 1; }
            set { }
        }

        [ChangeParameter(NewReturnValue = 20)]
        public int this[int v]
        {
            get { return 2; }
        }
    }
}

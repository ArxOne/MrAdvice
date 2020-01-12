#region Mr. Advice

// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php

#endregion

namespace NetStandardTest
{
	using ArxOne.MrAdvice.Advice;
	using System;

	public class BasicTestAdvice : Attribute, IMethodAdvice
	{
		public BasicTestAdvice(bool validateAllProperties = true)
		{
		}

		public void Advise(MethodAdviceContext context)
		{
		}
	}
}

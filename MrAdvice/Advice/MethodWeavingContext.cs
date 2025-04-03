#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Advice;

using ArxOne.MrAdvice.Advice.Builder;

/// <summary>
/// Context for <see cref="IMethodWeavingAdvice"/>
/// </summary>
public class MethodWeavingContext : WeavingContext
{
    /// <summary>
    /// Gets or sets the name of the target method.
    /// </summary>
    /// <value>
    /// The name of the target method.
    /// </value>
    public string TargetMethodName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodWeavingContext" /> class.
    /// </summary>
    /// <param name="targetMethodName">Name of the target method.</param>
    /// <param name="typeWeaver">The type weaver.</param>
    public MethodWeavingContext(string targetMethodName, ITypeWeaver typeWeaver)
        : base(typeWeaver)
    {
        TargetMethodName = targetMethodName;
    }
}

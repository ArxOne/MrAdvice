
namespace ArxOne.MrAdvice.Advice;

public interface ITypeWeavingAdvice: IWeavingAdvice
{
    /// <summary>
    /// Advises the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    void Advise(WeavingContext context);
}

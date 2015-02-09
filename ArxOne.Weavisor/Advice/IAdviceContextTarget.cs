
namespace ArxOne.Weavisor.Advice
{
    /// <summary>
    /// Target part with advice context
    /// Used by introduced fields
    /// </summary>
    public interface IAdviceContextTarget
    {
        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        object Target { get; }
    }
}

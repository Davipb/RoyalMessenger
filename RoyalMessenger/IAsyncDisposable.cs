using System.Threading.Tasks;

namespace RoyalMessenger
{
    /// <summary>
    /// <para>
    /// Represents an object holding resources that must be released after the application is done using them,
    /// and is also capable of releasing such resources asynchronously.
    /// </para>
    /// <para>
    /// This interface is intended for library use only and will be removed when the async-streams proposal
    /// is implemented in C#, since <code>IAsyncDiposable</code> will become a native type.
    /// </para>
    /// </summary>
    public interface IAsyncDisposable
    {
        /// <summary> Releases the resources being held by this object asynchronously. </summary>
        Task DisposeAsync();
    }
}

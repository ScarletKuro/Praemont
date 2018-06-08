using NoteCore.Model;

namespace NoteCore.Repositories
{
    public interface IOptionRepository
    {
        /// <summary>
        /// Saves the Options.
        /// </summary>
        void Save();

        /// <summary>
        /// Get property of <see cref="Options"/> class.
        /// </summary>
        /// <returns>A List with all property</returns>
        Options GetConfig { get; }
        
        /// <summary>
        /// Resets the repository.
        /// </summary>
        void RepositoryReset();
    }
}

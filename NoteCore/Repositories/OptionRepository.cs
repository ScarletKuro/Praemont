using System;
using System.IO;
using NoteCore.Model;
using NoteCore.Utils;

namespace NoteCore.Repositories
{
    /// <summary>
    /// This class manages a note repository.
    /// The options will be saved using serialization.
    /// </summary>
    public class OptionRepository
        : IOptionRepository
    {
        #region Internal properties

        private Options _optionStore;
        private readonly string _dataFile;

        #endregion
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionRepository"/> class
        /// </summary>
        /// <param name="fileName">The file name.</param>
        public OptionRepository(string fileName)
        {
            fileName = string.Format("{0}.dat", fileName);
            _dataFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            Deserialize();
        }
        #endregion
        #region Implementation interface IOptionRepository
        /// <summary>
        /// Saves the Options.
        /// </summary>
        public void Save()
        {
            Serialize();
        }
        /// <summary>
        /// Get property of <see cref="Options"/> class.
        /// </summary>
        /// <returns>A List with all property</returns>
        public Options GetConfig
        {
            get { return _optionStore; }
        }

        /// <summary>
        /// Resets the repository.
        /// </summary>
        public void RepositoryReset()
        {
            File.Delete(_dataFile);
            _optionStore = new Options();
        }
        #endregion

        /// <summary>
        /// Serialize options to a file.
        /// </summary>
        private void Serialize()
        {
            ClassSerialization.Save(_dataFile, _optionStore);
        }

        /// <summary>
        /// Deserialize options or creates an empty option class.
        /// </summary>
        private void Deserialize()
        {
            _optionStore = File.Exists(_dataFile) ? ClassSerialization.Load<Options>(_dataFile) : new Options();
        }
    }
}
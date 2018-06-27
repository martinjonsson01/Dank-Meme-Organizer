using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Storage;

namespace DMO.Models
{
    public abstract class MediaData : BaseModel
    {
        #region Public Properties

        public StorageFile MediaFile;

        /// <summary>
        /// Gets or sets the title of this Media.
        /// </summary>
        /// <value>
        /// The title of this Media.
        /// </value>
        public string Title
        {
            get => MediaFile?.Name;
            set => TryRenameFile(value);
        }

        public DateTime LastModified { get; set; }

        public DateTimeOffset Created => MediaFile.DateCreated;

        /// <summary>
        /// The unique file-system-wide identifier string of this file.
        /// </summary>
        public string Uid { get; }

        #endregion

        #region Constructor

        public MediaData(StorageFile file)
        {
            MediaFile = file;
            Uid = GetNTFSUid();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Tries to rename this file to the provided value.
        /// </summary>
        /// <param name="value">The new name of this file.</param>
        public async void TryRenameFile(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            if (value == Title) return;
            if (value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return;

            var nameIndex = MediaFile.Path.Length - MediaFile.Name.Length;
            var folderPath = MediaFile.Path.Remove(nameIndex);
            var combinedPath = Path.Combine(folderPath, value);
            if (await Task.Factory.StartNew(() => File.Exists(combinedPath))) return;

            App.Files.Remove(MediaFile.Name);
            await MediaFile.RenameAsync(value);
            App.Files.Add(value, MediaFile);
            OnPropertyChanged(nameof(Title));
        }

        #region Interop stuff for NTFSUid methods

        struct BY_HANDLE_FILE_INFORMATION
        {
            public uint FileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetFileInformationByHandle(SafeFileHandle hFile, out BY_HANDLE_FILE_INFORMATION lpFileInformation);

        #endregion

        /// <summary>
        /// Opens a stream to the <see cref="StorageFile"/> of this object and gets the NTFS unique identifier from it.
        /// </summary>
        /// <returns>A unique file-system-wide identifier string.</returns>
        public string GetNTFSUid()
        {
            using (var stream = new FileStream(MediaFile.CreateSafeFileHandle(), FileAccess.ReadWrite))
            {
                return GetNTFSUid(stream);
            }
        }

        /// <summary>
        /// Uses the SafeFileHandle of the provided <see cref="FileStream"/> to access the _BY_HANDLE_FILE_INFORMATION structure
        /// to get the VolumeSerialNumber, FileIndexHigh and FileIndexLow which are then concatenated and returned as a string.
        /// </summary>
        /// <param name="stream">The open read-access file stream to the file.</param>
        /// <returns>A unique file-system-wide identifier string.</returns>
        public static string GetNTFSUid(FileStream stream)
        {
            BY_HANDLE_FILE_INFORMATION hInfo = new BY_HANDLE_FILE_INFORMATION();
            GetFileInformationByHandle(stream.SafeFileHandle, out hInfo);
            return $"{hInfo.VolumeSerialNumber}{hInfo.FileIndexHigh}{hInfo.FileIndexLow}";
        }

        #endregion
    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GoreUpdater
{
    /// <summary>
    /// Delegate for handling error events from the <see cref="IFileUploader"/>.
    /// </summary>
    /// <param name="sender">The <see cref="IFileUploader"/> that the event came from.</param>
    /// <param name="localFile">The local file for the job related to the error.
    /// Will be an empty string if not relevant to a specific job.</param>
    /// <param name="remoteFile">The remote file for the job related to the error
    /// Will be an empty string if not relevant to a specific job.</param>
    /// <param name="error">A string containing the error message.</param>
    public delegate void FileUploaderErrorEventHandler(IFileUploader sender, string localFile, string remoteFile, string error);

    /// <summary>
    /// Interface for an object that can handle uploading files to a destination.
    /// All implementations must be completely thread-safe.
    /// </summary>
    public interface IFileUploader : IDisposable
    {
        /// <summary>
        /// Notifies listeners when there has been an error related to one of the upload jobs. The job in question will still
        /// be re-attempted by default.
        /// </summary>
        event FileUploaderErrorEventHandler UploadError;

        /// <summary>
        /// Gets if the <see cref="IFileUploader"/> is currently busy.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// Gets if this object has been disposed.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Gets or sets if files that already exist on the destination will be skipped.
        /// </summary>
        bool SkipIfExists { get; set; }

        /// <summary>
        /// Enqueues a file for uploading.
        /// </summary>
        /// <param name="sourcePath">The path to the local file to upload.</param>
        /// <param name="targetPath">The path to upload the file to on the destionation.</param>
        /// <returns>True if the file was enqueued; false if either of the arguments were invalid, or the file already
        /// exists in the queue.</returns>
        bool Enqueue(string sourcePath, string targetPath);

        /// <summary>
        /// Enqueues multiple files for uploading.
        /// </summary>
        /// <param name="files">The files to upload, where the key is the source path, and the value is the
        /// path to upload the file on the destination.</param>
        void Enqueue(IEnumerable<KeyValuePair<string, string>> files);
    }
}

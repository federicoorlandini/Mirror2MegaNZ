using System;

namespace Mirror2MegaNZ.Logic
{
    public class RemoteDeletingEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the filename of the file that the cleaner is deleting.
        /// </summary>
        /// <value>
        /// The filename.
        /// </value>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RemoteDeletingEventArgs"/> is canceled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancel; otherwise, <c>false</c>.
        /// </value>
        public bool Cancel { get; set; }
    }
}

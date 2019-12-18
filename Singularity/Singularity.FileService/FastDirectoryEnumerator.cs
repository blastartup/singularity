using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace Singularity.FileService
{
    /// <summary>
    /// Contains information about a file returned by the 
    /// <see cref="FastDirectoryEnumerator"/> class.
    /// </summary>
    [Serializable]
    public class FileData
    {
        /// <summary>
        /// Attributes of the file.
        /// </summary>
        public readonly FileAttributes Attributes;

        public DateTime CreationTime => CreationTimeUtc.ToLocalTime();

        /// <summary>
        /// File creation time in UTC
        /// </summary>
        public readonly DateTime CreationTimeUtc;

        /// <summary>
        /// Gets the last access time in local time.
        /// </summary>
        public DateTime LastAccessTime => LastAccessTimeUtc.ToLocalTime();

        /// <summary>
        /// File last access time in UTC
        /// </summary>
        public readonly DateTime LastAccessTimeUtc;

        /// <summary>
        /// Gets the last access time in local time.
        /// </summary>
        public DateTime LastWriteTime => LastWriteTimeUtc.ToLocalTime();

        /// <summary>
        /// File last write time in UTC
        /// </summary>
        public readonly DateTime LastWriteTimeUtc;
        
        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        public readonly Int64 Size;

        /// <summary>
        /// Name of the file
        /// </summary>
        public readonly String Name;

        /// <summary>
        /// Full path to the file.
        /// </summary>
        public readonly String Path;

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override String ToString()
        {
            return Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileData"/> class.
        /// </summary>
        /// <param name="dir">The directory that the file is stored at</param>
        /// <param name="findData">WIN32_FIND_DATA structure that this
        /// object wraps.</param>
        internal FileData(String dir, WIN32_FIND_DATA findData) 
        {
            Attributes = findData.dwFileAttributes;


            CreationTimeUtc = ConvertDateTime(findData.ftCreationTime_dwHighDateTime, 
                                                findData.ftCreationTime_dwLowDateTime);

            LastAccessTimeUtc = ConvertDateTime(findData.ftLastAccessTime_dwHighDateTime,
                                                findData.ftLastAccessTime_dwLowDateTime);

            LastWriteTimeUtc = ConvertDateTime(findData.ftLastWriteTime_dwHighDateTime,
                                                findData.ftLastWriteTime_dwLowDateTime);

            Size = CombineHighLowInts(findData.nFileSizeHigh, findData.nFileSizeLow);

            Name = findData.cFileName;
            Path = System.IO.Path.Combine(dir, findData.cFileName);
        }

        private static Int64 CombineHighLowInts(UInt32 high, UInt32 low) => (((Int64)high) << 0x20) | low;

        private static DateTime ConvertDateTime(UInt32 high, UInt32 low)
        {
            Int64 fileTime = CombineHighLowInts(high, low);
            return DateTime.FromFileTimeUtc(fileTime);
        }
    }

    /// <summary>
    /// Contains information about the file that is found 
    /// by the FindFirstFile or FindNextFile functions.
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto), BestFitMapping(false)]
    internal class WIN32_FIND_DATA
    {
        public FileAttributes dwFileAttributes;
        public UInt32 ftCreationTime_dwLowDateTime;
        public UInt32 ftCreationTime_dwHighDateTime;
        public UInt32 ftLastAccessTime_dwLowDateTime;
        public UInt32 ftLastAccessTime_dwHighDateTime;
        public UInt32 ftLastWriteTime_dwLowDateTime;
        public UInt32 ftLastWriteTime_dwHighDateTime;
        public UInt32 nFileSizeHigh;
        public UInt32 nFileSizeLow;
        public Int32 dwReserved0;
        public Int32 dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public String cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public String cAlternateFileName;

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override String ToString()
        {
            return "File name=" + cFileName;
        }
    }

    /// <summary>
    /// A fast enumerator of files in a directory.  Use this if you need to get attributes for 
    /// all files in a directory.
    /// </summary>
    /// <remarks>
    /// This enumerator is substantially faster than using <see cref="Directory.GetFiles(string)"/>
    /// and then creating a new FileInfo object for each path.  Use this version when you 
    /// will need to look at the attributes of each file returned (for example, you need
    /// to check each file in a directory to see if it was modified after a specific date).
    /// </remarks>
    public static class FastDirectoryEnumerator
    {
        /// <summary>
        /// Gets <see cref="FileData"/> for all the files in a directory.
        /// </summary>
        /// <param name="path">The path to search.</param>
        /// <returns>An object that implements <see cref="IEnumerable{FileData}"/> and 
        /// allows you to enumerate the files in the given directory.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="path"/> is a null reference (Nothing in VB)
        /// </exception>
        public static IEnumerable<FileData> EnumerateFiles(String path) => EnumerateFiles(path, "*");

        /// <summary>
        /// Gets <see cref="FileData"/> for all the files in a directory that match a 
        /// specific filter.
        /// </summary>
        /// <param name="path">The path to search.</param>
        /// <param name="searchPattern">The search string to match against files in the path.</param>
        /// <returns>An object that implements <see cref="IEnumerable{FileData}"/> and 
        /// allows you to enumerate the files in the given directory.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="path"/> is a null reference (Nothing in VB)
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filter"/> is a null reference (Nothing in VB)
        /// </exception>
        public static IEnumerable<FileData> EnumerateFiles(String path, String searchPattern) => EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);

        /// <summary>
		/// Gets <see cref="FileData"/> for all the files in a directory that 
		/// match a specific filter, optionally including all sub directories.
		/// </summary>
		/// <param name="path">The path to search.</param>
		/// <param name="searchPattern">The search string to match against files in the path.</param>
		/// <param name="searchOption">
		/// One of the SearchOption values that specifies whether the search 
		/// operation should include all subdirectories or only the current directory.
		/// </param>
		/// <returns>An object that implements <see cref="IEnumerable{FileData}"/> and 
		/// allows you to enumerate the files in the given directory.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="path"/> is a null reference (Nothing in VB)
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="searchPattern"/> is a null reference (Nothing in VB)
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="searchOption"/> is not one of the valid values of the
		/// <see cref="System.IO.SearchOption"/> enumeration.
		/// </exception>
		public static IEnumerable<FileData> EnumerateFiles(String path, String searchPattern, SearchOption searchOption)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
            {
                throw new ArgumentOutOfRangeException("searchOption");
            }

            String fullPath = Path.GetFullPath(path);

            return new FileEnumerable(fullPath, searchPattern, searchOption);
        }

		/// <summary>
		/// Gets <see cref="FileData"/> for all the files in a directory that match a 
		/// specific filter.
		/// </summary>
		/// <param name="path">The path to search.</param>
		/// <param name="searchPattern">The search string to match against files in the path.</param>
		/// <param name="searchOption">Search option.</param>
		/// <returns>An object that implements <see cref="IEnumerable{FileData}"/> and 
		/// allows you to enumerate the files in the given directory.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="path"/> is a null reference)
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="searchPattern"/> is a null reference
		/// </exception>
		public static FileData[] GetFiles(String path, String searchPattern, SearchOption searchOption)
        {
            IEnumerable<FileData> e = EnumerateFiles(path, searchPattern, searchOption);
            List<FileData> list = new List<FileData>(e);

            FileData[] retval = new FileData[list.Count];
            list.CopyTo(retval);

            return retval;
        }

        /// <summary>
        /// Provides the implementation of the 
        /// <see cref="T:System.Collections.Generic.IEnumerable`1"/> interface
        /// </summary>
        private class FileEnumerable : IEnumerable<FileData>
        {
            private readonly String _path;
            private readonly String _filter;
            private readonly SearchOption _searchOption;

            /// <summary>
            /// Initializes a new instance of the <see cref="FileEnumerable"/> class.
            /// </summary>
            /// <param name="path">The path to search.</param>
            /// <param name="filter">The search string to match against files in the path.</param>
            /// <param name="searchOption">
            /// One of the SearchOption values that specifies whether the search 
            /// operation should include all subdirectories or only the current directory.
            /// </param>
            public FileEnumerable(String path, String filter, SearchOption searchOption)
            {
                _path = path;
                _filter = filter;
                _searchOption = searchOption;
            }

            #region IEnumerable<FileData> Members

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can 
            /// be used to iterate through the collection.
            /// </returns>
            public IEnumerator<FileData> GetEnumerator() => new FileEnumerator(_path, _filter, _searchOption);

            #endregion

            #region IEnumerable Members

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Collections.IEnumerator"/> object that can be 
            /// used to iterate through the collection.
            /// </returns>
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new FileEnumerator(_path, _filter, _searchOption);

            #endregion
        }

        /// <summary>
        /// Wraps a FindFirstFile handle.
        /// </summary>
        private sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [DllImport("kernel32.dll")]
            private static extern Boolean FindClose(IntPtr handle);

            /// <summary>
            /// Initializes a new instance of the <see cref="SafeFindHandle"/> class.
            /// </summary>
            [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
            internal SafeFindHandle() : base(true)
            {
            }

            /// <summary>
            /// When overridden in a derived class, executes the code required to free the handle.
            /// </summary>
            /// <returns>
            /// true if the handle is released successfully; otherwise, in the 
            /// event of a catastrophic failure, false. In this case, it 
            /// generates a releaseHandleFailed MDA Managed Debugging Assistant.
            /// </returns>
            protected override Boolean ReleaseHandle() => FindClose(handle);
        }

        /// <summary>
        /// Provides the implementation of the 
        /// <see cref="T:System.Collections.Generic.IEnumerator`1"/> interface
        /// </summary>
        [System.Security.SuppressUnmanagedCodeSecurity]
        private class FileEnumerator : IEnumerator<FileData>
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern SafeFindHandle FindFirstFile(String fileName, [In, Out] WIN32_FIND_DATA data);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern Boolean FindNextFile(SafeFindHandle hndFindFile, [In, Out, MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_DATA lpFindFileData);

            /// <summary>
            /// Hold context information about where we current are in the directory search.
            /// </summary>
            private class SearchContext
            {
                public readonly String Path;
                public Stack<String> SubdirectoriesToProcess;

                public SearchContext(String path)
                {
                    Path = path;
                }
            }

            private String _path;
            private String _filter;
            private SearchOption _searchOption;
            private Stack<SearchContext> _contextStack;
            private SearchContext _currentContext;

            private SafeFindHandle _hndFindFile;
            private WIN32_FIND_DATA _win_find_data = new WIN32_FIND_DATA();

            /// <summary>
            /// Initializes a new instance of the <see cref="FileEnumerator"/> class.
            /// </summary>
            /// <param name="path">The path to search.</param>
            /// <param name="filter">The search string to match against files in the path.</param>
            /// <param name="searchOption">
            /// One of the SearchOption values that specifies whether the search 
            /// operation should include all subdirectories or only the current directory.
            /// </param>
            public FileEnumerator(String path, String filter, SearchOption searchOption)
            {
                _path = path;
                _filter = filter;
                _searchOption = searchOption;
                _currentContext = new SearchContext(path);
                
                if (_searchOption == SearchOption.AllDirectories)
                {
                    _contextStack = new Stack<SearchContext>();
                }
            }

            #region IEnumerator<FileData> Members

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value></value>
            /// <returns>
            /// The element in the collection at the current position of the enumerator.
            /// </returns>
            public FileData Current => new FileData(_path, _win_find_data);

            #endregion

            #region IDisposable Members

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, 
            /// or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if (_hndFindFile != null)
                {
                    _hndFindFile.Dispose();
                }
            }

            #endregion

            #region IEnumerator Members

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value></value>
            /// <returns>
            /// The element in the collection at the current position of the enumerator.
            /// </returns>
            Object System.Collections.IEnumerator.Current => new FileData(_path, _win_find_data);

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// true if the enumerator was successfully advanced to the next element; 
            /// false if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public Boolean MoveNext()
            {
                Boolean retval = false;

                //If the handle is null, this is first call to MoveNext in the current 
                // directory.  In that case, start a new search.
                if (_currentContext.SubdirectoriesToProcess == null)
                {
                    if (_hndFindFile == null)
                    {
                        new FileIOPermission(FileIOPermissionAccess.PathDiscovery, _path).Demand();

                        String searchPath = Path.Combine(_path, _filter);
                        _hndFindFile = FindFirstFile(searchPath, _win_find_data);
                        retval = !_hndFindFile.IsInvalid;
                    }
                    else
                    {
                        //Otherwise, find the next item.
                        retval = FindNextFile(_hndFindFile, _win_find_data);
                    }
                }

                //If the call to FindNextFile or FindFirstFile succeeded...
                if (retval)
                {
                    if (((FileAttributes)_win_find_data.dwFileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        //Ignore folders for now.   We call MoveNext recursively here to 
                        // move to the next item that FindNextFile will return.
                        return MoveNext();
                    }
                }
                else if (_searchOption == SearchOption.AllDirectories)
                {
                    //SearchContext context = new SearchContext(m_hndFindFile, m_path);
                    //m_contextStack.Push(context);
                    //m_path = Path.Combine(m_path, m_win_find_data.cFileName);
                    //m_hndFindFile = null;

                    if (_currentContext.SubdirectoriesToProcess == null)
                    {
                        String[] subDirectories = Directory.GetDirectories(_path);
                        _currentContext.SubdirectoriesToProcess = new Stack<String>(subDirectories);
                    }

                    if (_currentContext.SubdirectoriesToProcess.Count > 0)
                    {
                        String subDir = _currentContext.SubdirectoriesToProcess.Pop();

                        _contextStack.Push(_currentContext);
                        _path = subDir;
                        _hndFindFile = null;
                        _currentContext = new SearchContext(_path);
                        return MoveNext();
                    }

                    //If there are no more files in this directory and we are 
                    // in a sub directory, pop back up to the parent directory and
                    // continue the search from there.
                    if (_contextStack.Count > 0)
                    {
                        _currentContext = _contextStack.Pop();
                        _path = _currentContext.Path;
                        if (_hndFindFile != null)
                        {
                            _hndFindFile.Close();
                            _hndFindFile = null;
                        }

                        return MoveNext();
                    }
                }

                return retval;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            /// <exception cref="T:System.InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public void Reset()
            {
                _hndFindFile = null;
            }

            #endregion
        }
    }
}

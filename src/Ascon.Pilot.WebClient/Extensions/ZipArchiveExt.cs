using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Ascon.Pilot.WebClient.Extensions
{
    public static class ZipArchiveExt
    {
        public static ZipArchiveDirectory CreateDirectory(this ZipArchive @this, string directoryPath)
        {
            return new ZipArchiveDirectory(@this, directoryPath);
        }
    }

    public class ZipArchiveDirectory
    {
        private readonly string _directory;
        private ZipArchive _archive;

        internal ZipArchiveDirectory(ZipArchive archive, string directory)
        {
            _archive = archive;
            _directory = directory;
        }

        public ZipArchive Archive { get { return _archive; } }

        public ZipArchiveEntry CreateEntry(string entry)
        {
            return _archive.CreateEntry(_directory + "/" + entry);
        }

        public ZipArchiveEntry CreateEntry(string entry, CompressionLevel compressionLevel)
        {
            return _archive.CreateEntry(_directory + "/" + entry, compressionLevel);
        }
    }
}

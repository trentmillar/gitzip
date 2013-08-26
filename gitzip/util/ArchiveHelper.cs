using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;

namespace gitzip.util
{
    
    public class ArchiveType
    {
        public string Extension { get; private set; }

        private ArchiveType(string extension)
        {
            Extension = extension;
        }

        public static readonly ArchiveType ZIP = new ArchiveType(".zip");
        public static readonly ArchiveType RAR = new ArchiveType(".rar");
        public static readonly ArchiveType TAR_GZ = new ArchiveType(".tar.gz");

    }

    public class ArchiveHelper
    {
        public string CreateArchive(string sourceDirectory, ArchiveType type)
        {
            string path = IOHelper.GetUniqueArchiveFileAndPath(type);

            new FileInfo(path).Directory.Create();

            if(type == ArchiveType.ZIP)
            {
                ZipDirectory(path, sourceDirectory);

            }
            else if(type == ArchiveType.TAR_GZ)
            {
                CreateTarGZ(path, sourceDirectory);
            }
            else
            {
                throw new ArgumentOutOfRangeException("ONLY SUPPORT ZIP OR TARGZ");
            }
            return path;
        }

        private void CreateTarGZ(string tgzFilename, string sourceDirectory)
        {
            using(Stream outStream = File.Create(tgzFilename))
            {
                Stream gzoStream = new GZipOutputStream(outStream);
                TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);

                // Note that the RootPath is currently case sensitive and must be forward slashes e.g. "c:/temp"
                // and must not end with a slash, otherwise cuts off first char of filename
                // This is scheduled for fix in next release
                tarArchive.RootPath = sourceDirectory.Replace('\\', '/');
                if (tarArchive.RootPath.EndsWith("/"))
                    tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

                AddDirectoryFilesToTar(tarArchive, sourceDirectory, true);

                tarArchive.Close();
            }
        }

        private void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
        {

            // Optionally, write an entry for the directory itself.
            // Specify false for recursion here if we will add the directory's files individually.
            //
            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarArchive.WriteEntry(tarEntry, false);

            // Write each file to the tar.
            //
            string[] filenames = Directory.GetFiles(sourceDirectory);
            foreach (string filename in filenames)
            {
                tarEntry = TarEntry.CreateEntryFromFile(filename);
                tarArchive.WriteEntry(tarEntry, true);
            }

            if (recurse)
            {
                string[] directories = Directory.GetDirectories(sourceDirectory);
                foreach (string directory in directories)
                    AddDirectoryFilesToTar(tarArchive, directory, recurse);
            }
        }

        // Compresses the files in the nominated folder, and creates a zip file on disk named as outPathname.
        private void ZipDirectory(string outPathname, string folderName)
        {
            using (FileStream fsOut = File.Create(outPathname))
            {
                ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                zipStream.SetLevel(9); //0-9, 9 being the highest level of compression

               // zipStream.Password = password; // optional. Null is the same as not setting. Required if using AES.

                // This setting will strip the leading part of the folder path in the entries, to
                // make the entries relative to the starting folder.
                // To include the full path for each entry up to the drive root, assign folderOffset = 0.
                int folderOffset = folderName.Length + (folderName.EndsWith("\\") ? 0 : 1);

                ZipDirectory(folderName, zipStream, folderOffset);

                zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
                zipStream.Close();
            }
        }

        // Recurses down the folder structure
        //
        private void ZipDirectory(string path, ZipOutputStream zipStream, int folderOffset)
        {
            string[] files = Directory.GetFiles(path);

            foreach (string filename in files)
            {
                FileInfo fi = new FileInfo(filename);

                string entryName = filename.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity

                // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                // A password on the ZipOutputStream is required if using AES.
                //   newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                // but the zip will be in Zip64 format which not all utilities can understand.
                //   zipStream.UseZip64 = UseZip64.Off;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                ZipDirectory(folder, zipStream, folderOffset);
            }
        }

    }
}
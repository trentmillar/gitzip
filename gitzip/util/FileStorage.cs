using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace gitzip.util
{
    public class FileStorage
    {
        private FileStorage() { }

        /*public static FileInfo CompressToRegistryZipToTemporaryLocation(FileInfo fileInfo)
        {
            FileInfo compressedFile = new FileInfo(
                Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(fileInfo.Name) + ".zip"));
            using (ZipFile zipFile = new ZipFile(compressedFile.FullName))
            {
                zipFile.Add(fileInfo.FullName);
            }
            return compressedFile;
        }

        public static FileInfo UncompressRegistryZipToTemporaryLocation(FileInfo fileInfo)
        {
            ICSharpCode.SharpZipLib.Core.WindowsPathUtils.
            using (ZipFile zipFile = ZipFile.Read(fileInfo.FullName))
            {
                if (zipFile.Entries.Count != 1)
                {
                    throw new Exception(String.Format("Expecting 1 file in a zip file from the Registry; found {0}.  File {1}.", zipFile.Entries.Count, fileInfo.FullName));
                }

                ZipEntry entry = zipFile.Entries.First();
                string filenameWithoutFolders = new FileInfo(entry.FileName).Name;
                FileInfo uncompressedFile = new FileInfo(Path.Combine(Path.GetTempPath(), filenameWithoutFolders));
                using (Stream s = uncompressedFile.OpenWrite())
                {
                    entry.Extract(s);
                }

                //entry.Extract(uncompressedFile.FullName, );

                return uncompressedFile;
            }
        }

        public static FileInfo UncompressSingleFileFromZipToTemporaryLocation(FileInfo fileInfo, String filename, DirectoryInfo targetDirectoryInfo = null)
        {
            if (targetDirectoryInfo == null)
            {
                targetDirectoryInfo = new DirectoryInfo(Path.GetTempPath());
            }

            targetDirectoryInfo.Create();

            using (ZipFile zipFile = ZipFile.Read(fileInfo.FullName))
            {
                ZipEntry entry = zipFile.Entries.Where(z => z.FileName.ToLowerInvariant() == filename.ToLowerInvariant()).SingleOrDefault();
                if (entry == null)
                {
                    throw new FileNotFoundException(String.Format("Failed to find file {0} in zip archive {1}.", filename, fileInfo.FullName));
                }

                string filenameWithoutFolders = new FileInfo(entry.FileName).Name;
                FileInfo uncompressedFile = new FileInfo(Path.Combine(targetDirectoryInfo.FullName, filenameWithoutFolders));
                using (Stream s = uncompressedFile.OpenWrite())
                {
                    entry.Extract(s);
                }

                return uncompressedFile;
            }
        }

        public static DirectoryInfo ExtractStorageItemZipToTemporaryLocation(Guid storageItemFk, Guid? uniqueFolderName = null, bool forceOverwrite = false)
        {
            if (uniqueFolderName == null)
            {
                uniqueFolderName = storageItemFk;
            }
            DirectoryInfo rootFolder = new DirectoryInfo(Path.Combine(Path.GetTempPath(), uniqueFolderName.ToString()));

            if (!rootFolder.Exists || forceOverwrite)
            {
                FileInfo stagingZipFile = FileStorageUtil.ResolveStorageItemFileInfo(storageItemFk);

                rootFolder.Create();
                using (ZipFile zip = ZipFile.Read(stagingZipFile.FullName))
                {
                    zip.ExtractAll(rootFolder.FullName, ExtractExistingFileAction.OverwriteSilently);
                }
            }

            return rootFolder;
        }

        public static FileInfo ResolveStorageItemFileInfo(Guid storageItemFk)
        {
            //StorageItem storageItem = new StorageItemManager().Select(storageItemFk);
            RemoteFile remoteFile = new FileManager().SelectByStorageItemPk(storageItemFk);
            Guard.NotNull(remoteFile, String.Format("Storage Item does not exist {0}", storageItemFk));

            FileInfo payloadPath = new FileInfo(remoteFile.StoredLocationPath);
            if (!payloadPath.Exists)
            {
                throw new FileNotFoundException(String.Format("CalculatedFullPath for storageItem {0} was {1} but that file does not exist."
                    , storageItemFk, payloadPath.FullName));
            }
            return payloadPath;
        }

        public static Guid StoreFileAsZip(Stream content, string fileName, string integrationStorageCategoryTypeCode)
        {
            var zipFileName = new FileInfo(Path.ChangeExtension(fileName, "zip")).Name;

            using (var stream = new MemoryStream())
            {
                using (var zipFile = new ZipFile())
                {
                    zipFile.AddEntry(fileName, content);
                    zipFile.Save(stream);
                }
                stream.Position = 0;
                return StoreFile(stream, zipFileName, integrationStorageCategoryTypeCode, StorageItemContentType.Codes.ZIP);
            }
        }

        public static Guid StoreFileAsZip(string content, string fileName, string integrationStorageCategoryTypeCode)
        {

            var zipFileName = new FileInfo(Path.ChangeExtension(fileName, "zip")).Name;

            using (var stream = new MemoryStream())
            {
                using (var zipFile = new ZipFile())
                {
                    zipFile.AddEntry(fileName, content);
                    zipFile.Save(stream);
                }
                stream.Position = 0;
                return StoreFile(stream, zipFileName, integrationStorageCategoryTypeCode, StorageItemContentType.Codes.ZIP);
            }
        }

        public static Guid StoreFile(string content, string fileName, string integrationStorageCategoryTypeCode, string contentExtension)
        {
            Guard.IsTrue(new StorageItemContentType().GetReferenceCodeEntityEnumList().Contains(contentExtension),
                         "Content Extension is invalid");

            using (var stream = new MemoryStream())
            {
                using (var zipFile = new ZipFile())
                {
                    zipFile.AddEntry(fileName, content);
                    zipFile.Save(stream);
                }
                stream.Position = 0;
                return StoreFile(stream, fileName, integrationStorageCategoryTypeCode, contentExtension);
            }
        }

        public static Guid StoreFile(byte[] bytes, string fileName, string storageItemContentTypeCode, string integrationStorageCategoryTypeCode)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return StoreFile(stream, fileName, integrationStorageCategoryTypeCode, storageItemContentTypeCode);
            }
        }

        public static Guid StoreFile(Stream stream, string filename, string integrationStorageCategoryTypeCode, string storageItemContentTypeCodeValue = null)
        {
            // Strip any folder information
            filename = new FileInfo(filename).Name;

            LocalReferenceCodeCache.Add<IntegrationStorageCategoryType>(new IntegrationStorageCategoryTypeManager());
            IntegrationStorageCategoryType integrationStorageCategoryType = LocalReferenceCodeCache
                .SelectByCodeValue<IntegrationStorageCategoryType>(integrationStorageCategoryTypeCode);
            Guard.CodeValueResolved(integrationStorageCategoryType, "IntegrationStorageCategoryType", integrationStorageCategoryTypeCode);

            if (String.IsNullOrWhiteSpace(storageItemContentTypeCodeValue))
            {
                String fileExtension = new FileInfo(filename).Extension.Replace(".", "");
                if (!String.IsNullOrWhiteSpace(fileExtension))
                {
                    LocalReferenceCodeCache.Add<StorageItemContentType>(new StorageItemContentTypeManager());

                    StorageItemContentType storageItemContentType = LocalReferenceCodeCache.SelectByCodeValue<StorageItemContentType>(fileExtension);
                    storageItemContentTypeCodeValue = fileExtension;
                    if (storageItemContentType != null)
                    {
                        // Try to find the content type using the file extension.
                        storageItemContentTypeCodeValue = storageItemContentType.CodeValue;
                    }
                }
            }

            RemoteFileWithStream remoteFileWithStream = new RemoteFileWithStream()
            {
                RequestingApplicationTypeCodeValue = ApplicationTypeCode.Codes.DataIntegration,
                Category = integrationStorageCategoryType.CodeValue,
                FileByteStream = stream,
                FileName = filename,
                ContentType = storageItemContentTypeCodeValue,
            };


            RemoteFileWithMessages remoteFileWithMessages = new FileManager().UploadFileStream(remoteFileWithStream);

            if (remoteFileWithMessages.StorageItemPk == Guid.Empty)
            {
                throw new Exception(string.Format("Tried to save a file {0} but the Shared Storage failed to return an Id!", filename));
            }

            return remoteFileWithMessages.StorageItemPk;
        }

        public static Guid StoreFile(FileInfo fileInfo, string integrationStorageCategoryTypeCode)
        {
            Guard.FileExists(fileInfo.FullName, "fileToStore");

            using (FileStream fileStream = File.OpenRead(fileInfo.FullName))
            {
                return StoreFile(fileStream, fileInfo.Name, integrationStorageCategoryTypeCode);
            }
        }

        public static Guid StoreFile(StringBuilder fileContents,
                    string fileName,
                    string associatedFileTypeCode,
                    string integrationStorageCategoryTypeCode
            )
        {
            string storageItemContentTypeCode = null;
            if (AssociatedFileType.Codes.BusinessObjectDocument == associatedFileTypeCode)
            {
                storageItemContentTypeCode = StorageItemContentType.Codes.XML;
            }
            else
            {
                storageItemContentTypeCode = StorageItemContentType.Codes.TXT;
            }

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContents.ToString())))
            {
                return StoreFile(stream, fileName, integrationStorageCategoryTypeCode, storageItemContentTypeCode);
            }
        }

        public static Guid StoreFile(XDocument xml,
            string fileName,
            string integrationStorageCategoryTypeCode
            )
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml.ToString())))
            {
                return StoreFile(stream, fileName, integrationStorageCategoryTypeCode, StorageItemContentType.Codes.XML);
            }
        }*/
    }
}
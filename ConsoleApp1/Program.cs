
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;

namespace ConsoleApp1
{
    class Program
    {
        private static string getCurrentPath()
        {
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if(!path.EndsWith("\\") || !path.EndsWith("/"))
            {
                path += "\\";
            }
            return path;
        }

        private static void uploadPets(CloudBlobContainer blobContainer)
        {
            // upload item
            CloudBlockBlob cloudBlockBlob;
            String petKind;

            // from file
            petKind = "dog.jpg";
            if(File.Exists(getCurrentPath() + petKind))
            {
                cloudBlockBlob = blobContainer.GetBlockBlobReference(petKind);
                if (!cloudBlockBlob.Exists())
                {
                    cloudBlockBlob.UploadFromFile(getCurrentPath() + petKind);
                }
                else
                {
                    Console.WriteLine($"Item {petKind} exists! Skipped...");
                }
            }
            else
            {
                Console.WriteLine($"File {petKind} does not exists! Skipped...");
            }

            // from stream
            petKind = "cat.jpg";
            if (File.Exists(getCurrentPath() + petKind))
            {
                cloudBlockBlob = blobContainer.GetBlockBlobReference(petKind);
                if (!cloudBlockBlob.Exists())
                {
                    using (var fileStream = File.OpenRead(getCurrentPath() + petKind))
                    {
                        cloudBlockBlob.UploadFromStream(fileStream);
                    }
                }
                else
                {
                    Console.WriteLine($"Local item {petKind} exists! Skipped...");
                }
            }
            else
            {
                Console.WriteLine($"Local item {petKind} does not exists! Skipped...");
            }
        }

        private static void listPets(CloudBlobContainer blobContainer)
        {
            IEnumerable<IListBlobItem> blobs = blobContainer.ListBlobs();
            foreach (IListBlobItem item in blobs)
            {
                if(item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blobFile = (CloudBlockBlob)item;
                    Console.WriteLine($"Remote blob: {blobFile.Name}");
                }
                else if(item.GetType() == typeof(CloudBlobDirectory))
                {
                    CloudBlobDirectory blobDir = (CloudBlobDirectory)item;
                    Console.WriteLine($"Remote dir: {blobDir.Uri}");
                }
                else
                {
                    Console.WriteLine($"Remote item: {item.GetType()}");
                }
            }
        }

        private static void downloadPets(CloudBlobContainer blobContainer)
        {
            IEnumerable<IListBlobItem> blobs = blobContainer.ListBlobs(null, false);
            foreach (IListBlobItem item in blobs)
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blockBlob = (CloudBlockBlob)item;

                    if (!File.Exists(getCurrentPath() + blockBlob.Name))
                    {
                        using (var fileStream = File.OpenWrite(getCurrentPath() + blockBlob.Name))
                        {
                            blockBlob.DownloadToStream(fileStream);
                        }
                        Console.WriteLine($"Blob {blockBlob.Name} has been downloaded");
                    }
                    else
                    {
                        Console.WriteLine($"Blob {blockBlob.Name} has not been downloaded, local file exists");
                    }
                }
                else if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    CloudBlobDirectory blobDir = (CloudBlobDirectory)item;
                    Console.WriteLine($"Remote dir: {blobDir.Uri}");
                }
                else
                {
                    Console.WriteLine($"Item {item.GetType()} has been skipped");
                }
            }
        }

        private static void cloneCat(CloudBlobContainer blobContainer)
        {
            CloudBlockBlob block = blobContainer.GetBlockBlobReference("cat.jpg");

            if (block.Exists())
            {
                CloudBlockBlob blockCopy = blobContainer.GetBlockBlobReference("cloned/cat.jpg");
                AsyncCallback callbackCopy = new AsyncCallback(x => Console.WriteLine("Blob copy completed!"));

                blockCopy.BeginStartCopy(block, callbackCopy, null);
            }
            else
            {
                Console.WriteLine("Cat was not found!");
            }
        }

        private static void setMetadata(CloudBlobContainer blobContainer)
        {
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference("cat.jpg");
            if (blockBlob.Exists())
            {
                blockBlob.Metadata.Clear();
                blockBlob.Metadata.Add("Breed", "Std");
                blockBlob.Metadata.Add("Updated", DateTime.Now.ToString("dd.MM.yyyy hh.mm.ss"));
                blockBlob.SetMetadata();

                Console.WriteLine("Metadata for cat has been set");
            }
        }

        private static void getMetadata(CloudBlobContainer blobContainer)
        {
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference("cat.jpg");
            blockBlob.FetchAttributes();
            foreach (var item in blockBlob.Metadata)
            {
                Console.WriteLine($"{item.Key} -> {item.Value}");
            }
        }

        static void Main(string[] args)
        {
            // read connection string from config
            String connectionString = CloudConfigurationManager.GetSetting("StorageConnection");

            // set connection string
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // init connection
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // get reference to container
            CloudBlobContainer blobContainer = blobClient.GetContainerReference("images");

            // create container if not exists
            blobContainer.CreateIfNotExists(BlobContainerPublicAccessType.Blob);

            // upload files
            uploadPets(blobContainer);

            // list pets
            listPets(blobContainer);

            // download files
            downloadPets(blobContainer);

            // clone cat
            cloneCat(blobContainer);

            // metadata
            setMetadata(blobContainer);

            // dump metadata
            getMetadata(blobContainer);

            // interrupt
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(false);
        }
    }
}

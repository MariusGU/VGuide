using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace VirtualGuidePlatform.Data.Repositories
{
    public interface IFilesRepository
    {
        Task<string> UploadFileToFirebase(IFormFile file, string newname, string folder);
        Task<bool> DeleteFile(string path);
        Task<string> DownloadFile(string path);
        Task<string> UploadFileFromMemory(string fileName, string newname, string folder);
        Task<string> ReuploadFile(string uri, string newname, string folder);
    }

    public class FilesRepository : IFilesRepository
    {
        private readonly IConfiguration _configuration;

        public FilesRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> UploadFileToFirebase(IFormFile file, string newname, string folder)
        {
            Stream stream;
            if (file.Length > 0)
            {
                stream = file.OpenReadStream();
                var auth = new FirebaseAuthProvider(new FirebaseConfig(_configuration.GetConnectionString("FirebaseApiKey")));
                var a = await auth.SignInWithEmailAndPasswordAsync(_configuration.GetConnectionString("FirebaseEmail"), _configuration.GetConnectionString("FirebasePass"));
                var cancellation = new CancellationTokenSource();
                var task = new FirebaseStorage(_configuration.GetConnectionString("FirebaseBucket"),
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),

                    }
                    ).Child(folder).Child(newname).PutAsync(stream);

                task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                try
                {
                    string link = await task;
                    Console.WriteLine(link);
                    return link;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Nepavyko");
                }
            }
            return "";
        }
        public async Task<bool> DeleteFile(string path)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(_configuration.GetConnectionString("FirebaseApiKey")));
            var a = await auth.SignInWithEmailAndPasswordAsync(_configuration.GetConnectionString("FirebaseEmail"), _configuration.GetConnectionString("FirebasePass"));

            var task = new FirebaseStorage(_configuration.GetConnectionString("FirebaseBucket"),
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                }
                ).Child(path).DeleteAsync();

            try
            {
                await task;
            }
            catch(Exception ex)
            {
                Console.WriteLine(task.IsFaulted);
                return false;
            }
            return true;
        }
        public async Task<string> DownloadFile(string path)
        {
            var splited = path.Split('/');
            var secondSplit = splited[splited.Length - 1].Split('.');
            var thirdSplit = secondSplit[secondSplit.Length - 1].Split('?');

            string targetFileName = "temp." + thirdSplit[0];
            using (WebClient client = new WebClient())
            {
                Uri downloadURI = new Uri(path);
                client.DownloadFile(downloadURI, targetFileName);
            }
            if (File.Exists(targetFileName))
            {
                return targetFileName;
            }
            return "";
        }
        public async Task<string> ReuploadFile(string uri, string newname, string folder)
        {
            var splited = uri.Split('/');
            var secondSplit = splited[splited.Length - 1].Split('.');
            var thirdSplit = secondSplit[secondSplit.Length - 1].Split('?');

            var nameSplitFirst = splited[splited.Length - 1].Split("%2F");
            var nameSplitSecond = nameSplitFirst[1].Split('?');
            var nameDelete = folder + "/" + nameSplitSecond[0];

            Console.WriteLine(nameDelete);

            string targetFileName = "temp." + thirdSplit[0];

            Console.WriteLine(secondSplit[secondSplit.Length - 2]);

            string downloadedPath = await DownloadFile(uri);
            if (downloadedPath == "")
            {
                return "";
            }

            var isDeleted = await DeleteFile(nameDelete);
            if (isDeleted != true)
            {
                Console.WriteLine("Delete path bad");
                return "";
            }

            var link = await UploadFileFromMemory(downloadedPath, newname, folder);

            if(link == "")
            {
                return "";
            }

            return link;

        }

        public async Task<string> UploadFileFromMemory(string fileName, string newname, string folder)
        {
            Stream stream;
            var file = File.Exists(fileName);
            if (File.Exists(fileName))
            {
                stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                var auth = new FirebaseAuthProvider(new FirebaseConfig(_configuration.GetConnectionString("FirebaseApiKey")));
                var a = await auth.SignInWithEmailAndPasswordAsync(_configuration.GetConnectionString("FirebaseEmail"), _configuration.GetConnectionString("FirebasePass"));
                var cancellation = new CancellationTokenSource();

                var task = new FirebaseStorage(_configuration.GetConnectionString("FirebaseBucket"),
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),

                    }
                    ).Child(folder).Child(newname).PutAsync(stream);

                task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                try
                {
                    string link = await task;
                    Console.WriteLine(link);
                    return link;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Nepavyko");
                    return "";
                }
            }
                
            return "";
        }
    }
}

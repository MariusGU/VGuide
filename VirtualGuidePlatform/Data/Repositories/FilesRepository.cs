using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VirtualGuidePlatform.Data.Repositories
{
    public interface IFilesRepository
    {
        Task<string> UploadFileToFirebase(IFormFile file, string newname, string folder);
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
    }
}

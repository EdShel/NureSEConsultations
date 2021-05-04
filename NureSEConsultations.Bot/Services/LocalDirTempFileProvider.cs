using System;
using System.IO;

namespace NureSEConsultations.Bot.Services
{
    public class LocalDirTempFileProvider : ITempFileProvider
    {
        private readonly string folderName;

        public LocalDirTempFileProvider(string folderName)
        {
            this.folderName = folderName;
        }

        public string GetTempFile(string extension)
        {
            return Path.Combine(this.folderName, Guid.NewGuid() + "." + extension);
        }
    }
}

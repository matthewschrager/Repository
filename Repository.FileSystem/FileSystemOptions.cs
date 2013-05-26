using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Serialization;

namespace Repository.FileSystem
{
    public class FileSystemOptions<T>
    {
        //===============================================================
        public FileSystemOptions()
        {
            Encoder = new EncodingEncoder<T>(Encoding.UTF8, new JsonSerializer<T>());
            FolderPath = "";
            FileExtension = ".txt";
        }
        //===============================================================
        public Encoder<T> Encoder { get; set; }
        //===============================================================
        public String FolderPath { get; set; }
        //===============================================================
        public String FileExtension { get; set; }
        //===============================================================
    }
}

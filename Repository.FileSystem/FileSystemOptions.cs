using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
            Serializer = new JsonSerializer<IEnumerable<T>>();
            FolderPath = "";
            FileExtension = ".txt";
            StreamGenerator = new StandardStreamGenerator();
            UseTypeNameFolder = false;
        }
        //===============================================================
        public ISerializer<IEnumerable<T>> Serializer { get; set; }
        //===============================================================
        public String FolderPath { get; set; }
        //===============================================================
        public String FileExtension { get; set; }
        //===============================================================
        public StreamGenerator StreamGenerator { get; set; }
        //===============================================================
        public bool UseTypeNameFolder { get; set; }
        //================================================================================
    }

    public abstract class StreamGenerator
    {
        //================================================================================
        public abstract Stream GetReadStream(String filePath);
        //================================================================================
        public abstract Stream GetWriteStream(String filePath);
        //================================================================================
    }

    public class StandardStreamGenerator : StreamGenerator
    {
        //================================================================================
        public override Stream GetReadStream(string filePath)
        {
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
        //================================================================================
        public override Stream GetWriteStream(string filePath)
        {
            return new FileStream(filePath, FileMode.Create, FileAccess.Write);
        }
        //================================================================================
    }

    public class GZipStreamGenerator : StreamGenerator
    {
        //================================================================================
        public override Stream GetReadStream(string filePath)
        {
            return new GZipStream(new FileStream(filePath, FileMode.Open, FileAccess.Read), CompressionMode.Decompress);
        }
        //================================================================================
        public override Stream GetWriteStream(string filePath)
        {
            return new GZipStream(new FileStream(filePath, FileMode.Create, FileAccess.Write), CompressionMode.Compress);
        }
        //================================================================================
    }
}

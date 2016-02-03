using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Persistent.Base;
using System.IO;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
namespace NKD.Module.BusinessObjects
{

    public class FileValueConverter : ValueConverter
    {

        public override Type StorageType { get { return typeof(byte[]); } }
        public override object ConvertToStorageType(object value)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                return ((File)value).FileBytes;
            }
        }
        public override object ConvertFromStorageType(object value)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                var b = ((byte[])value);
                return new File(ref b);
            }
        }
    }

    [FileAttachmentAttribute("FileInfo")]
    public partial class FileData : DevExpress.Persistent.Base.IFileData
    {
        File fFile;
        [Size(SizeAttribute.Unlimited)]
        [ValueConverter(typeof(FileValueConverter))]
        [NonPersistent()]
        public File FileInfo
        {
            get { if (fFile == null) fFile = new File(ref _FileBytes, ref _FileName, this); return fFile; }
            set { if (fFile == null) fFile = value; FileBytes = value.FileBytes; FileName = value.FileName; }
        }

        [Size(SizeAttribute.Unlimited)]
        [ValueConverter(typeof(ImageValueConverter))]
        [NonPersistent()]
        public System.Drawing.Image FilePreview
        {            
            get 
            {
                if (FileBytes == null)
                    return null;
                try
                {
                    System.Drawing.ImageConverter cv = new System.Drawing.ImageConverter();
                    return cv.ConvertFrom(FileBytes) as System.Drawing.Image;
                }
                catch { return null; }
            }
        }


        public int Size { get { return File.Length(FileBytes); } }
        public void Clear() { FileBytes = null; }
        public void LoadFromStream(string fileName, Stream stream)
        {
            FileName = fileName;
            FileBytes = File.ReadToEnd(stream);
        }
        public void SaveToStream(Stream stream)
        {
            stream.Write(FileBytes, 0, FileBytes.Length);
        }
    }


    public partial class File : DevExpress.Persistent.Base.IFileData
    {

        
        public File() {  }
        public File(ref byte[] bytes) { FileBytes = bytes; }
        public File(ref byte[] bytes, ref string filename, FileData fileData) { fFileData = fileData; FileBytes = bytes; FileName = filename; }        
        public File(DevExpress.Xpo.UnitOfWork uow) : this() { }

        private FileData fFileData;
        private string fFileName;
        private byte[] fFileBytes;
        public string FileName
        {
            get { if (fFileData != null) return fFileData.FileName; else return fFileName; }
            set { if (fFileData != null) fFileData.FileName = value; else fFileName = value; }
        }

        public byte[] FileBytes
        {
            get { if (fFileData != null) return fFileData.FileBytes; else return fFileBytes; }
            set { if (fFileData != null) fFileData.FileBytes = value; else fFileBytes = value; }
        }


        public int Size { get { return Length(FileBytes); } }
        public void Clear() { FileBytes = null; }
        public void LoadFromStream(string fileName, Stream stream)
        {
            FileName = fileName;
            FileBytes = ReadToEnd(stream);
        }
        public void SaveToStream(Stream stream)
        {
            stream.Write(FileBytes, 0, FileBytes.Length);
        }

        public static int Length(byte[] bytes) { if (bytes == null) return 0; else return bytes.Length; } 
        public static void Clear(ref byte[] bytes) { if (bytes != null) Array.Resize<byte>(ref bytes, 0); }

        public static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }
    }

}

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows;
using System.IO.Compression;



namespace Saekano_Depack
{
    class Program
    {

        static int INT_LENGTH = 4;
        static int INT_OFFSET = 4;
        static int LINE_OFFSET = 16;


        static void Main(string[] args)
        {

#if DEBUG
            string path = Console.ReadLine();
            byte[] datFile = File.ReadAllBytes(path);
#else
            byte[] datFile = File.ReadAllBytes(args[0]);
#endif

            ExtractFile(datFile);
        }

        private static void ExtractFile(byte[] dataFile, string directory = "Output\\")
        {
            //Parse the GPDA Header
            //0x00 - 0x03 = Type (Should be GPDA)
            //0x04 - 0x07 = File Size of bin file
            //0x08 - 0x0B = All Zeros (can be ignored for depack)
            //0x0C - 0x0F = File Count in bin file
            string binType = Encoding.ASCII.GetString(dataFile.Take(INT_LENGTH).ToArray());
            long binSize = BitConverter.ToInt32(dataFile.Skip(INT_OFFSET).Take(INT_LENGTH).ToArray());
            long fileCount = BitConverter.ToInt32(dataFile.Skip(INT_OFFSET * 3).Take(INT_LENGTH).ToArray());

            Directory.CreateDirectory(directory);
#if DEBUG
            Console.Write("Type:{0}, Bin Size:{1}, File Count:{2}", binType, binSize, fileCount);
#endif

            for (int i = 1; i <= fileCount; i++)
            {
                //Parse File metadata header
                //0x0 - 0x3 = Offset/Position
                //0x4 - 0x7 = Is Compressed?
                //0x8 - 0xB = File Size
                //0xC - 0Xf = Offset for File Name
                int offset = BitConverter.ToInt32(dataFile.Skip(LINE_OFFSET * i).Take(INT_LENGTH).ToArray());
                bool isCompressed = BitConverter.ToBoolean(dataFile, ((LINE_OFFSET * i) + 5));
                int fileSize = BitConverter.ToInt32(dataFile.Skip((LINE_OFFSET * i) + (INT_OFFSET * 2)).Take(INT_LENGTH).ToArray());
                int nameOffset = BitConverter.ToInt32(dataFile.Skip((LINE_OFFSET * i) + (INT_OFFSET * 3)).Take(INT_LENGTH).ToArray());

                //Getting the file name
                // First four bytes at offset are the file name length then just the filename
                int fileNameLength = BitConverter.ToInt32(dataFile.Skip(nameOffset).Take(INT_LENGTH).ToArray());
                string fileName = Encoding.ASCII.GetString(dataFile.Skip(nameOffset + INT_OFFSET).Take(fileNameLength).ToArray());
                if(fileName.EndsWith(".gz") || fileName.EndsWith(".gz ")){
                    fileName = fileName.Remove(fileName.LastIndexOf("."));
                    isCompressed = true;
                }
#if DEBUG
                Console.WriteLine("Offset:{0}, Compressed:{1}, File Size:{2}, Name Offset:{3}", offset, isCompressed, fileSize, nameOffset);
                Console.WriteLine("File Name:" + fileName);
#endif
                //Copy the file to its own array for processing
                byte[] subBinFile = new byte[fileSize];
                Buffer.BlockCopy(dataFile, offset, subBinFile, 0, fileSize);

                //Have to check if the file is another GPDA .bin file and if so recursively process it
                string fileType = Encoding.ASCII.GetString(subBinFile.Take(INT_LENGTH).ToArray());
                if (fileType == "GPDA")
                {
                    ExtractFile(subBinFile, directory + fileName.Remove(fileName.Length - 5) + "\\");
                }
                else
                {
                    byte[] gzipHeader = { 31, 139, 8, 0, };
                    byte[] compareer = subBinFile.Take(INT_LENGTH).ToArray();
                    bool compressedHeader = (gzipHeader.SequenceEqual(compareer));
                    if (isCompressed || compressedHeader)
                    {
                        Decompress(subBinFile, directory + fileName);
                    }
                    else
                    {
                        File.WriteAllBytes(directory + fileName, subBinFile);
                    }
                }
            }
        }

        private static void Decompress(byte[] data, string fileName)
        {

            using (var dataStream = new MemoryStream(data))
            using (var gzipStream = new GZipStream(dataStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                try
                {
                    gzipStream.CopyTo(outputStream);
                    byte[] output = outputStream.ToArray();
                    if (Encoding.ASCII.GetString(output.Take(INT_LENGTH).ToArray()) == "GPDA")
                    {
                        ExtractFile(output, fileName.Remove(fileName.LastIndexOf(".")) + "\\");
                    }
                    else
                    {
                        File.WriteAllBytes(fileName, output);
                    }
                }
                //For the rare files that arne't compressed
                catch
                {
                    Console.WriteLine("ERROR DECOMPRESSING, FILE MIGHT NOT BE COMPRESSED?");
                    File.WriteAllBytes(fileName, data);
                }
            }
        }
    }
}

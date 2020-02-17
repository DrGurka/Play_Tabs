using System;
using System.IO;
using System.Security.Cryptography;
using zlib;

namespace Play_Tabs.Tools
{
    public static class RijndaelEncryptor
    {
        #region RS1

        public static byte[] DLCKey = new byte[32]
        {
            0xFA, 0x6F, 0x4F, 0x42, 0x3E, 0x66, 0x9F, 0x9E,
            0x6A, 0xD2, 0x3A, 0x2F, 0x8F, 0xE5, 0x81, 0x88,
            0x63, 0xD9, 0xB8, 0xFD, 0xED, 0xDF, 0xFE, 0xBD,
            0x12, 0xB2, 0x7F, 0x76, 0x80, 0xD1, 0x51, 0x41
        };

        public static byte[] PCFilesKey = new byte[32]
        {
            0xB8, 0x7A, 0x00, 0xBD, 0xB8, 0x9C, 0x21, 0x03,
            0xA3, 0x94, 0xC0, 0x44, 0x71, 0x51, 0xEE, 0xC4,
            0x3C, 0x3F, 0x72, 0x17, 0xCA, 0x7F, 0x44, 0xC1,
            0xE4, 0x36, 0xFC, 0xFC, 0x84, 0xE6, 0xE7, 0x15
        };

        #endregion

        #region RS2

        public static byte[] PsarcKey = new byte[32]
        {
            0xC5, 0x3D, 0xB2, 0x38, 0x70, 0xA1, 0xA2, 0xF7,
            0x1C, 0xAE, 0x64, 0x06, 0x1F, 0xDD, 0x0E, 0x11,
            0x57, 0x30, 0x9D, 0xC8, 0x52, 0x04, 0xD4, 0xC5,
            0xBF, 0xDF, 0x25, 0x09, 0x0D, 0xF2, 0x57, 0x2C
        };

        public static byte[] SngKeyMac = new byte[32]
        {
            0x98, 0x21, 0x33, 0x0E, 0x34, 0xB9, 0x1F, 0x70,
            0xD0, 0xA4, 0x8C, 0xBD, 0x62, 0x59, 0x93, 0x12,
            0x69, 0x70, 0xCE, 0xA0, 0x91, 0x92, 0xC0, 0xE6,
            0xCD, 0xA6, 0x76, 0xCC, 0x98, 0x38, 0x28, 0x9D
        };

        public static byte[] SngKeyPC = new byte[32]
        {
            0xCB, 0x64, 0x8D, 0xF3, 0xD1, 0x2A, 0x16, 0xBF,
            0x71, 0x70, 0x14, 0x14, 0xE6, 0x96, 0x19, 0xEC,
            0x17, 0x1C, 0xCA, 0x5D, 0x2A, 0x14, 0x2E, 0x3E,
            0x59, 0xDE, 0x7A, 0xDD, 0xA1, 0x8A, 0x3A, 0x30
        };
        //metadata
        public static byte[] PCMetaDatKey = new byte[32]
        {
            0x5F, 0xB0, 0x23, 0xEF, 0x19, 0xD5, 0xDC, 0x37,
            0xAD, 0xDA, 0xC8, 0xF0, 0x17, 0xF8, 0x8F, 0x0E,
            0x98, 0x18, 0xA3, 0xAC, 0x2F, 0x72, 0x46, 0x96,
            0xA5, 0x9D, 0xE2, 0xBF, 0x05, 0x25, 0x12, 0xEB
        };
        //profile and other cdr profile.json stuff common for RS2\RS1
        public static byte[] PCSaveKey = new byte[32]
        {
            0x72, 0x8B, 0x36, 0x9E, 0x24, 0xED, 0x01, 0x34,
            0x76, 0x85, 0x11, 0x02, 0x18, 0x12, 0xAF, 0xC0,
            0xA3, 0xC2, 0x5D, 0x02, 0x06, 0x5F, 0x16, 0x6B,
            0x4B, 0xCC, 0x58, 0xCD, 0x26, 0x44, 0xF2, 0x9E
        };

        public static byte[] IniKey_Mac = new byte[32]
        {
            0x37, 0x8B, 0x90, 0x26, 0xEE, 0x7D, 0xE7, 0x0B,
            0x8A, 0xF1, 0x24, 0xC1, 0xE3, 0x09, 0x78, 0x67,
            0x0F, 0x9E, 0xC8, 0xFD, 0x5E, 0x72, 0x85, 0xA8,
            0x64, 0x42, 0xDD, 0x73, 0x06, 0x8C, 0x04, 0x73
        };

        #endregion

        /// <summary>
        /// Unpacks zipped data.
        /// </summary>
        /// <param name="str">In Stream.</param>
        /// <param name="outStream">Out stream.</param>
        /// <param name = "plainLen">Data size after decompress.</param>
        /// <param name = "rewind">Manual control for stream seek position.</param>
        public static void Unzip(Stream str, Stream outStream, bool rewind = true)
        {
            int len;
            var buffer = new byte[65536];
            var zOutputStream = new ZInputStream(str);

            while ((len = zOutputStream.read(buffer, 0, buffer.Length)) > 0)
                outStream.Write(buffer, 0, len);

            zOutputStream.Close();
            buffer = null;

            if (rewind)
            {
                outStream.Position = 0;
                outStream.Flush();
            }
        }

        public static void Unzip(byte[] array, Stream outStream, bool rewind = true)
        {
            Unzip(new MemoryStream(array), outStream, rewind);
        }

        public static void DecryptPSARC(Stream input, Stream output, long len)
        {
            using (var rij = new RijndaelManaged())
            {
                InitRijndael(rij, PsarcKey, CipherMode.CFB);
                Crypto(input, output, rij.CreateDecryptor(), len);
            }
        }

        private static bool activated = false;
        private static void InitRijndael(Rijndael rij, byte[] key, CipherMode cipher)
        {
            if (!activated)
            {
                activated = true;
            }

            rij.Padding = PaddingMode.None;
            rij.Mode = cipher;
            rij.BlockSize = 128;
            rij.IV = new byte[16];
            rij.Key = key;          // byte[32]
        }

        private static void Crypto(Stream input, Stream output, ICryptoTransform transform, long len)
        {
            var buffer = new byte[512];
            int pad = buffer.Length - (int)(len % buffer.Length);
            var coder = new CryptoStream(output, transform, CryptoStreamMode.Write);
            while (input.Position < len)
            {
                int size = (int)Math.Min(len - input.Position, buffer.Length);
                input.Read(buffer, 0, size);
                coder.Write(buffer, 0, size);
            }

            if (pad > 0)
                coder.Write(new byte[pad], 0, pad);

            coder.Flush();
            output.Seek(0, SeekOrigin.Begin);
            output.Flush();
        }
    }
}

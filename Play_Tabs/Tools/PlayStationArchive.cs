using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Play_Tabs.Tools
{
    class PlayStationArchive : IDisposable
    {

        private class Header
        {
            public uint MagicNumber;
            public uint VersionNumber;
            public uint CompressionMethod;
            public uint TotalTOCSize;
            public uint TOCEntrySize;
            public uint NumFiles;
            public uint BlockSizeAlloc;
            public uint ArchiveFlags;
            public Header()
            {
                MagicNumber = 1347633490; //'PSAR'
                VersionNumber = 65540; //1.4
                CompressionMethod = 2053925218; //'zlib' (also available 'lzma')
                TOCEntrySize = 30;//bytes
                //NumFiles = 0;
                BlockSizeAlloc = 65536; //Decompression buffer size = 64kb
                ArchiveFlags = 0; //It's bitfield actually, see PSARC.bt
            }
        }

        private Header _header;
        private List<Entry> _toc;
        public List<Entry> TOC { get { return _toc; } }
        private uint[] _zBlocksSizeList;
        private int bNum { get { return (int)Math.Log(_header.BlockSizeAlloc, byte.MaxValue + 1); } }
        private bool UseMemory = false;

        public PlayStationArchive()
        {
            _header = new Header();
            _toc = new List<Entry> { new Entry() };
        }

        #region IDisposable implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _header = null;
            foreach (var entry in TOC.Where(entry => entry.Data != null))
                entry.Data.Dispose();

            TOC.Clear();

            if (_reader != null) _reader.Dispose();
        }
        #endregion

        #region Helpers Inflater/Deflater

        /// <summary>
        /// Inflates selected entry.
        /// </summary>
        /// <param name="entry">Entry to unpack.</param>
        /// <param name = "destfilepath">Destination file used instead of the temp file.</param>
        public void InflateEntry(Entry entry, string destfilepath = "")
        {
            if (entry.Length == 0) return;//skip empty files
            // Decompress Entry
            const int zHeader = 0x78DA;
            uint zChunkID = entry.zIndexBegin;
            int blockSize = (int)_header.BlockSizeAlloc;
            //bool isZlib = _header.CompressionMethod == 2053925218;
            if (destfilepath.Length > 0)
                entry.Data = new FileStream(destfilepath, FileMode.Create, FileAccess.Write, FileShare.Read);
            else
            {
                if (UseMemory)
                    entry.Data = new MemoryStreamExtension();
                else
                    entry.Data = new TempFileStream();
            }

            _reader.BaseStream.Position = (long)entry.Offset;

            do
            {
                // check for corrupt CDLC content and catch exceptions
                try
                {
                    if (_zBlocksSizeList[zChunkID] == 0U) // raw. full cluster used.
                    {
                        entry.Data.Write(_reader.ReadBytes(blockSize), 0, blockSize);
                    }
                    else
                    {
                        var num = _reader.ReadUInt16();
                        _reader.BaseStream.Position -= 2L;

                        var array = _reader.ReadBytes((int)_zBlocksSizeList[zChunkID]);
                        if (num == zHeader)
                        {
                            // compressed
                            try
                            {
                                RijndaelEncryptor.Unzip(array, entry.Data, false);
                            }
                            catch (Exception ex) //IOException
                            {
                                // corrupt CDLC zlib.net exception ... try to unpack
                                if (String.IsNullOrEmpty(entry.Name))
                                    Console.WriteLine(String.Format(@"CDLC contains a zlib exception.{1}Warning: {0}", ex.Message, Environment.NewLine));
                                else
                                    Console.WriteLine(String.Format(@"CDLC contains a broken datachunk in file '{0}'.{2}Warning Type 1: {1}", entry.Name.Split('/').Last(), ex.Message, Environment.NewLine));
                            }
                        }
                        else // raw. used only for data(chunks) smaller than 64 kb
                        {
                            entry.Data.Write(array, 0, array.Length);
                        }
                    }

                    zChunkID += 1;
                }
                catch (Exception ex) // index is outside the bounds of the array 
                {
                    // corrupt CDLC data length ... try to unpack
                    Console.WriteLine(String.Format(@"CDLC contains a broken datachunk in file '{0}'.{2}Warning Type 2: {1}", entry.Name.Split('/').Last(), ex.Message, Environment.NewLine));
                    break;
                }

            } while (entry.Data.Length < (long)entry.Length);

            entry.Data.Seek(0, SeekOrigin.Begin);
            entry.Data.Flush();
        }

        /// <summary>
        /// Inflates all entries in current psarc.
        /// </summary>
        public void InflateEntries()
        {
            foreach (var current in _toc)
            {
                // We really can use Parallel here.
                InflateEntry(current);
            }
        }

        /// <summary>
        /// Reads file names from the manifest.
        /// </summary>
        public void ReadManifest()
        {
            var toc = _toc[0];

            toc.Name = "NamesBlock.bin";
            InflateEntry(toc);
            using (var bReader = new StreamReader(toc.Data, true))
            {
                var count = _toc.Count;
                var data = bReader.ReadToEnd().Split('\n');//0x0A

                Parallel.For(0, data.Length, i =>
                {
                    if (i + 1 != count)
                        _toc[i + 1].Name = data[i];
                });
            }

            // commented out to leave NamesXblock.bin for debugging
            // _toc.RemoveAt(0);
        }

        void ParseTOC()
        {
            // Parse TOC Entries
            for (int i = 0, tocFiles = (int)_header.NumFiles; i < tocFiles; i++)
            {
                _toc.Add(new Entry
                {
                    Id = i,
                    MD5 = _reader.ReadBytes(16),
                    zIndexBegin = _reader.ReadUInt32(),
                    Length = _reader.ReadUInt40(),
                    Offset = _reader.ReadUInt40()
                });/* FIXME: general idea was to implement parallel inflate route, still need to re-think this.
                if (i == 0) continue;
                if (i == tocFiles - 1)
                    _toc[i].zDatalen = (ulong)_reader.BaseStream.Length - _toc[i].Offset; //HACK: fails if psarc is truncated.
                _toc[i-1].zDatalen = _toc[i].Offset - _toc[i-1].Offset; */
            }
        }
        #endregion

        #region Binary Reader/Writer

        private BigEndianBinaryReader _reader;
        public void Read(Stream psarc, bool lazy = false)
        {
            _toc.Clear();
            _reader = new BigEndianBinaryReader(psarc);
            _header.MagicNumber = _reader.ReadUInt32();

            if (_header.MagicNumber == 1347633490U)//PSAR (BE)
            {
                //Parse Header
                _header.VersionNumber = _reader.ReadUInt32();
                _header.CompressionMethod = _reader.ReadUInt32();
                _header.TotalTOCSize = _reader.ReadUInt32();
                _header.TOCEntrySize = _reader.ReadUInt32();
                _header.NumFiles = _reader.ReadUInt32();
                _header.BlockSizeAlloc = _reader.ReadUInt32();
                _header.ArchiveFlags = _reader.ReadUInt32();
                //Read TOC
                int tocSize = (int)(_header.TotalTOCSize - 32U);

                if (_header.ArchiveFlags == 4)//TOC_ENCRYPTED
                {
                    // Decrypt TOC
                    var tocStream = new MemoryStream();
                    using (var decStream = new MemoryStream())
                    {
                        RijndaelEncryptor.DecryptPSARC(psarc, decStream, _header.TotalTOCSize);

                        int bytesRead;
                        int decSize = 0;
                        var buffer = new byte[_header.BlockSizeAlloc];
                        while ((bytesRead = decStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            decSize += bytesRead;
                            if (decSize > tocSize)
                                bytesRead = tocSize - (decSize - bytesRead);
                            tocStream.Write(buffer, 0, bytesRead);
                        }
                    }

                    tocStream.Position = 0;
                    _reader = new BigEndianBinaryReader(tocStream);
                }

                ParseTOC();
                //Parse zBlocksSizeList
                int tocChunkSize = (int)(_header.NumFiles * _header.TOCEntrySize);//(int)_reader.BaseStream.Position //don't alter this with. causes issues
                int zNum = (tocSize - tocChunkSize) / bNum;
                var zLengths = new uint[zNum];

                for (int i = 0; i < zNum; i++)
                {
                    switch (bNum)
                    {
                        case 2://64KB
                            zLengths[i] = _reader.ReadUInt16();
                            break;
                        case 3://16MB
                            zLengths[i] = _reader.ReadUInt24();
                            break;
                        case 4://4GB
                            zLengths[i] = _reader.ReadUInt32();
                            break;
                    }
                }

                _zBlocksSizeList = zLengths;//TODO: validate
                _reader.BaseStream.Flush();//Free tocStream resources
                _reader = new BigEndianBinaryReader(psarc);

                // Validate psarc size
                // if (psarc.Length < RequiredPsarcSize())
                // throw new InvalidDataException("Truncated psarc.");
                // try to unpack corrupt CDLC for now

                switch (_header.CompressionMethod)
                {
                    case 2053925218: //zlib (BE)
                        ReadManifest();
                        psarc.Seek(_header.TotalTOCSize, SeekOrigin.Begin);
                        if (!lazy)
                        {// Decompress Data
                            InflateEntries();
                        }
                        break;
                    case 1819962721: //lzma (BE)
                        throw new NotImplementedException("LZMA compression not supported.");
                    default:
                        throw new InvalidDataException("Unknown compression.");
                }
            }

            psarc.Flush();
        }

        #endregion

        #region TempFileStream Methods

        public class TempFileStream : FileStream
        {
            private const int _buffer_size = 65536;

            public TempFileStream()
                : base(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read, _buffer_size, FileOptions.DeleteOnClose)
            {
            }

            public TempFileStream(FileMode mode) // for Appending can not use FileAccess.ReadWrite
                : base(Path.GetTempFileName(), mode, FileAccess.Write, FileShare.Read, _buffer_size, FileOptions.DeleteOnClose)
            {
            }

            public TempFileStream(FileAccess access)
                : base(Path.GetTempFileName(), FileMode.Create, access, FileShare.Read, _buffer_size, FileOptions.DeleteOnClose)
            {
            }

            public TempFileStream(FileAccess access, FileShare share)
                : base(Path.GetTempFileName(), FileMode.Create, access, share, _buffer_size, FileOptions.DeleteOnClose)
            {
            }

            public TempFileStream(FileAccess access, FileShare share, int bufferSize)
                : base(Path.GetTempFileName(), FileMode.Create, access, share, bufferSize, FileOptions.DeleteOnClose)
            {
            }

            public TempFileStream(string path, FileMode mode)
                : base(path, mode)
            {
            }
        }
        #endregion

        /// MemoryStreamExtension is a re-implementation of MemoryStream that uses a dynamic list of byte arrays as a backing store,
        /// instead of a single byte array, the allocation of which will fail for relatively small streams as it requires contiguous memory.
        /// </summary>
        public class MemoryStreamExtension : Stream /* http://msdn.microsoft.com/en-us/library/system.io.stream.aspx */
        {
            #region Constructors

            public MemoryStreamExtension()
            {
                Position = 0;
            }

            public MemoryStreamExtension(byte[] source)
            {
                this.Write(source, 0, source.Length);
                Position = 0;
            }

            /* length is ignored because capacity has no meaning unless we implement an artifical limit */

            public MemoryStreamExtension(int length)
            {
                SetLength(length);
                Position = length;
                byte[] d = block; //access block to prompt the allocation of memory
                Position = 0;
            }

            #endregion

            #region Status Properties

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return true; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            #endregion

            #region Public Properties

            public override long Length
            {
                get { return length; }
            }

            public override long Position { get; set; }

            #endregion

            #region Members

            protected long length = 0;

            protected long blockSize = 65536;

            protected List<byte[]> blocks = new List<byte[]>();

            #endregion

            #region Internal Properties

            /* Use these properties to gain access to the appropriate block of memory for the current Position */

            /// <summary>
            /// The block of memory currently addressed by Position
            /// </summary>
            protected byte[] block
            {
                get
                {
                    while (blocks.Count <= blockId)
                        blocks.Add(new byte[blockSize]);
                    return blocks[(int)blockId];
                }
            }

            /// <summary>
            /// The id of the block currently addressed by Position
            /// </summary>
            protected long blockId
            {
                get { return Position / blockSize; }
            }

            /// <summary>
            /// The offset of the byte currently addressed by Position, into the block that contains it
            /// </summary>
            protected long blockOffset
            {
                get { return Position % blockSize; }
            }

            #endregion

            #region Public Stream Methods

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                long lcount = (long)count;

                if (lcount < 0)
                {
                    throw new ArgumentOutOfRangeException("count", lcount, "Number of bytes to copy cannot be negative.");
                }

                long remaining = (length - Position);
                if (lcount > remaining)
                    lcount = remaining;

                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer", "Buffer cannot be null.");
                }
                if (offset < 0)
                {
                    throw new ArgumentOutOfRangeException("offset", offset, "Destination offset cannot be negative.");
                }

                int read = 0;
                long copysize = 0;
                do
                {
                    copysize = Math.Min(lcount, (blockSize - blockOffset));
                    Buffer.BlockCopy(block, (int)blockOffset, buffer, offset, (int)copysize);
                    lcount -= copysize;
                    offset += (int)copysize;

                    read += (int)copysize;
                    Position += copysize;
                } while (lcount > 0);

                return read;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        Position = offset;
                        break;
                    case SeekOrigin.Current:
                        Position += offset;
                        break;
                    case SeekOrigin.End:
                        Position = Length - offset;
                        break;
                }
                return Position;
            }

            public override void SetLength(long value)
            {
                length = value;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                long initialPosition = Position;
                int copysize;
                try
                {
                    do
                    {
                        copysize = Math.Min(count, (int)(blockSize - blockOffset));

                        EnsureCapacity(Position + copysize);

                        Buffer.BlockCopy(buffer, (int)offset, block, (int)blockOffset, copysize);
                        count -= copysize;
                        offset += copysize;

                        Position += copysize;
                    } while (count > 0);
                }
                catch (Exception)
                {
                    Position = initialPosition;
                    throw;
                }
            }

            public override int ReadByte()
            {
                if (Position >= length)
                    return -1;

                byte b = block[blockOffset];
                Position++;

                return b;
            }

            public override void WriteByte(byte value)
            {
                EnsureCapacity(Position + 1);
                block[blockOffset] = value;
                Position++;
            }

            protected void EnsureCapacity(long intended_length)
            {
                if (intended_length > length)
                    length = (intended_length);
            }

            #endregion

            #region IDispose

            /* http://msdn.microsoft.com/en-us/library/fs2xkftw.aspx */

            protected override void Dispose(bool disposing)
            {
                /* We do not currently use unmanaged resources */
                base.Dispose(disposing);
            }

            #endregion

            #region Public Additional Helper Methods

            /// <summary>
            /// Returns the entire content of the stream as a byte array. This is not safe because the call to new byte[] may 
            /// fail if the stream is large enough. Where possible use methods which operate on streams directly instead.
            /// </summary>
            /// <returns>A byte[] containing the current data in the stream</returns>
            public byte[] ToArray()
            {
                long firstposition = Position;
                Position = 0;
                byte[] destination = new byte[Length];
                Read(destination, 0, (int)Length);
                Position = firstposition;
                return destination;
            }

            /// <summary>
            /// Reads length bytes from source into the this instance at the current position.
            /// </summary>
            /// <param name="source">The stream containing the data to copy</param>
            /// <param name="length">The number of bytes to copy</param>
            public void ReadFrom(Stream source, long length)
            {
                byte[] buffer = new byte[4096];
                int read;
                do
                {
                    read = source.Read(buffer, 0, (int)Math.Min(4096, length));
                    length -= read;
                    this.Write(buffer, 0, read);
                } while (length > 0);
            }

            /// <summary>
            /// Writes the entire stream into destination, regardless of Position, which remains unchanged.
            /// </summary>
            /// <param name="destination">The stream to write the content of this stream to</param>
            public void WriteTo(Stream destination)
            {
                long initialpos = Position;
                Position = 0;
                this.CopyTo(destination);
                Position = initialpos;
            }

            #endregion
        }

    }
}

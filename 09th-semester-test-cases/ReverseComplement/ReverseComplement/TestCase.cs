// The Computer Language Benchmarks Game
// https://benchmarksgame-team.pages.debian.net/benchmarksgame/

// ported from F# version with improvements by Anthony Lloyd

using System;
using System.IO;
using System.Buffers;
using System.Threading;
using System.Collections.Concurrent;

namespace ReverseComplement
{
    class RevCompSequence { public List<byte[]> Pages; public int StartHeader, EndExclusive; public Thread ReverseThread; }
    public static class TestCase
    {
        public static int Run(DateTime stopTime)
        {
            new Thread(Reader).Start();
            new Thread(Grouper).Start();
            return Writer(stopTime);
        }

        const int READER_BUFFER_SIZE = 1024 * 1024;
        const byte LF = 10, GT = (byte)'>', SP = 32;
        static BlockingCollection<byte[]> readQue = new BlockingCollection<byte[]>();
        static BlockingCollection<RevCompSequence> writeQue = new BlockingCollection<RevCompSequence>();
        static byte[] map;

        static int read(Stream stream, byte[] buffer, int offset, int count)
        {
            var bytesRead = stream.Read(buffer, offset, count);
            return bytesRead == count ? offset + count
                 : bytesRead == 0 ? offset
                 : read(stream, buffer, offset + bytesRead, count - bytesRead);
        }
        static void Reader()
        {
            using (var stream = Console.OpenStandardInput())
            {
                int bytesRead;
                do
                {
                    var buffer = new byte[READER_BUFFER_SIZE];
                    bytesRead = read(stream, buffer, 0, READER_BUFFER_SIZE);
                    readQue.Add(buffer);
                } while (bytesRead == READER_BUFFER_SIZE);
                readQue.CompleteAdding();
            }
        }

        static bool tryTake<T>(BlockingCollection<T> q, out T t) where T : class
        {
            t = null;
            while (!q.IsCompleted && !q.TryTake(out t)) Thread.SpinWait(0);
            return t != null;
        }

        static void Grouper()
        {
            // Set up complements map
            map = new byte[256];
            for (byte b = 0; b < 255; b++) map[b] = b;
            map[(byte)'A'] = (byte)'T';
            map[(byte)'B'] = (byte)'V';
            map[(byte)'C'] = (byte)'G';
            map[(byte)'D'] = (byte)'H';
            map[(byte)'G'] = (byte)'C';
            map[(byte)'H'] = (byte)'D';
            map[(byte)'K'] = (byte)'M';
            map[(byte)'M'] = (byte)'K';
            map[(byte)'R'] = (byte)'Y';
            map[(byte)'T'] = (byte)'A';
            map[(byte)'V'] = (byte)'B';
            map[(byte)'Y'] = (byte)'R';
            map[(byte)'a'] = (byte)'T';
            map[(byte)'b'] = (byte)'V';
            map[(byte)'c'] = (byte)'G';
            map[(byte)'d'] = (byte)'H';
            map[(byte)'g'] = (byte)'C';
            map[(byte)'h'] = (byte)'D';
            map[(byte)'k'] = (byte)'M';
            map[(byte)'m'] = (byte)'K';
            map[(byte)'r'] = (byte)'Y';
            map[(byte)'t'] = (byte)'A';
            map[(byte)'v'] = (byte)'B';
            map[(byte)'y'] = (byte)'R';

            var startHeader = 0;
            var i = 0;
            bool afterFirst = false;
            var data = new List<byte[]>();
            byte[] bytes;
            while (tryTake(readQue, out bytes))
            {
                data.Add(bytes);
                while ((i = Array.IndexOf<byte>(bytes, GT, i + 1)) != -1)
                {
                    var sequence = new RevCompSequence
                    {
                        Pages = data
                        ,
                        StartHeader = startHeader,
                        EndExclusive = i
                    };
                    if (afterFirst)
                        (sequence.ReverseThread = new Thread(() => Reverse(sequence))).Start();
                    else
                        afterFirst = true;
                    writeQue.Add(sequence);
                    startHeader = i;
                    data = new List<byte[]> { bytes };
                }
            }
            i = Array.IndexOf<byte>(data[data.Count - 1], 0, 0);
            var lastSequence = new RevCompSequence
            {
                Pages = data
                ,
                StartHeader = startHeader,
                EndExclusive = i == -1 ? data[data.Count - 1].Length : i
            };
            Reverse(lastSequence);
            writeQue.Add(lastSequence);
            writeQue.CompleteAdding();
        }

        static void Reverse(RevCompSequence sequence)
        {
            var startPageId = 0;
            var startBytes = sequence.Pages[0];
            var startIndex = sequence.StartHeader;

            // Skip header line
            while ((startIndex = Array.IndexOf<byte>(startBytes, LF, startIndex)) == -1)
            {
                startBytes = sequence.Pages[++startPageId];
                startIndex = 0;
            }

            var endPageId = sequence.Pages.Count - 1;
            var endIndex = sequence.EndExclusive - 1;
            if (endIndex == -1) endIndex = sequence.Pages[--endPageId].Length - 1;
            var endBytes = sequence.Pages[endPageId];

            // Swap in place across pages
            do
            {
                var startByte = startBytes[startIndex];
                if (startByte < SP)
                {
                    if (++startIndex == startBytes.Length)
                    {
                        startBytes = sequence.Pages[++startPageId];
                        startIndex = 0;
                    }
                    if (startIndex == endIndex && startPageId == endPageId) break;
                    startByte = startBytes[startIndex];
                }
                var endByte = endBytes[endIndex];
                if (endByte < SP)
                {
                    if (--endIndex == -1)
                    {
                        endBytes = sequence.Pages[--endPageId];
                        endIndex = endBytes.Length - 1;
                    }
                    if (startIndex == endIndex && startPageId == endPageId) break;
                    endByte = endBytes[endIndex];
                }

                startBytes[startIndex] = map[endByte];
                endBytes[endIndex] = map[startByte];

                if (++startIndex == startBytes.Length)
                {
                    startBytes = sequence.Pages[++startPageId];
                    startIndex = 0;
                }
                if (--endIndex == -1)
                {
                    endBytes = sequence.Pages[--endPageId];
                    endIndex = endBytes.Length - 1;
                }
            } while (startPageId < endPageId || (startPageId == endPageId && startIndex < endIndex));
            if (startIndex == endIndex) startBytes[startIndex] = map[startBytes[startIndex]];
        }

        static int Writer(DateTime stopTime)
        {
            var counter = 0;
            using (var stream = Console.OpenStandardOutput())
            {
                bool first = true;
                RevCompSequence sequence;
                while (tryTake(writeQue, out sequence) && stopTime > DateTime.UtcNow)
                {
                    counter += 1;
                    var startIndex = sequence.StartHeader;
                    var pages = sequence.Pages;
                    if (first)
                    {
                        Reverse(sequence);
                        first = false;
                    }
                    else
                    {
                        sequence.ReverseThread?.Join();
                    }
                    for (int i = 0; i < pages.Count - 1; i++)
                    {
                        var bytes = pages[i];
                        stream.Write(bytes, startIndex, bytes.Length - startIndex);
                        startIndex = 0;
                    }
                    stream.Write(pages[pages.Count - 1], startIndex, sequence.EndExclusive - startIndex);
                }
            }
            return counter;



            //const int PAGE_SIZE = 1024 * 1024;
            //const byte LF = (byte)'\n', GT = (byte)'>';
            //static volatile int readCount = 0, lastPageSize = PAGE_SIZE, canWriteCount = 0;
            //static byte[][] pages = new byte[1024][];

            //public static int Run(DateTime stopTime)
            //{
            //    new Thread(() =>
            //    {
            //        static int Read(Stream stream, byte[] bytes, int offset)
            //        {
            //            var bytesRead = stream.Read(bytes, offset, PAGE_SIZE - offset);
            //            return bytesRead + offset == PAGE_SIZE ? PAGE_SIZE
            //                    : bytesRead == 0 ? offset
            //                    : Read(stream, bytes, offset + bytesRead);
            //        }
            //        using var inStream = Console.OpenStandardInput();
            //        do
            //        {
            //            var page = ArrayPool<byte>.Shared.Rent(PAGE_SIZE);
            //            lastPageSize = Read(inStream, page, 0);
            //            pages[readCount] = page;
            //            readCount++;
            //        } while (lastPageSize == PAGE_SIZE);
            //    }).Start();

            //    new Thread(() =>
            //    {
            //        static void Reverse(object o)
            //        {
            //            Span<byte> map = stackalloc byte[256];
            //            for (int b = 0; b < map.Length; b++) map[b] = (byte)b;
            //            map['A'] = map['a'] = (byte)'T';
            //            map['B'] = map['b'] = (byte)'V';
            //            map['C'] = map['c'] = (byte)'G';
            //            map['D'] = map['d'] = (byte)'H';
            //            map['G'] = map['g'] = (byte)'C';
            //            map['H'] = map['h'] = (byte)'D';
            //            map['K'] = map['k'] = (byte)'M';
            //            map['M'] = map['m'] = (byte)'K';
            //            map['R'] = map['r'] = (byte)'Y';
            //            map['T'] = map['t'] = (byte)'A';
            //            map['V'] = map['v'] = (byte)'B';
            //            map['Y'] = map['y'] = (byte)'R';
            //            var (loPageID, lo, lastPageID, hi, previous) =
            //                ((int, int, int, int, Thread))o;
            //            var hiPageID = lastPageID;
            //            if (lo == PAGE_SIZE) { lo = 0; loPageID++; }
            //            if (hi == -1) { hi = PAGE_SIZE - 1; hiPageID--; }
            //            var loPage = pages[loPageID];
            //            var hiPage = pages[hiPageID];
            //            do
            //            {
            //                ref var loValue = ref loPage[lo++];
            //                ref var hiValue = ref hiPage[hi--];
            //                if (loValue == LF)
            //                {
            //                    if (hiValue != LF) hi++;
            //                }
            //                else if (hiValue == LF)
            //                {
            //                    lo--;
            //                }
            //                else
            //                {
            //                    var swap = map[loValue];
            //                    loValue = map[hiValue];
            //                    hiValue = swap;
            //                }
            //                if (lo == PAGE_SIZE)
            //                {
            //                    lo = 0;
            //                    loPage = pages[++loPageID];
            //                    if (previous == null || !previous.IsAlive)
            //                        canWriteCount = loPageID;
            //                }
            //                if (hi == -1)
            //                {
            //                    hi = PAGE_SIZE - 1;
            //                    hiPage = pages[--hiPageID];
            //                }
            //            } while (loPageID < hiPageID
            //                    || (loPageID == hiPageID && lo <= hi));
            //            previous?.Join();
            //            canWriteCount = lastPageID;
            //        }

            //        int pageID = 0, index = 0; Thread previous = null;
            //        while (true)
            //        {
            //            while (true) // skip header
            //            {
            //                while (pageID == readCount) Thread.Sleep(0);
            //                index = Array.IndexOf(pages[pageID], LF, index);
            //                if (index != -1) break;
            //                index = 0;
            //                pageID++;
            //            }
            //            var loPageID = pageID;
            //            var lo = ++index;
            //            while (true)
            //            {
            //                while (pageID == readCount) Thread.Sleep(0);
            //                var isLastPage = pageID + 1 == readCount
            //                              && lastPageSize != PAGE_SIZE;
            //                index = Array.IndexOf(pages[pageID], GT, index,
            //                    (isLastPage ? lastPageSize : PAGE_SIZE) - index);
            //                if (index != -1)
            //                {
            //                    object o = (loPageID, lo, pageID, index - 1, previous);
            //                    (previous = new Thread(Reverse)).Start(o);
            //                    break;
            //                }
            //                else if (isLastPage)
            //                {
            //                    Reverse((loPageID, lo, pageID, lastPageSize - 1, previous));
            //                    canWriteCount = readCount;
            //                    return;
            //                }
            //                pageID++;
            //                index = 0;
            //            }
            //        }
            //    }).Start();

            //    using var outStream = Console.OpenStandardOutput();
            //    int writtenCount = 0;
            //    var counter = 0;
            //    while (stopTime > DateTime.UtcNow)
            //    {
            //        counter += 1;
            //        while (writtenCount == canWriteCount) Thread.Sleep(0);
            //        var page = pages[writtenCount++];
            //        if (writtenCount == readCount && lastPageSize != PAGE_SIZE)
            //        {
            //            outStream.Write(page, 0, lastPageSize);
            //            return counter;
            //        }
            //        outStream.Write(page, 0, PAGE_SIZE);
            //        ArrayPool<byte>.Shared.Return(page);
            //    }

            //    return counter;
            //}
        }
    }
}

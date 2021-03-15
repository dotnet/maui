using System;
using System.Text;
using Java.IO;

namespace Microsoft.Maui.Graphics.Android
{
    // The class which loads the TTF file, parses it and returns the TTF font name
    public class FontAnalyzer
    {
        // Font file; must be seekable
        private RandomAccessFile _mFile;

        // This function parses the TTF file and returns the font name specified in the file
        public FontInfo GetFontInfo(string aFileName)
        {
            try
            {
                // Parses the TTF file format.
                // See http://developer.apple.com/fonts/ttrefman/rm06/Chap6.html
                _mFile = new RandomAccessFile(aFileName, "r");

                // Read the version first
                int version = ReadDWord();

                // The version must be either 'true' (0x74727565) or 0x00010000 or 'OTTO' (0x4f54544f) for CFF style fonts.
                if (version != 0x74727565 && version != 0x00010000 && version != 0x4f54544f)
                {
                    return null;
                }

                // The TTF file consist of several sections called "tables", and we need to know how many of them are there.
                int numTables = ReadWord();

                // Skip the rest in the header
                ReadWord(); // skip searchRange
                ReadWord(); // skip entrySelector
                ReadWord(); // skip rangeShift

                // Now we can read the tables
                for (int i = 0; i < numTables; i++)
                {
                    // Read the table entry
                    int tag = ReadDWord();
                    ReadDWord(); // skip checksum
                    int offset = ReadDWord();
                    int length = ReadDWord();

                    // Now here' the trick. 'name' field actually contains the textual string name.
                    // So the 'name' string in characters equals to 0x6E616D65
                    if (tag == 0x6E616D65)
                    {
                        // Here's the name section. Read it completely into the allocated buffer
                        var table = new byte[length];

                        _mFile.Seek(offset);
                        Read(table);

                        // This is also a table. See http://developer.apple.com/fonts/ttrefman/rm06/Chap6name.html
                        // According to Table 36, the total number of table records is stored in the second word, at the offset 2.
                        // Getting the count and string offset - remembering it's big endian.
                        int count = GetWord(table, 2);
                        int stringOffset = GetWord(table, 4);

                        string familyName = null;
                        string styleName = null;
                        string fullName = null;

                        // Record starts from offset 6
                        for (int record = 0; record < count; record++)
                        {
                            // Table 37 tells us that each record is 6 words -> 12 bytes, and that the nameID is 4th word so its offset is 6.
                            // We also need to account for the first 6 bytes of the header above (Table 36), so...
                            int nameidOffset = record * 12 + 6;
                            int platformId = GetWord(table, nameidOffset);
                            int nameidValue = GetWord(table, nameidOffset + 6);

                            if (nameidValue == 1 && platformId == 1)
                                familyName = ReadTableEntry(table, nameidOffset, stringOffset);

                            if (nameidValue == 2 && platformId == 1)
                                styleName = ReadTableEntry(table, nameidOffset, stringOffset);

                            if (nameidValue == 4 && platformId == 1)
                                fullName = ReadTableEntry(table, nameidOffset, stringOffset);

                            if (familyName != null && styleName != null && fullName != null)
                                return new FontInfo(aFileName, familyName, styleName, fullName);
                        }
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Info(e);
                // Most likely a corrupted font file
                return null;
            }
        }

        private string ReadTableEntry(byte[] table, int nameidOffset, int stringOffset)
        {
            // We need the string offset and length, which are the word 6 and 5 respectively
            int nameLength = GetWord(table, nameidOffset + 8);
            int nameOffset = GetWord(table, nameidOffset + 10);

            // The real name string offset is calculated by adding the string_offset
            nameOffset = nameOffset + stringOffset;

            // Make sure it is inside the array
            if (nameOffset >= 0 && nameOffset + nameLength < table.Length)
                return Encoding.UTF8.GetString(table, nameOffset, nameLength);

            return null;
        }

        // Helper I/O functions
        private int ReadByte()
        {
            return _mFile.Read() & 0xFF;
        }

        private int ReadWord()
        {
            int b1 = ReadByte();
            int b2 = ReadByte();

            return b1 << 8 | b2;
        }

        private int ReadDWord()
        {
            int b1 = ReadByte();
            int b2 = ReadByte();
            int b3 = ReadByte();
            int b4 = ReadByte();

            return b1 << 24 | b2 << 16 | b3 << 8 | b4;
        }

        private void Read(byte[] array)
        {
            if (_mFile.Read(array) != array.Length)
                throw new Exception();
        }

        // Helper
        private int GetWord(byte[] array, int offset)
        {
            int b1 = array[offset] & 0xFF;
            int b2 = array[offset + 1] & 0xFF;

            return b1 << 8 | b2;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// 读取.nes格式的文件
public class Cartridge
{
    public class ByteReader
    {
        private byte[] bytes;
        private int pos;

        public ByteReader(byte[] bytes)
        {
            this.bytes = bytes;
            pos = 0;
        }

        public byte Read()
        {
            return bytes[pos++];
        }

        public byte[] Read(int count)
        {
            byte[] result = new byte[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = Read();
            }
            return result;
        }
    }

    public class RomHeader
    {
        public byte PRGNum; // x 16K
        public byte CHRNum; // x 8K
        public byte Flag0;
        public byte Flag1;
        public byte[] Unknown;

        public bool Load(ByteReader reader)
        {
            if (reader.Read() != 0x4E || reader.Read() != 0x45 || reader.Read() != 0x53 || reader.Read() != 0x1A)
            {
                return false;
            }
            PRGNum = reader.Read();
            CHRNum = reader.Read();
            Flag0 = reader.Read();
            Flag1 = reader.Read();
            Unknown = reader.Read(8);
            return true;
        }

        public byte MapperNumber
        {
            get
            {
                int low = (Flag0 & 0xF0) >> 4;
                int high = Flag1 & 0xF0;
                return (byte)(low | high);
            }
        }
    }

    public RomHeader header;
    public byte[] Trainer;
    public IMapper mapper;

    public Cartridge(byte[] bytes)
    {
        var reader = new ByteReader(bytes);
        header = new RomHeader();
        if (!header.Load(reader))
        {
            throw new NesException("File format error");
        }
        if ((header.Flag0 & (1 << 2)) != 0)
        {
            Trainer = reader.Read(512);
        }
        var PRG = reader.Read(header.PRGNum * 16 * 1024);
        var CHR = reader.Read(header.CHRNum * 8 * 1024);
        if (header.MapperNumber == 0)
        {
            mapper = new Mapper0(PRG, CHR);
        }
        else
        {
            throw new NesException(string.Format("Unsupported mapper: {0}", header.MapperNumber));
        }
    }
}

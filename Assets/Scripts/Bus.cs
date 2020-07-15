using UnityEngine;
using System.Collections;

public class Bus
{
    private IMapper mapper;
    private Memory memory;

    public Bus(IMapper mapper, Memory memory)
    {
        this.mapper = mapper;
        this.memory = memory;
    }

    public byte Read8(ushort address)
    {
        // CHR 0x0000 - 0x2000
        // PRG 0x8000 - 0xFFFF
        if (address < 0x2000)
        {
            return mapper.Read(address);
        }
        if (address >= 0x8000)
        {
            return mapper.Read(address);
        }
        throw new NesException("Bus.Read8 unknown address");
    }

    public void Write8(ushort address, byte value)
    {
        if (address < 0x2000)
        {
            mapper.Write(address, value);
        }
        if (address >= 0x8000)
        {
            mapper.Write(address, value);
        }
        throw new NesException("Bus.Write8 unknown address");
    }

    public ushort Read16(ushort address)
    {
        return (ushort)(Read8(address) | (Read8((ushort)(address + 1)) << 8));
    }

    public void Write16(ushort address, ushort value)
    {
        Write8(address, (byte)(value & 0xFF));
        Write8((ushort)(address + 1), (byte)(value >> 8));
    }
}

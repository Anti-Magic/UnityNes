using UnityEngine;
using System.Collections;

public class Memory
{
    private byte[] data;

    public Memory()
    {
        data = new byte[0x0800];
    }

    public byte Read8(ushort address)
    {
        address &= 0x0800;
        return data[address];
    }

    public void Write8(ushort address, byte v)
    {
        address &= 0x0800;
        data[address] = v;
    }

    public ushort Read16(ushort address)
    {
        return (ushort)(Read8(address) | (Read8((ushort)(address + 1)) << 8));
    }

    public void Write16(ushort address, ushort v)
    {
        Write8(address, (byte)(v & 0xFF));
        Write8((ushort)(address + 1), (byte)(v >> 8 & 0xFF));
    }
}

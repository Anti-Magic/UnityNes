using UnityEngine;
using System.Collections;

public class Memory
{
    private byte[] buffer;

    public Memory(int size)
    {
        buffer = new byte[size];
    }

    public byte Read8(ushort address)
    {
        throw new NesException("Memory");
    }

    public void Write8(ushort address, byte v)
    {
        throw new NesException("Memory");
    }

    public ushort Read16(ushort address)
    {
        throw new NesException("Memory");
    }

    public void Write16(ushort address, ushort v)
    {
        throw new NesException("Memory");
    }
}

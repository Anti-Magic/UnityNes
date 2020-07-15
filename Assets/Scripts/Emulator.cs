using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Emulator
{
    private ulong cpuCycles = 0;
    private ulong time2Cycles = 0;

    private Cartridge cart;
    private Memory memory;
    private Bus bus;
    private CPU cpu;

    public Emulator(byte[] nesFileData)
    {
        cart = new Cartridge(nesFileData);
        bus = new Bus(cart.mapper, memory);
        cpu = new CPU(bus);
    }

    public void Tick(float dt)
    {
        cpu.Tick();
    }
}

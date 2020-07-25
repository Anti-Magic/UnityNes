using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://zhuanlan.zhihu.com/p/44088842
// https://www.jianshu.com/nb/44676155
public class Emulator
{
    private ulong cpuCycles = 0;
    private ulong time2Cycles = 0;

    private Cartridge cart;
    private Memory memory;
    private Bus bus;
    private CPU cpu;
    private Debugger debugger;

    public Emulator(byte[] nesFileData)
    {
        cart = new Cartridge(nesFileData);
        memory = new Memory();
        bus = new Bus(cart.mapper, memory);
        cpu = new CPU(bus);
        debugger = new Debugger(bus, cpu);
    }

    public void Tick(float dt)
    {
        debugger.BeginTickCPU();
        cpu.Tick();
        debugger.EndTickCPU();
        debugger.Print();
    }
}

using UnityEngine;
using UnityEditor;

public class CPU
{
    // https://zhuanlan.zhihu.com/p/44088842
    public enum AddrMode
    {
        Imp, // Implicit 特殊指令的寻址方式
        Acc, // Accumulator 累加器寻址
        Imm, // Immediate 立即寻址
        Zpg, // Zero-page Absolute 绝对零页寻址
        Zpx, // Zero-page X Indexed 零页X变址
        Zpy, // Zero-page Y Indexed 零页Y变址
        Izx, // Pre-indexed Indirect 间接X变址(先变址X后间接寻址)
        Izy, // Post-indexed Indirect 间接Y变址(后变址Y间接寻址)
        Abs, // Absolute 绝对寻址
        Abx, // Absolute X Indexed 绝对X变址
        Aby, // Absolute Y Indexed 绝对Y变址
        Ind, // Indirect 间接寻址
        Rel, // Relative 相对寻址
    }

    public class Opcode
    {
        public string name;
        public AddrMode addrMode;
        public int cycles;

        public Opcode(string name, AddrMode addrMode, int cycles)
        {
            this.name = name;
            this.addrMode = addrMode;
            this.cycles = cycles;
        }
    }

    // http://www.oxyron.de/html/opcodes02.html
    private static Opcode[] Opcodes = new Opcode[]
    {
        new Opcode("BRK", AddrMode.Imp, 7), new Opcode("ORA", AddrMode.Izx, 6), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0),
        new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0),
        new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0),
        new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0),
        new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0),
        new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0),
        new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0),
        new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0), new Opcode("KIL", AddrMode.Imp, 0),
    };

    private ushort regPC;
    private byte regA;
    private byte regX;
    private byte regY;
    private byte regSP;
    private byte regP;

    private Bus bus;

    public CPU(Bus bus)
    {
        this.bus = bus;
        Reset();
    }

    public void Tick()
    {
        byte opcode = bus.Read8(regPC++);
        switch (opcode)
        {
            default:
                throw new NesException(string.Format("Unknown opcode: {0}", opcode));
        }
    }

    public void Reset()
    {
        regA = 0;
        regX = 0;
        regY = 0;
        regP = 0;
        regSP = 0xFD;
        //regPC = bus.Read16(0xFFFC);
        regPC = 0xC000;
    }

    public void NMI()
    {

    }

    public void IRQ()
    {
        if ((regP & (1 << 2)) != 0)
        {
            return;
        }
        PushStack16(regPC);
        //PushStack8(regP | )

    }

    private void PushStack8(byte value)
    {
        bus.Write8((ushort)(0x100 | regSP), value);
        regSP -= 1;
    }

    private byte PopStack8()
    {
        regSP += 1;
        return bus.Read8((ushort)(0x100 | regSP));
    }

    private void PushStack16(ushort value)
    {
        PushStack8((byte)(value >> 8));
        PushStack8((byte)(value & 0xFF));
    }

    private ushort PopStack16()
    {
        return (ushort)(PopStack8() | (PopStack8() << 8));
    }
}

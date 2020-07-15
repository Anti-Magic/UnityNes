using UnityEngine;
using UnityEditor;

public class CPU
{
    // instructions http://obelisk.me.uk/6502/reference.html
    public enum Ins
    {
        ADC,
        AHX,
        ALR,
        ANC,
        AND,
        ARR,
        ASL,
        AXS,
        BCC,
        BCS,
        BEQ,
        BIT,
        BMI,
        BNE,
        BPL,
        BRK,
        BVC,
        BVS,
        CLC,
        CLD,
        CLI,
        CLV,
        CMP,
        CPX,
        CPY,
        DCP,
        DEC,
        DEX,
        DEY,
        EOR,
        INC,
        INX,
        INY,
        ISC,
        JMP,
        JSR,
        KIL,
        LAS,
        LAX,
        LDA,
        LDX,
        LDY,
        LSR,
        NOP,
        ORA,
        PHA,
        PHP,
        PLA,
        PLP,
        RLA,
        ROL,
        ROR,
        RRA,
        RTI,
        RTS,
        SAX,
        SBC,
        SEC,
        SED,
        SEI,
        SHX,
        SHY,
        SLO,
        SRE,
        STA,
        STX,
        STY,
        TAS,
        TAX,
        TAY,
        TSX,
        TXA,
        TXS,
        TYA,
        XAA,
    }

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
        public Ins instruction;
        public AddrMode addrMode;
        public int cycles;

        public Opcode(Ins instruction, AddrMode addrMode, int cycles)
        {
            this.instruction = instruction;
            this.addrMode = addrMode;
            this.cycles = cycles;
        }
    }

    // http://www.oxyron.de/html/opcodes02.html
    private static Opcode[] Opcodes = new Opcode[]
    {
        new Opcode(Ins.BRK, AddrMode.Imp, 7), new Opcode(Ins.ORA, AddrMode.Izx, 6), new Opcode(Ins.KIL, AddrMode.Imp, 0), new Opcode(Ins.SLO, AddrMode.Izx, 8), new Opcode(Ins.NOP, AddrMode.Zpg, 3), new Opcode(Ins.ORA, AddrMode.Zpg, 3), new Opcode(Ins.ASL, AddrMode.Zpg, 5), new Opcode(Ins.SLO, AddrMode.Zpg, 5), new Opcode(Ins.PHP, AddrMode.Imp, 3), new Opcode(Ins.ORA, AddrMode.Imm, 2), new Opcode(Ins.ASL, AddrMode.Imp, 2), new Opcode(Ins.ANC, AddrMode.Imm, 2), new Opcode(Ins.NOP, AddrMode.Abs, 4), new Opcode(Ins.ORA, AddrMode.Abs, 4), new Opcode(Ins.ASL, AddrMode.Abs, 6), new Opcode(Ins.SLO, AddrMode.Abs, 6),
        new Opcode(Ins.BPL, AddrMode.Rel, 2), new Opcode(Ins.ORA, AddrMode.Izy, 5), new Opcode(Ins.KIL, AddrMode.Imp, 0), new Opcode(Ins.SLO, AddrMode.Izy, 8), new Opcode(Ins.NOP, AddrMode.Zpx, 4), new Opcode(Ins.ORA, AddrMode.Zpx, 4), new Opcode(Ins.ASL, AddrMode.Zpx, 6), new Opcode(Ins.SLO, AddrMode.Zpx, 6), new Opcode(Ins.CLC, AddrMode.Imp, 2), new Opcode(Ins.ORA, AddrMode.Aby, 4), new Opcode(Ins.NOP, AddrMode.Imp, 2), new Opcode(Ins.SLO, AddrMode.Aby, 7), new Opcode(Ins.NOP, AddrMode.Abx, 4), new Opcode(Ins.ORA, AddrMode.Abx, 4), new Opcode(Ins.ASL, AddrMode.Abx, 7), new Opcode(Ins.SLO, AddrMode.Abx, 7),
        new Opcode(Ins.JSR, AddrMode.Abs, 6), new Opcode(Ins.AND, AddrMode.Izx, 6), new Opcode(Ins.KIL, AddrMode.Imp, 0), new Opcode(Ins.RLA, AddrMode.Izx, 8), new Opcode(Ins.BIT, AddrMode.Zpg, 3), new Opcode(Ins.AND, AddrMode.Zpg, 3), new Opcode(Ins.ROL, AddrMode.Zpg, 5), new Opcode(Ins.RLA, AddrMode.Zpg, 5), new Opcode(Ins.PLP, AddrMode.Imp, 4), new Opcode(Ins.AND, AddrMode.Imm, 2), new Opcode(Ins.ROL, AddrMode.Imp, 2), new Opcode(Ins.ANC, AddrMode.Imm, 2), new Opcode(Ins.BIT, AddrMode.Abs, 4), new Opcode(Ins.AND, AddrMode.Abs, 4), new Opcode(Ins.ROL, AddrMode.Abs, 6), new Opcode(Ins.RLA, AddrMode.Abs, 6),
        new Opcode(Ins.BMI, AddrMode.Rel, 2), new Opcode(Ins.AND, AddrMode.Izy, 5), new Opcode(Ins.KIL, AddrMode.Imp, 0), new Opcode(Ins.RLA, AddrMode.Izy, 8), new Opcode(Ins.NOP, AddrMode.Zpx, 4), new Opcode(Ins.AND, AddrMode.Zpx, 4), new Opcode(Ins.ROL, AddrMode.Zpx, 6), new Opcode(Ins.RLA, AddrMode.Zpx, 6), new Opcode(Ins.SEC, AddrMode.Imp, 2), new Opcode(Ins.AND, AddrMode.Aby, 4), new Opcode(Ins.NOP, AddrMode.Imp, 2), new Opcode(Ins.RLA, AddrMode.Aby, 7), new Opcode(Ins.NOP, AddrMode.Abx, 4), new Opcode(Ins.AND, AddrMode.Abx, 4), new Opcode(Ins.ROL, AddrMode.Abx, 7), new Opcode(Ins.RLA, AddrMode.Abx, 7),
        new Opcode(Ins.RTI, AddrMode.Imp, 6), new Opcode(Ins.EOR, AddrMode.Izx, 6), new Opcode(Ins.KIL, AddrMode.Imp, 0), new Opcode(Ins.SRE, AddrMode.Izx, 8), new Opcode(Ins.NOP, AddrMode.Zpg, 3), new Opcode(Ins.EOR, AddrMode.Zpg, 3), new Opcode(Ins.LSR, AddrMode.Zpg, 5), new Opcode(Ins.SRE, AddrMode.Zpg, 5), new Opcode(Ins.PHA, AddrMode.Imp, 3), new Opcode(Ins.EOR, AddrMode.Imm, 2), new Opcode(Ins.LSR, AddrMode.Imp, 2), new Opcode(Ins.ALR, AddrMode.Imm, 2), new Opcode(Ins.JMP, AddrMode.Abs, 3), new Opcode(Ins.EOR, AddrMode.Abs, 4), new Opcode(Ins.LSR, AddrMode.Abs, 6), new Opcode(Ins.SRE, AddrMode.Abs, 6),
        new Opcode(Ins.BVC, AddrMode.Rel, 2), new Opcode(Ins.EOR, AddrMode.Izy, 5), new Opcode(Ins.KIL, AddrMode.Imp, 0), new Opcode(Ins.SRE, AddrMode.Izy, 8), new Opcode(Ins.NOP, AddrMode.Zpx, 4), new Opcode(Ins.EOR, AddrMode.Zpx, 4), new Opcode(Ins.LSR, AddrMode.Zpx, 6), new Opcode(Ins.SRE, AddrMode.Zpx, 6), new Opcode(Ins.CLI, AddrMode.Imp, 2), new Opcode(Ins.EOR, AddrMode.Aby, 4), new Opcode(Ins.NOP, AddrMode.Imp, 2), new Opcode(Ins.SRE, AddrMode.Aby, 7), new Opcode(Ins.NOP, AddrMode.Abx, 4), new Opcode(Ins.EOR, AddrMode.Abx, 4), new Opcode(Ins.LSR, AddrMode.Abx, 7), new Opcode(Ins.SRE, AddrMode.Abx, 7),
        new Opcode(Ins.RTS, AddrMode.Imp, 6), new Opcode(Ins.ADC, AddrMode.Izx, 6), new Opcode(Ins.KIL, AddrMode.Imp, 0), new Opcode(Ins.RRA, AddrMode.Izx, 8), new Opcode(Ins.NOP, AddrMode.Zpg, 3), new Opcode(Ins.ADC, AddrMode.Zpg, 3), new Opcode(Ins.ROR, AddrMode.Zpg, 5), new Opcode(Ins.RRA, AddrMode.Zpg, 5), new Opcode(Ins.PLA, AddrMode.Imp, 4), new Opcode(Ins.ADC, AddrMode.Imm, 2), new Opcode(Ins.ROR, AddrMode.Imp, 2), new Opcode(Ins.ARR, AddrMode.Imm, 2), new Opcode(Ins.JMP, AddrMode.Ind, 5), new Opcode(Ins.ADC, AddrMode.Abs, 4), new Opcode(Ins.ROR, AddrMode.Abs, 6), new Opcode(Ins.RRA, AddrMode.Abs, 6),
        new Opcode(Ins.BVS, AddrMode.Rel, 2), new Opcode(Ins.ADC, AddrMode.Izy, 5), new Opcode(Ins.KIL, AddrMode.Imp, 0), new Opcode(Ins.RRA, AddrMode.Izy, 8), new Opcode(Ins.NOP, AddrMode.Zpx, 4), new Opcode(Ins.ADC, AddrMode.Zpx, 4), new Opcode(Ins.ROR, AddrMode.Zpx, 6), new Opcode(Ins.RRA, AddrMode.Zpx, 6), new Opcode(Ins.SEI, AddrMode.Imp, 2), new Opcode(Ins.ADC, AddrMode.Aby, 4), new Opcode(Ins.NOP, AddrMode.Imp, 2), new Opcode(Ins.RRA, AddrMode.Aby, 7), new Opcode(Ins.NOP, AddrMode.Abx, 4), new Opcode(Ins.ADC, AddrMode.Abx, 4), new Opcode(Ins.ROR, AddrMode.Abx, 7), new Opcode(Ins.RRA, AddrMode.Abx, 7),
        new Opcode(Ins.NOP, AddrMode.Imm, 2), new Opcode(Ins.STA, AddrMode.Izx, 6), new Opcode(Ins.NOP, AddrMode.Imm, 2), new Opcode(Ins.SAX, AddrMode.Izx, 6), new Opcode(Ins.STY, AddrMode.Zpg, 3), new Opcode(Ins.STA, AddrMode.Zpg, 3), new Opcode(Ins.STX, AddrMode.Zpg, 3), new Opcode(Ins.SAX, AddrMode.Zpg, 3), new Opcode(Ins.DEY, AddrMode.Imp, 2), new Opcode(Ins.NOP, AddrMode.Imm, 2), new Opcode(Ins.TXA, AddrMode.Imp, 2), new Opcode(Ins.XAA, AddrMode.Imm, 2), new Opcode(Ins.STY, AddrMode.Abs, 4), new Opcode(Ins.STA, AddrMode.Abs, 4), new Opcode(Ins.STX, AddrMode.Abs, 4), new Opcode(Ins.SAX, AddrMode.Abs, 4),
        new Opcode(Ins.BCC, AddrMode.Rel, 2), new Opcode(Ins.STA, AddrMode.Izy, 6), new Opcode(Ins.KIL, AddrMode.Imp, 0), new Opcode(Ins.AHX, AddrMode.Izy, 6), new Opcode(Ins.STY, AddrMode.Zpx, 4), new Opcode(Ins.STA, AddrMode.Zpx, 4), new Opcode(Ins.STX, AddrMode.Zpy, 4), new Opcode(Ins.SAX, AddrMode.Zpy, 4), new Opcode(Ins.TYA, AddrMode.Imp, 2), new Opcode(Ins.STA, AddrMode.Aby, 5), new Opcode(Ins.TXS, AddrMode.Imp, 2), new Opcode(Ins.TAS, AddrMode.Aby, 5), new Opcode(Ins.SHY, AddrMode.Abx, 5), new Opcode(Ins.STA, AddrMode.Abx, 5), new Opcode(Ins.SHX, AddrMode.Aby, 5), new Opcode(Ins.AHX, AddrMode.Aby, 5),
        new Opcode(Ins.LDY, AddrMode.Imm, 2), new Opcode(Ins.LDA, AddrMode.Izx, 6), new Opcode(Ins.LDX, AddrMode.Imm, 2), new Opcode(Ins.LAX, AddrMode.Izx, 6), new Opcode(Ins.LDY, AddrMode.Zpg, 3), new Opcode(Ins.LDA, AddrMode.Zpg, 3), new Opcode(Ins.LDX, AddrMode.Zpg, 3), new Opcode(Ins.LAX, AddrMode.Zpg, 3), new Opcode(Ins.TAY, AddrMode.Imp, 2), new Opcode(Ins.LDA, AddrMode.Imm, 2), new Opcode(Ins.TAX, AddrMode.Imp, 2), new Opcode(Ins.LAX, AddrMode.Imm, 2), new Opcode(Ins.LDY, AddrMode.Abs, 4), new Opcode(Ins.LDA, AddrMode.Abs, 4), new Opcode(Ins.LDX, AddrMode.Abs, 4), new Opcode(Ins.LAX, AddrMode.Abs, 4),
        new Opcode(Ins.BCS, AddrMode.Rel, 2), new Opcode(Ins.LDA, AddrMode.Izy, 5), new Opcode(Ins.KIL, AddrMode.Imp, 0), new Opcode(Ins.LAX, AddrMode.Izy, 5), new Opcode(Ins.LDY, AddrMode.Zpx, 4), new Opcode(Ins.LDA, AddrMode.Zpx, 4), new Opcode(Ins.LDX, AddrMode.Zpy, 4), new Opcode(Ins.LAX, AddrMode.Zpy, 4), new Opcode(Ins.CLV, AddrMode.Imp, 2), new Opcode(Ins.LDA, AddrMode.Aby, 4), new Opcode(Ins.TSX, AddrMode.Imp, 2), new Opcode(Ins.LAS, AddrMode.Aby, 4), new Opcode(Ins.LDY, AddrMode.Abx, 4), new Opcode(Ins.LDA, AddrMode.Abx, 4), new Opcode(Ins.LDX, AddrMode.Aby, 4), new Opcode(Ins.LAX, AddrMode.Aby, 4),
        new Opcode(Ins.CPY, AddrMode.Imm, 2), new Opcode(Ins.CMP, AddrMode.Izx, 6), new Opcode(Ins.NOP, AddrMode.Imm, 2), new Opcode(Ins.DCP, AddrMode.Izx, 8), new Opcode(Ins.CPY, AddrMode.Zpg, 3), new Opcode(Ins.CMP, AddrMode.Zpg, 3), new Opcode(Ins.DEC, AddrMode.Zpg, 5), new Opcode(Ins.DCP, AddrMode.Zpg, 5), new Opcode(Ins.INY, AddrMode.Imp, 2), new Opcode(Ins.CMP, AddrMode.Imm, 2), new Opcode(Ins.DEX, AddrMode.Imp, 2), new Opcode(Ins.AXS, AddrMode.Imm, 2), new Opcode(Ins.CPY, AddrMode.Abs, 4), new Opcode(Ins.CMP, AddrMode.Abs, 4), new Opcode(Ins.DEC, AddrMode.Abs, 6), new Opcode(Ins.DCP, AddrMode.Abs, 6),
        new Opcode(Ins.BNE, AddrMode.Rel, 2), new Opcode(Ins.CMP, AddrMode.Izy, 5), new Opcode(Ins.KIL, AddrMode.Imp, 0), new Opcode(Ins.DCP, AddrMode.Izy, 8), new Opcode(Ins.NOP, AddrMode.Zpx, 4), new Opcode(Ins.CMP, AddrMode.Zpx, 4), new Opcode(Ins.DEC, AddrMode.Zpx, 6), new Opcode(Ins.DCP, AddrMode.Zpx, 6), new Opcode(Ins.CLD, AddrMode.Imp, 2), new Opcode(Ins.CMP, AddrMode.Aby, 4), new Opcode(Ins.NOP, AddrMode.Imp, 2), new Opcode(Ins.DCP, AddrMode.Aby, 7), new Opcode(Ins.NOP, AddrMode.Abx, 4), new Opcode(Ins.CMP, AddrMode.Abx, 4), new Opcode(Ins.DEC, AddrMode.Abx, 7), new Opcode(Ins.DCP, AddrMode.Abx, 7),
        new Opcode(Ins.CPX, AddrMode.Imm, 2), new Opcode(Ins.SBC, AddrMode.Izx, 6), new Opcode(Ins.NOP, AddrMode.Imm, 2), new Opcode(Ins.ISC, AddrMode.Izx, 8), new Opcode(Ins.CPX, AddrMode.Zpg, 3), new Opcode(Ins.SBC, AddrMode.Zpg, 3), new Opcode(Ins.INC, AddrMode.Zpg, 5), new Opcode(Ins.ISC, AddrMode.Zpg, 5), new Opcode(Ins.INX, AddrMode.Imp, 2), new Opcode(Ins.SBC, AddrMode.Imm, 2), new Opcode(Ins.NOP, AddrMode.Imp, 2), new Opcode(Ins.SBC, AddrMode.Imm, 2), new Opcode(Ins.CPX, AddrMode.Abs, 4), new Opcode(Ins.SBC, AddrMode.Abs, 4), new Opcode(Ins.INC, AddrMode.Abs, 6), new Opcode(Ins.ISC, AddrMode.Abs, 6),
        new Opcode(Ins.BEQ, AddrMode.Rel, 2), new Opcode(Ins.SBC, AddrMode.Izy, 5), new Opcode(Ins.KIL, AddrMode.Imp, 0), new Opcode(Ins.ISC, AddrMode.Izy, 8), new Opcode(Ins.NOP, AddrMode.Zpx, 4), new Opcode(Ins.SBC, AddrMode.Zpx, 4), new Opcode(Ins.INC, AddrMode.Zpx, 6), new Opcode(Ins.ISC, AddrMode.Zpx, 6), new Opcode(Ins.SED, AddrMode.Imp, 2), new Opcode(Ins.SBC, AddrMode.Aby, 4), new Opcode(Ins.NOP, AddrMode.Imp, 2), new Opcode(Ins.ISC, AddrMode.Aby, 7), new Opcode(Ins.NOP, AddrMode.Abx, 4), new Opcode(Ins.SBC, AddrMode.Abx, 4), new Opcode(Ins.INC, AddrMode.Abx, 7), new Opcode(Ins.ISC, AddrMode.Abx, 7),
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
        Opcode opcode = Opcodes[bus.Read8(regPC++)];
        switch (opcode.instruction)
        {
            default:
                throw new NesException(string.Format("Unknown opcode: {0}", opcode.instruction));
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

    private ushort Addressing(AddrMode mode)
    {
        ushort addr = 0;
        switch (mode)
        {
            case AddrMode.Imm:
                addr = regPC;
                regPC += 1;
                break;

            case AddrMode.Zpg:
                addr = bus.Read8(regPC);
                regPC += 1;
                break;

            case AddrMode.Zpx:
                addr = bus.Read8(regPC);
                regPC += 1;
                addr += regX;
                break;

            case AddrMode.Zpy:
                addr = bus.Read8(regPC);
                regPC += 1;
                addr += regY;
                break;

            case AddrMode.Izx:
                addr = bus.Read8(regPC);
                regPC += 1;
                addr += regX;
                addr = bus.Read16(addr);
                break;

            case AddrMode.Izy:
                addr = bus.Read8(regPC);
                regPC += 1;
                addr = bus.Read16(addr);
                addr += regY;
                break;

            case AddrMode.Abs:
                addr = bus.Read16(regPC);
                regPC += 2;
                break;

            case AddrMode.Abx:
                addr = bus.Read16(regPC);
                regPC += 2;
                addr += regX;
                break;

            case AddrMode.Aby:
                addr = bus.Read16(regPC);
                regPC += 2;
                addr += regY;
                break;

            case AddrMode.Ind:
                int low = bus.Read16(regPC);
                int high = low + 1;
                // 2A03 bug，间接寻址不能跨页
                high = (low & 0xFF00) | (high & 0x00FF);
                addr = (ushort)(bus.Read8((ushort)low) | (bus.Read8((ushort)(high)) << 8));
                regPC += 2;
                break;

            case AddrMode.Rel:
                addr = bus.Read8(regPC);
                regPC += 1;
                addr = (ushort)(regPC + (short)addr);
                break;

            default:
                throw new NesException("AddrMode error");
        }
        return addr;
    }
}

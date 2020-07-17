using UnityEngine;
using UnityEditor;
using System;

// http://obelisk.me.uk/6502/
public class CPU
{
    public enum AddrMode
    {
        Imp, // Implicit 特殊指令的寻址方式
        //Acc, // Accumulator 累加器寻址。跟Imp没啥区别，统一用Imp了
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
        public Action<ushort> instruction;
        public AddrMode addrMode;
        public int cycles;
        public bool extraCycles;

        public Opcode(string name, Action<ushort> instruction, AddrMode addrMode, int cycles, bool extraCycles)
        {
            this.name = name;
            this.instruction = instruction;
            this.addrMode = addrMode;
            this.cycles = cycles;
            this.extraCycles = extraCycles;
        }
    }
    
    public enum Flag : byte
    {
        C = 1 << 0,    // 进位标记(Carry flag)
        Z = 1 << 1,    // 零标记 (Zero flag)
        I = 1 << 2,    // 禁止中断(Irq disabled flag)
        D = 1 << 3,    // 十进制模式(Decimal mode flag)
        B = 1 << 4,    // 软件中断(BRK flag)
        R = 1 << 5,    // 保留标记(Reserved) 一直为1
        V = 1 << 6,    // 溢出标记(Overflow flag)
        N = 1 << 7,    // 负标志(Negative flag)
    }

    // http://obelisk.me.uk/6502/registers.html
    public ushort PC;
    public byte A;
    public byte X;
    public byte Y;
    public byte S; // Stack pointer
    public Flag P;

    public long cycles;
    public Opcode[] opcodes;
    public Bus bus;

    private bool isImplicitAddressing;
    private bool isPageCrossed;
    private bool isBranchTaken;

    public CPU(Bus bus)
    {
        // http://obelisk.me.uk/6502/reference.html
        // http://www.oxyron.de/html/opcodes02.html
        opcodes = new Opcode[]
        {
            //  x0                                              x1                                              x2                                              x3                                              x4                                              x5                                              x6                                              x7                                              x8                                              x9                                              xA                                              xB                                              xC                                              xD                                              xE                                              xF
/* 0x */    new Opcode("BRK", BRK, AddrMode.Imp, 7, false), new Opcode("ORA", ORA, AddrMode.Izx, 6, false), new Opcode("KIL", KIL, AddrMode.Imp, 0, false), new Opcode("SLO", SLO, AddrMode.Izx, 8, false), new Opcode("NOP", NOP, AddrMode.Zpg, 3, false), new Opcode("ORA", ORA, AddrMode.Zpg, 3, false), new Opcode("ASL", ASL, AddrMode.Zpg, 5, false), new Opcode("SLO", SLO, AddrMode.Zpg, 5, false), new Opcode("PHP", PHP, AddrMode.Imp, 3, false), new Opcode("ORA", ORA, AddrMode.Imm, 2, false), new Opcode("ASL", ASL, AddrMode.Imp, 2, false), new Opcode("ANC", ANC, AddrMode.Imm, 2, false), new Opcode("NOP", NOP, AddrMode.Abs, 4, false), new Opcode("ORA", ORA, AddrMode.Abs, 4, false), new Opcode("ASL", ASL, AddrMode.Abs, 6, false), new Opcode("SLO", SLO, AddrMode.Abs, 6, false),
/* 1x */    new Opcode("BPL", BPL, AddrMode.Rel, 2, true),  new Opcode("ORA", ORA, AddrMode.Izy, 5, true),  new Opcode("KIL", KIL, AddrMode.Imp, 0, false), new Opcode("SLO", SLO, AddrMode.Izy, 8, false), new Opcode("NOP", NOP, AddrMode.Zpx, 4, false), new Opcode("ORA", ORA, AddrMode.Zpx, 4, false), new Opcode("ASL", ASL, AddrMode.Zpx, 6, false), new Opcode("SLO", SLO, AddrMode.Zpx, 6, false), new Opcode("CLC", CLC, AddrMode.Imp, 2, false), new Opcode("ORA", ORA, AddrMode.Aby, 4, true),  new Opcode("NOP", NOP, AddrMode.Imp, 2, false), new Opcode("SLO", SLO, AddrMode.Aby, 7, false), new Opcode("NOP", NOP, AddrMode.Abx, 4, true),  new Opcode("ORA", ORA, AddrMode.Abx, 4, true),  new Opcode("ASL", ASL, AddrMode.Abx, 7, false), new Opcode("SLO", SLO, AddrMode.Abx, 7, false),
/* 2x */    new Opcode("JSR", JSR, AddrMode.Abs, 6, false), new Opcode("AND", AND, AddrMode.Izx, 6, false), new Opcode("KIL", KIL, AddrMode.Imp, 0, false), new Opcode("RLA", RLA, AddrMode.Izx, 8, false), new Opcode("BIT", BIT, AddrMode.Zpg, 3, false), new Opcode("AND", AND, AddrMode.Zpg, 3, false), new Opcode("ROL", ROL, AddrMode.Zpg, 5, false), new Opcode("RLA", RLA, AddrMode.Zpg, 5, false), new Opcode("PLP", PLP, AddrMode.Imp, 4, false), new Opcode("AND", AND, AddrMode.Imm, 2, false), new Opcode("ROL", ROL, AddrMode.Imp, 2, false), new Opcode("ANC", ANC, AddrMode.Imm, 2, false), new Opcode("BIT", BIT, AddrMode.Abs, 4, false), new Opcode("AND", AND, AddrMode.Abs, 4, false), new Opcode("ROL", ROL, AddrMode.Abs, 6, false), new Opcode("RLA", RLA, AddrMode.Abs, 6, false),
/* 3x */    new Opcode("BMI", BMI, AddrMode.Rel, 2, true),  new Opcode("AND", AND, AddrMode.Izy, 5, true),  new Opcode("KIL", KIL, AddrMode.Imp, 0, false), new Opcode("RLA", RLA, AddrMode.Izy, 8, false), new Opcode("NOP", NOP, AddrMode.Zpx, 4, false), new Opcode("AND", AND, AddrMode.Zpx, 4, false), new Opcode("ROL", ROL, AddrMode.Zpx, 6, false), new Opcode("RLA", RLA, AddrMode.Zpx, 6, false), new Opcode("SEC", SEC, AddrMode.Imp, 2, false), new Opcode("AND", AND, AddrMode.Aby, 4, true),  new Opcode("NOP", NOP, AddrMode.Imp, 2, false), new Opcode("RLA", RLA, AddrMode.Aby, 7, false), new Opcode("NOP", NOP, AddrMode.Abx, 4, true),  new Opcode("AND", AND, AddrMode.Abx, 4, true),  new Opcode("ROL", ROL, AddrMode.Abx, 7, false), new Opcode("RLA", RLA, AddrMode.Abx, 7, false),
/* 4x */    new Opcode("RTI", RTI, AddrMode.Imp, 6, false), new Opcode("EOR", EOR, AddrMode.Izx, 6, false), new Opcode("KIL", KIL, AddrMode.Imp, 0, false), new Opcode("SRE", SRE, AddrMode.Izx, 8, false), new Opcode("NOP", NOP, AddrMode.Zpg, 3, false), new Opcode("EOR", EOR, AddrMode.Zpg, 3, false), new Opcode("LSR", LSR, AddrMode.Zpg, 5, false), new Opcode("SRE", SRE, AddrMode.Zpg, 5, false), new Opcode("PHA", PHA, AddrMode.Imp, 3, false), new Opcode("EOR", EOR, AddrMode.Imm, 2, false), new Opcode("LSR", LSR, AddrMode.Imp, 2, false), new Opcode("ALR", ALR, AddrMode.Imm, 2, false), new Opcode("JMP", JMP, AddrMode.Abs, 3, false), new Opcode("EOR", EOR, AddrMode.Abs, 4, false), new Opcode("LSR", LSR, AddrMode.Abs, 6, false), new Opcode("SRE", SRE, AddrMode.Abs, 6, false),
/* 5x */    new Opcode("BVC", BVC, AddrMode.Rel, 2, true),  new Opcode("EOR", EOR, AddrMode.Izy, 5, true),  new Opcode("KIL", KIL, AddrMode.Imp, 0, false), new Opcode("SRE", SRE, AddrMode.Izy, 8, false), new Opcode("NOP", NOP, AddrMode.Zpx, 4, false), new Opcode("EOR", EOR, AddrMode.Zpx, 4, false), new Opcode("LSR", LSR, AddrMode.Zpx, 6, false), new Opcode("SRE", SRE, AddrMode.Zpx, 6, false), new Opcode("CLI", CLI, AddrMode.Imp, 2, false), new Opcode("EOR", EOR, AddrMode.Aby, 4, true),  new Opcode("NOP", NOP, AddrMode.Imp, 2, false), new Opcode("SRE", SRE, AddrMode.Aby, 7, false), new Opcode("NOP", NOP, AddrMode.Abx, 4, true),  new Opcode("EOR", EOR, AddrMode.Abx, 4, true),  new Opcode("LSR", LSR, AddrMode.Abx, 7, false), new Opcode("SRE", SRE, AddrMode.Abx, 7, false),
/* 6x */    new Opcode("RTS", RTS, AddrMode.Imp, 6, false), new Opcode("ADC", ADC, AddrMode.Izx, 6, false), new Opcode("KIL", KIL, AddrMode.Imp, 0, false), new Opcode("RRA", RRA, AddrMode.Izx, 8, false), new Opcode("NOP", NOP, AddrMode.Zpg, 3, false), new Opcode("ADC", ADC, AddrMode.Zpg, 3, false), new Opcode("ROR", ROR, AddrMode.Zpg, 5, false), new Opcode("RRA", RRA, AddrMode.Zpg, 5, false), new Opcode("PLA", PLA, AddrMode.Imp, 4, false), new Opcode("ADC", ADC, AddrMode.Imm, 2, false), new Opcode("ROR", ROR, AddrMode.Imp, 2, false), new Opcode("ARR", ARR, AddrMode.Imm, 2, false), new Opcode("JMP", JMP, AddrMode.Ind, 5, false), new Opcode("ADC", ADC, AddrMode.Abs, 4, false), new Opcode("ROR", ROR, AddrMode.Abs, 6, false), new Opcode("RRA", RRA, AddrMode.Abs, 6, false),
/* 7x */    new Opcode("BVS", BVS, AddrMode.Rel, 2, true),  new Opcode("ADC", ADC, AddrMode.Izy, 5, true),  new Opcode("KIL", KIL, AddrMode.Imp, 0, false), new Opcode("RRA", RRA, AddrMode.Izy, 8, false), new Opcode("NOP", NOP, AddrMode.Zpx, 4, false), new Opcode("ADC", ADC, AddrMode.Zpx, 4, false), new Opcode("ROR", ROR, AddrMode.Zpx, 6, false), new Opcode("RRA", RRA, AddrMode.Zpx, 6, false), new Opcode("SEI", SEI, AddrMode.Imp, 2, false), new Opcode("ADC", ADC, AddrMode.Aby, 4, true),  new Opcode("NOP", NOP, AddrMode.Imp, 2, false), new Opcode("RRA", RRA, AddrMode.Aby, 7, false), new Opcode("NOP", NOP, AddrMode.Abx, 4, true),  new Opcode("ADC", ADC, AddrMode.Abx, 4, true),  new Opcode("ROR", ROR, AddrMode.Abx, 7, false), new Opcode("RRA", RRA, AddrMode.Abx, 7, false),
/* 8x */    new Opcode("NOP", NOP, AddrMode.Imm, 2, false), new Opcode("STA", STA, AddrMode.Izx, 6, false), new Opcode("NOP", NOP, AddrMode.Imm, 2, false), new Opcode("SAX", SAX, AddrMode.Izx, 6, false), new Opcode("STY", STY, AddrMode.Zpg, 3, false), new Opcode("STA", STA, AddrMode.Zpg, 3, false), new Opcode("STX", STX, AddrMode.Zpg, 3, false), new Opcode("SAX", SAX, AddrMode.Zpg, 3, false), new Opcode("DEY", DEY, AddrMode.Imp, 2, false), new Opcode("NOP", NOP, AddrMode.Imm, 2, false), new Opcode("TXA", TXA, AddrMode.Imp, 2, false), new Opcode("XAA", XAA, AddrMode.Imm, 2, false), new Opcode("STY", STY, AddrMode.Abs, 4, false), new Opcode("STA", STA, AddrMode.Abs, 4, false), new Opcode("STX", STX, AddrMode.Abs, 4, false), new Opcode("SAX", SAX, AddrMode.Abs, 4, false),
/* 9x */    new Opcode("BCC", BCC, AddrMode.Rel, 2, true),  new Opcode("STA", STA, AddrMode.Izy, 6, false), new Opcode("KIL", KIL, AddrMode.Imp, 0, false), new Opcode("AHX", AHX, AddrMode.Izy, 6, false), new Opcode("STY", STY, AddrMode.Zpx, 4, false), new Opcode("STA", STA, AddrMode.Zpx, 4, false), new Opcode("STX", STX, AddrMode.Zpy, 4, false), new Opcode("SAX", SAX, AddrMode.Zpy, 4, false), new Opcode("TYA", TYA, AddrMode.Imp, 2, false), new Opcode("STA", STA, AddrMode.Aby, 5, false), new Opcode("TXS", TXS, AddrMode.Imp, 2, false), new Opcode("TAS", TAS, AddrMode.Aby, 5, false), new Opcode("SHY", SHY, AddrMode.Abx, 5, false), new Opcode("STA", STA, AddrMode.Abx, 5, false), new Opcode("SHX", SHX, AddrMode.Aby, 5, false), new Opcode("AHX", AHX, AddrMode.Aby, 5, false),
/* Ax */    new Opcode("LDY", LDY, AddrMode.Imm, 2, false), new Opcode("LDA", LDA, AddrMode.Izx, 6, false), new Opcode("LDX", LDX, AddrMode.Imm, 2, false), new Opcode("LAX", LAX, AddrMode.Izx, 6, false), new Opcode("LDY", LDY, AddrMode.Zpg, 3, false), new Opcode("LDA", LDA, AddrMode.Zpg, 3, false), new Opcode("LDX", LDX, AddrMode.Zpg, 3, false), new Opcode("LAX", LAX, AddrMode.Zpg, 3, false), new Opcode("TAY", TAY, AddrMode.Imp, 2, false), new Opcode("LDA", LDA, AddrMode.Imm, 2, false), new Opcode("TAX", TAX, AddrMode.Imp, 2, false), new Opcode("LAX", LAX, AddrMode.Imm, 2, false), new Opcode("LDY", LDY, AddrMode.Abs, 4, false), new Opcode("LDA", LDA, AddrMode.Abs, 4, false), new Opcode("LDX", LDX, AddrMode.Abs, 4, false), new Opcode("LAX", LAX, AddrMode.Abs, 4, false),
/* Bx */    new Opcode("BCS", BCS, AddrMode.Rel, 2, true),  new Opcode("LDA", LDA, AddrMode.Izy, 5, true),  new Opcode("KIL", KIL, AddrMode.Imp, 0, false), new Opcode("LAX", LAX, AddrMode.Izy, 5, true),  new Opcode("LDY", LDY, AddrMode.Zpx, 4, false), new Opcode("LDA", LDA, AddrMode.Zpx, 4, false), new Opcode("LDX", LDX, AddrMode.Zpy, 4, false), new Opcode("LAX", LAX, AddrMode.Zpy, 4, false), new Opcode("CLV", CLV, AddrMode.Imp, 2, false), new Opcode("LDA", LDA, AddrMode.Aby, 4, true),  new Opcode("TSX", TSX, AddrMode.Imp, 2, false), new Opcode("LAS", LAS, AddrMode.Aby, 4, true),  new Opcode("LDY", LDY, AddrMode.Abx, 4, true),  new Opcode("LDA", LDA, AddrMode.Abx, 4, true),  new Opcode("LDX", LDX, AddrMode.Aby, 4, true),  new Opcode("LAX", LAX, AddrMode.Aby, 4, true),
/* Cx */    new Opcode("CPY", CPY, AddrMode.Imm, 2, false), new Opcode("CMP", CMP, AddrMode.Izx, 6, false), new Opcode("NOP", NOP, AddrMode.Imm, 2, false), new Opcode("DCP", DCP, AddrMode.Izx, 8, false), new Opcode("CPY", CPY, AddrMode.Zpg, 3, false), new Opcode("CMP", CMP, AddrMode.Zpg, 3, false), new Opcode("DEC", DEC, AddrMode.Zpg, 5, false), new Opcode("DCP", DCP, AddrMode.Zpg, 5, false), new Opcode("INY", INY, AddrMode.Imp, 2, false), new Opcode("CMP", CMP, AddrMode.Imm, 2, false), new Opcode("DEX", DEX, AddrMode.Imp, 2, false), new Opcode("AXS", AXS, AddrMode.Imm, 2, false), new Opcode("CPY", CPY, AddrMode.Abs, 4, false), new Opcode("CMP", CMP, AddrMode.Abs, 4, false), new Opcode("DEC", DEC, AddrMode.Abs, 6, false), new Opcode("DCP", DCP, AddrMode.Abs, 6, false),
/* Dx */    new Opcode("BNE", BNE, AddrMode.Rel, 2, true),  new Opcode("CMP", CMP, AddrMode.Izy, 5, true),  new Opcode("KIL", KIL, AddrMode.Imp, 0, false), new Opcode("DCP", DCP, AddrMode.Izy, 8, false), new Opcode("NOP", NOP, AddrMode.Zpx, 4, false), new Opcode("CMP", CMP, AddrMode.Zpx, 4, false), new Opcode("DEC", DEC, AddrMode.Zpx, 6, false), new Opcode("DCP", DCP, AddrMode.Zpx, 6, false), new Opcode("CLD", CLD, AddrMode.Imp, 2, false), new Opcode("CMP", CMP, AddrMode.Aby, 4, true),  new Opcode("NOP", NOP, AddrMode.Imp, 2, false), new Opcode("DCP", DCP, AddrMode.Aby, 7, false), new Opcode("NOP", NOP, AddrMode.Abx, 4, true),  new Opcode("CMP", CMP, AddrMode.Abx, 4, true),  new Opcode("DEC", DEC, AddrMode.Abx, 7, false), new Opcode("DCP", DCP, AddrMode.Abx, 7, false),
/* Ex */    new Opcode("CPX", CPX, AddrMode.Imm, 2, false), new Opcode("SBC", SBC, AddrMode.Izx, 6, false), new Opcode("NOP", NOP, AddrMode.Imm, 2, false), new Opcode("ISC", ISC, AddrMode.Izx, 8, false), new Opcode("CPX", CPX, AddrMode.Zpg, 3, false), new Opcode("SBC", SBC, AddrMode.Zpg, 3, false), new Opcode("INC", INC, AddrMode.Zpg, 5, false), new Opcode("ISC", ISC, AddrMode.Zpg, 5, false), new Opcode("INX", INX, AddrMode.Imp, 2, false), new Opcode("SBC", SBC, AddrMode.Imm, 2, false), new Opcode("NOP", NOP, AddrMode.Imp, 2, false), new Opcode("SBC", SBC, AddrMode.Imm, 2, false), new Opcode("CPX", CPX, AddrMode.Abs, 4, false), new Opcode("SBC", SBC, AddrMode.Abs, 4, false), new Opcode("INC", INC, AddrMode.Abs, 6, false), new Opcode("ISC", ISC, AddrMode.Abs, 6, false),
/* Fx */    new Opcode("BEQ", BEQ, AddrMode.Rel, 2, true),  new Opcode("SBC", SBC, AddrMode.Izy, 5, true),  new Opcode("KIL", KIL, AddrMode.Imp, 0, false), new Opcode("ISC", ISC, AddrMode.Izy, 8, false), new Opcode("NOP", NOP, AddrMode.Zpx, 4, false), new Opcode("SBC", SBC, AddrMode.Zpx, 4, false), new Opcode("INC", INC, AddrMode.Zpx, 6, false), new Opcode("ISC", ISC, AddrMode.Zpx, 6, false), new Opcode("SED", SED, AddrMode.Imp, 2, false), new Opcode("SBC", SBC, AddrMode.Aby, 4, true),  new Opcode("NOP", NOP, AddrMode.Imp, 2, false), new Opcode("ISC", ISC, AddrMode.Aby, 7, false), new Opcode("NOP", NOP, AddrMode.Abx, 4, true),  new Opcode("SBC", SBC, AddrMode.Abx, 4, true),  new Opcode("INC", INC, AddrMode.Abx, 7, false), new Opcode("ISC", ISC, AddrMode.Abx, 7, false),
        };

        this.bus = bus;
        Reset();
    }

    public void Tick()
    {
        isImplicitAddressing = false;
        isPageCrossed = false;
        isBranchTaken = false;

        Opcode opcode = opcodes[bus.Read8(PC++)];
        opcode.instruction(Addressing(opcode.addrMode));
        cycles += opcode.cycles;
        if (opcode.extraCycles)
        {
            CheckExtraCycles();
        }
    }

    public void Reset()
    {
        A = 0;
        X = 0;
        Y = 0;
        P = Flag.R;
        S = 0xFD;
        //regPC = bus.Read16(0xFFFC);
        PC = 0xC000;
    }

    public void NMI()
    {

    }

    public void IRQ()
    {

    }

    private void SetFlag(Flag f)
    {
        P |= f;
    }

    private void ClearFlag(Flag f)
    {
        P &= ~f;
    }

    private byte GetFlag(Flag f)
    {
        return (byte)((P & f) > 0 ? 1 : 0);
    }

    private void CheckFlagZ(byte v)
    {
        if (v == 0)
        {
            SetFlag(Flag.Z);
        }
        else
        {
            ClearFlag(Flag.Z);
        }
    }

    private void CheckFlagN(byte v)
    {
        if ((v & 0x80) != 0)
        {
            SetFlag(Flag.N);
        }
        else
        {
            ClearFlag(Flag.N);
        }
    }

    // 新旧地址不同页时，增加1个cycle。分支跳转时，增加1个cycle
    private void CheckExtraCycles()
    {
        if (isPageCrossed)
        {
            cycles += 1;
        }
        if (isBranchTaken)
        {
            cycles += 1;
        }
    }

    private void PushStack8(byte value)
    {
        bus.Write8((ushort)(0x100 | S), value);
        S -= 1;
    }

    private byte PopStack8()
    {
        S += 1;
        return bus.Read8((ushort)(0x100 | S));
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
                addr = PC;
                PC += 1;
                break;

            case AddrMode.Zpg:
                addr = bus.Read8(PC);
                PC += 1;
                break;

            case AddrMode.Zpx:
                addr = bus.Read8(PC);
                PC += 1;
                addr += X;
                break;

            case AddrMode.Zpy:
                addr = bus.Read8(PC);
                PC += 1;
                addr += Y;
                break;

            case AddrMode.Izx:
                addr = bus.Read8(PC);
                PC += 1;
                addr += X;
                addr = bus.Read16(addr);
                break;

            case AddrMode.Izy:
                addr = bus.Read8(PC);
                PC += 1;
                addr = bus.Read16(addr);
                addr += Y;
                break;

            case AddrMode.Abs:
                addr = bus.Read16(PC);
                PC += 2;
                break;

            case AddrMode.Abx:
                addr = bus.Read16(PC);
                PC += 2;
                addr += X;
                break;

            case AddrMode.Aby:
                addr = bus.Read16(PC);
                PC += 2;
                addr += Y;
                break;

            case AddrMode.Ind:
                int low = bus.Read16(PC);
                int high = low + 1;
                // 2A03 bug，间接寻址不能跨页
                high = (low & 0xFF00) | (high & 0x00FF);
                addr = (ushort)(bus.Read8((ushort)low) | (bus.Read8((ushort)(high)) << 8));
                PC += 2;
                break;

            case AddrMode.Rel:
                addr = bus.Read8(PC);
                PC += 1;
                addr = (ushort)(PC + (short)addr);
                break;

            default:
                isImplicitAddressing = true;
                break;
        }
        return addr;
    }

    private void ADC(ushort addr)
    {
        int sum = A + bus.Read8(addr) + GetFlag(Flag.C);
        // 把所有数当做无符号数看待，运算结果超过255，则进位
        if (sum > 0xFF)
        {
            SetFlag(Flag.C);
        }
        else
        {
            ClearFlag(Flag.C);
        }
        // 把所有数当做有符号数看待，运算结果大于127或小于-128（即符号位发生过变化），则溢出
        // 溢出的情况有2种：
        // 1、两个正数相加，溢出得到负数
        // 2、两个负数相加，溢出得到正数
        // 当正数和负数相加时，永远不会发生溢出
        if ((A & 0x80) != (sum & 0x80))
        {
            SetFlag(Flag.V);
        }
        else
        {
            ClearFlag(Flag.V);
        }

        A = (byte)(sum & 0xFF);
        CheckFlagN(A);
        CheckFlagZ(A);
    }

    private void AHX(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void ALR(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void ANC(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void AND(ushort addr)
    {
        A &= bus.Read8(addr);
        CheckFlagN(A);
        CheckFlagZ(A);
    }

    private void ARR(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void ASL(ushort addr)
    {
        if (isImplicitAddressing)
        {
            if ((A & 0x80) > 0)
            {
                SetFlag(Flag.C);
            }
            else
            {
                ClearFlag(Flag.C);
            }
            A <<= 1;
            CheckFlagN(A);
            CheckFlagZ(A);
        }
        else
        {
            byte v = bus.Read8(addr);
            if ((v & 0x80) > 0)
            {
                SetFlag(Flag.C);
            }
            else
            {
                ClearFlag(Flag.C);
            }
            v <<= 1;
            bus.Write8(addr, v);
            CheckFlagN(v);
            CheckFlagZ(v);
        }
    }

    private void AXS(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void BCC(ushort addr)
    {
        if (GetFlag(Flag.C) == 0)
        {
            PC = addr;
        }
    }

    private void BCS(ushort addr)
    {
        if (GetFlag(Flag.C) == 1)
        {
            PC = addr;
        }
    }

    private void BEQ(ushort addr)
    {
        if (GetFlag(Flag.Z) == 1)
        {
            PC = addr;
        }
    }

    private void BIT(ushort addr)
    {
        byte v = bus.Read8(addr);
        if ((v & A) == 0)
        {
            SetFlag(Flag.Z);
        }
        if ((v & 0x40) > 0)
        {
            SetFlag(Flag.V);
        }
        else
        {
            ClearFlag(Flag.V);
        }
        if ((v & 0x80) > 0)
        {
            SetFlag(Flag.N);
        }
        else
        {
            ClearFlag(Flag.N);
        }
    }

    private void BMI(ushort addr)
    {
        if (GetFlag(Flag.N) == 1)
        {
            PC = addr;
        }
    }

    private void BNE(ushort addr)
    {
        if (GetFlag(Flag.Z) == 0)
        {
            PC = addr;
        }
    }

    private void BPL(ushort addr)
    {
        if (GetFlag(Flag.N) == 0)
        {
            PC = addr;
        }
    }

    private void BRK(ushort addr)
    {
        SetFlag(Flag.I);
    }

    private void BVC(ushort addr)
    {
        if (GetFlag(Flag.V) == 0)
        {
            PC = addr;
        }
    }

    private void BVS(ushort addr)
    {
        if (GetFlag(Flag.V) == 1)
        {
            PC = addr;
        }
    }

    private void CLC(ushort addr)
    {
        ClearFlag(Flag.C);
    }

    private void CLD(ushort addr)
    {
        ClearFlag(Flag.D);
    }

    private void CLI(ushort addr)
    {
        ClearFlag(Flag.I);
    }

    private void CLV(ushort addr)
    {
        ClearFlag(Flag.V);
    }

    private void CMP(ushort addr)
    {
        byte v = bus.Read8(addr);
        int d = A - v;
        if (d >= 0)
        {
            SetFlag(Flag.C);
        }
        else
        {
            ClearFlag(Flag.C);
        }
        CheckFlagN((byte)d);
        CheckFlagZ((byte)d);
    }

    private void CPX(ushort addr)
    {
        byte v = bus.Read8(addr);
        int d = X - v;
        if (d >= 0)
        {
            SetFlag(Flag.C);
        }
        else
        {
            ClearFlag(Flag.C);
        }
        CheckFlagN((byte)d);
        CheckFlagZ((byte)d);
    }

    private void CPY(ushort addr)
    {
        byte v = bus.Read8(addr);
        int d = Y - v;
        if (d >= 0)
        {
            SetFlag(Flag.C);
        }
        else
        {
            ClearFlag(Flag.C);
        }
        CheckFlagN((byte)d);
        CheckFlagZ((byte)d);
    }

    private void DCP(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void DEC(ushort addr)
    {
        byte v = (byte)(bus.Read8(addr) - 1);
        bus.Write8(addr, v);
        CheckFlagN(v);
        CheckFlagZ(v);
    }

    private void DEX(ushort addr)
    {
        X -= 1;
        CheckFlagN(X);
        CheckFlagZ(X);
    }

    private void DEY(ushort addr)
    {
        Y -= 1;
        CheckFlagN(Y);
        CheckFlagZ(Y);
    }

    private void EOR(ushort addr)
    {
        A ^= bus.Read8(addr);
        CheckFlagN(A);
        CheckFlagZ(A);
    }

    private void INC(ushort addr)
    {
        byte v = (byte)(bus.Read8(addr) + 1);
        bus.Write8(addr, v);
        CheckFlagN(v);
        CheckFlagZ(v);
    }

    private void INX(ushort addr)
    {
        X += 1;
        CheckFlagN(X);
        CheckFlagZ(X);
    }

    private void INY(ushort addr)
    {
        Y += 1;
        CheckFlagN(Y);
        CheckFlagZ(Y);
    }

    private void ISC(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void JMP(ushort addr)
    {
        PC = addr;
    }

    private void JSR(ushort addr)
    {
        PushStack16(PC);
        PC = addr;
    }

    private void KIL(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void LAS(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void LAX(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void LDA(ushort addr)
    {
        A = bus.Read8(addr);
        CheckFlagN(A);
        CheckFlagZ(A);
    }

    private void LDX(ushort addr)
    {
        X = bus.Read8(addr);
        CheckFlagN(X);
        CheckFlagZ(X);
    }

    private void LDY(ushort addr)
    {
        Y = bus.Read8(addr);
        CheckFlagN(Y);
        CheckFlagZ(Y);
    }

    private void LSR(ushort addr)
    {
        if (isImplicitAddressing)
        {
            if ((A & 0x80) > 0)
            {
                SetFlag(Flag.C);
            }
            else
            {
                ClearFlag(Flag.C);
            }
            A >>= 1;
            CheckFlagN(A);
            CheckFlagZ(A);
        }
        else
        {
            byte v = bus.Read8(addr);
            if ((v & 0x80) > 0)
            {
                SetFlag(Flag.C);
            }
            else
            {
                ClearFlag(Flag.C);
            }
            v >>= 1;
            bus.Write8(addr, v);
            CheckFlagN(v);
            CheckFlagZ(v);
        }
    }

    private void NOP(ushort addr)
    {
    }

    private void ORA(ushort addr)
    {
        A |= bus.Read8(addr);
        CheckFlagN(A);
        CheckFlagZ(A);
    }

    private void PHA(ushort addr)
    {
        PushStack8(A);
    }

    private void PHP(ushort addr)
    {
        PushStack8((byte)P);
    }

    private void PLA(ushort addr)
    {
        A = PopStack8();
        CheckFlagN(A);
        CheckFlagZ(A);
    }

    private void PLP(ushort addr)
    {
        P = (Flag)PopStack8();
        CheckFlagN((byte)P);
        CheckFlagZ((byte)P);
    }

    private void RLA(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void ROL(ushort addr)
    {
        if (isImplicitAddressing)
        {
            byte c = GetFlag(Flag.C);
            if ((A & 0x80) > 0)
            {
                SetFlag(Flag.C);
            }
            else
            {
                ClearFlag(Flag.C);
            }
            A = (byte)((A << 1) | c);
        }
        else
        {
            byte c = GetFlag(Flag.C);
            byte v = bus.Read8(addr);
            if ((v & 0x80) > 0)
            {
                SetFlag(Flag.C);
            }
            else
            {
                ClearFlag(Flag.C);
            }
            v = (byte)((v << 1) | c);
            bus.Write8(addr, v);
        }
    }

    private void ROR(ushort addr)
    {
        if (isImplicitAddressing)
        {
            byte c = GetFlag(Flag.C);
            if ((A & 1) > 0)
            {
                SetFlag(Flag.C);
            }
            else
            {
                ClearFlag(Flag.C);
            }
            A = (byte)((A >> 1) | (c << 7));
        }
        else
        {
            byte c = GetFlag(Flag.C);
            byte v = bus.Read8(addr);
            if ((v & 1) > 0)
            {
                SetFlag(Flag.C);
            }
            else
            {
                ClearFlag(Flag.C);
            }
            v = (byte)((v >> 1) | (c << 7));
            bus.Write8(addr, v);
        }
    }

    private void RRA(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void RTI(ushort addr)
    {
        P = (Flag)PopStack8();
        PC = PopStack16();
    }

    private void RTS(ushort addr)
    {
        PC = PopStack16();
    }

    private void SAX(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void SBC(ushort addr)
    {
        int sum = A - bus.Read8(addr) - (1 - GetFlag(Flag.C));
        if (sum >= 0)
        {
            SetFlag(Flag.C);
        }
        else
        {
            ClearFlag(Flag.C);
        }
        // 同ADC
        if ((A & 0x80) != (sum & 0x80))
        {
            SetFlag(Flag.V);
        }
        else
        {
            ClearFlag(Flag.V);
        }

        A = (byte)(sum & 0xFF);
        CheckFlagN(A);
        CheckFlagZ(A);
    }

    private void SEC(ushort addr)
    {
        SetFlag(Flag.C);
    }

    private void SED(ushort addr)
    {
        SetFlag(Flag.D);
    }

    private void SEI(ushort addr)
    {
        SetFlag(Flag.I);
    }

    private void SHX(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void SHY(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void SLO(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void SRE(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void STA(ushort addr)
    {
        bus.Write8(addr, A);
    }

    private void STX(ushort addr)
    {
        bus.Write8(addr, X);
    }

    private void STY(ushort addr)
    {
        bus.Write8(addr, Y);
    }

    private void TAS(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }

    private void TAX(ushort addr)
    {
        X = A;
        CheckFlagN(X);
        CheckFlagZ(X);
    }

    private void TAY(ushort addr)
    {
        Y = A;
        CheckFlagN(Y);
        CheckFlagZ(Y);
    }

    private void TSX(ushort addr)
    {
        X = S;
        CheckFlagN(X);
        CheckFlagZ(X);
    }

    private void TXA(ushort addr)
    {
        A = X;
        CheckFlagN(A);
        CheckFlagZ(A);
    }

    private void TXS(ushort addr)
    {
        S = X;
    }

    private void TYA(ushort addr)
    {
        A = Y;
        CheckFlagN(A);
        CheckFlagZ(A);
    }

    private void XAA(ushort addr)
    {
        throw new NesException("Unofficial instruction");
    }
}

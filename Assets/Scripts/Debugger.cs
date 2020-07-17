using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Debugger
{
    private char[] buffer = new char[256];
    private Bus bus;
    private CPU cpu;

    public Debugger(Bus bus, CPU cpu)
    {
        this.bus = bus;
        this.cpu = cpu;
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = ' ';
        }
    }

    public void Print()
    {
        Debug.Log(new string(buffer).TrimEnd());
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = ' ';
        }
    }

    public void BeginTickCPU()
    {
        WriteToBuffer(cpu.PC, 0);
        var opcode = cpu.opcodes[bus.Read8(cpu.PC)];
        int len = GetOprandLength(opcode);
        WriteToBuffer(bus.Read8(cpu.PC), 6);
        if (len > 0)
        {
            WriteToBuffer(bus.Read8((ushort)(cpu.PC + 1)), 9);
        }
        if (len > 1)
        {
            WriteToBuffer(bus.Read8((ushort)(cpu.PC + 2)), 12);
        }
        WriteToBuffer(opcode.name, 16);
        WriteOprand(opcode);
    }

    public void EndTickCPU()
    {
        WriteToBuffer("A:", cpu.A, 48);
        WriteToBuffer("X:", cpu.X, 53);
        WriteToBuffer("Y:", cpu.Y, 58);
        WriteToBuffer("P:", (byte)cpu.P, 63);
        WriteToBuffer("SP:", cpu.S, 68);
        //WriteToBuffer("CYC:", cpu.cycles, 86);
    }

    private int GetOprandLength(CPU.Opcode opcode)
    {
        switch (opcode.addrMode)
        {
            case CPU.AddrMode.Imp:
            //case CPU.AddrMode.Acc:
                return 0;

            case CPU.AddrMode.Abs:
            case CPU.AddrMode.Abx:
            case CPU.AddrMode.Aby:
            case CPU.AddrMode.Ind:
                return 2;

            default:
                return 1;
        }
    }

    // 6502汇编中，"#"表示立即数，"$"表示十六进制
    private void WriteOprand(CPU.Opcode opcode)
    {
        string oprand = "";
        int len = GetOprandLength(opcode);
        if (len == 1)
        {
            oprand = Hex(bus.Read8((ushort)(cpu.PC + 1)));
        }
        else if(len == 2)
        {
            oprand = Hex(bus.Read8((ushort)(cpu.PC + 2))) + Hex(bus.Read8((ushort)(cpu.PC + 1)));
        }

        switch (opcode.name)
        {
            case "STX":
                WriteToBuffer(string.Format("${0} = {1}", oprand, Hex(cpu.X)), 20);
                break;
        }

        switch (opcode.addrMode)
        {
            case CPU.AddrMode.Imm:
                WriteToBuffer(string.Format("#${0}", oprand), 20);
                break;

            case CPU.AddrMode.Zpg:
                break;

            case CPU.AddrMode.Zpx:
                break;

            case CPU.AddrMode.Zpy:
                break;

            case CPU.AddrMode.Izx:
                break;

            case CPU.AddrMode.Izy:
                break;

            case CPU.AddrMode.Abs:
                WriteToBuffer(string.Format("${0}", oprand), 20);
                break;

            case CPU.AddrMode.Abx:
                break;

            case CPU.AddrMode.Aby:
                break;

            case CPU.AddrMode.Ind:
                break;

            case CPU.AddrMode.Rel:
                break;
        }
    }

    private string Hex(long v)
    {
        return Convert.ToString(v, 16).ToUpper();
    }

    private void WriteToBuffer(string s, long pos)
    {
        for (int i = 0; i < s.Length; i++)
        {
            buffer[pos + i] = s[i];
        }
    }

    private void WriteToBuffer(long v, int pos)
    {
        string vs = Convert.ToString(v, 16).ToUpper();
        if (vs.Length == 1 || vs.Length == 3)
        {
            vs = "0" + vs;
        }
        for (int i = 0; i < vs.Length; i++)
        {
            buffer[pos + i] = vs[i];
        }
    }

    private void WriteToBuffer(string s, long v, int pos)
    {
        for (int i = 0; i < s.Length; i++)
        {
            buffer[pos + i] = s[i];
        }
        WriteToBuffer(v, pos + s.Length);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mapper0 : IMapper
{
    public byte[] PRG; // 程序只读储存器
    public byte[] CHR; // 角色只读储存器
    private bool isPRG16KB;

    public Mapper0(byte[] PRG, byte[] CHR)
    {
        this.PRG = PRG;
        this.CHR = CHR;
        // mappper0支持32K的PRG，如果PRG只有16K，则镜像
        isPRG16KB = (PRG.Length == 16 * 1024);
    }

    public byte Read(ushort address)
    {
        // CHR 0x0000 - 0x2000
        // PRG 0x8000 - 0xFFFF
        if (address < 0x2000)
        {
            return CHR[address];
        }
        if (address >= 0x8000)
        {
            address -= 0x8000;
            if (isPRG16KB)
            {
                address &= 0xBFFF;
            }
            return PRG[address];
        }
        throw new NesException(string.Format("Mapper0 Read address error: {0}", address));
    }

    public void Write(ushort address, byte value)
    {
        // CHR 0x0000 - 0x2000
        // PRG 0x8000 - 0xFFFF
        if (address < 0x2000)
        {
            CHR[address] = value;
            return;
        }
        if (address >= 0x8000)
        {
            address -= 0x8000;
            if (isPRG16KB)
            {
                address &= 0xBFFF;
            }
            PRG[address] = value;
            return;
        }
        throw new NesException(string.Format("Mapper0 Write address error: {0}", address));
    }
}

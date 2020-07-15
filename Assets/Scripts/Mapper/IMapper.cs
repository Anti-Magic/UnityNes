using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMapper
{
    byte Read(ushort address);
    void Write(ushort address, byte value);
}

using UnityEngine;
using System.Collections;
using System;

public class NesException : Exception
{
    public NesException(string message) : base(message)
    {
    }
}

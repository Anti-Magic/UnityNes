using UnityEngine;
using System.Collections;
using System.IO;

public class Main : MonoBehaviour
{
    private Emulator emulator;

    void Start()
    {
        string nesFilePath = "F:/nestest.nes";
        byte[] nesFileData = File.ReadAllBytes(nesFilePath);
        emulator = new Emulator(nesFileData);

        emulator.Tick(Time.deltaTime);
    }

    void Update()
    {
        
    }
}

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

        //for (int i = 0; i < 8991; i++)
        for (int i = 0; i < 900; i++)
        {
            emulator.Tick(Time.deltaTime);
        }
        emulator.debugger.Save("F:/nestest.log2.txt");
        Compare();
    }

    void Compare()
    {
        var a = File.ReadAllLines("F:/nestest.log.txt");
        var b = File.ReadAllLines("F:/nestest.log2.txt");
        for (int i = 0; i < b.Length; i++)
        {
            var linea = a[i];
            var lineb = b[i];
            if (linea != lineb)
            {
                Debug.LogError(i + 1);
                break;
            }
        }
        Debug.Log("finish");
    }

    void Update()
    {
        
    }
}

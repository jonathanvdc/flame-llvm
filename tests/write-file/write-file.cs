using System;
using System.IO;
using System.Runtime.InteropServices;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("error: expected one argument, got " + args.Length + ".");
            return 1;
        }

        var testString = "hello, world!";
        using (var fs = new FileStream(args[0], FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            WriteBytes(fs, testString);
            fs.Flush();
            fs.Seek(0, SeekOrigin.Begin);
            var outputStr = ReadToEndAsASCII(fs);
            if (outputStr == testString)
            {
                Console.WriteLine("Write/read successful");
            }
        }
        return 0;
    }

    private static void WriteBytes(Stream stream, string data)
    {
        // TODO: handle UTF-8 properly
        var bytes = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            bytes[i] = (byte)data[i];
        }
        stream.Write(bytes, 0, bytes.Length);
    }

    private static string ReadToEndAsASCII(Stream stream)
    {
        var bytes = new byte[(int)stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        var chars = new char[bytes.Length];
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = (char)bytes[i];
        }
        return new String(chars);
    }
}
using System;
using System.Text;

namespace Otus.Server.ConsoleApp;

public static class SimpleStoreCommands
{
    public static byte[] GetCommand = Encoding.UTF8.GetBytes("GET");
    public static byte[] SetCommand = Encoding.UTF8.GetBytes("SET");
    public static byte[] DeleteCommand = Encoding.UTF8.GetBytes("DELETE");
}

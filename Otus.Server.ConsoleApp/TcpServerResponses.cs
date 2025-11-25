using System;
using System.Text;

namespace Otus.Server.ConsoleApp;

public static class TcpServerResponses
{
    public static byte[] Ok = Encoding.UTF8.GetBytes("OK\r\n");
    public static byte[] Nil = Encoding.UTF8.GetBytes("(nil)\r\n");
    public static byte[] UnknownCommand = Encoding.UTF8.GetBytes("-ERR Unknown command\r\n");
    public static byte[] EmptyKey = Encoding.UTF8.GetBytes("-ERR Key is Empty\r\n");
}

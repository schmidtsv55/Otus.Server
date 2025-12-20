using System;
using System.Text;

namespace Otus.Server.ConsoleApp;

public static class TcpServerResponses
{
    public static readonly byte[] Ok = Encoding.UTF8.GetBytes($"{OkTitle}\r\n");
    public static readonly byte[] Nil = Encoding.UTF8.GetBytes($"{NilTitle}\r\n");
    public static readonly byte[] UnknownCommand = Encoding.UTF8.GetBytes($"{UnknownCommandTitle}\r\n");
    public static readonly byte[] EmptyKey = Encoding.UTF8.GetBytes($"{EmptyKeyTitle}\r\n");

    public const string OkTitle = "OK";
    public const string NilTitle = "(nil)";
    public const string UnknownCommandTitle = "-ERR Unknown command";
    public const string EmptyKeyTitle = "-ERR Key is Empty";
}

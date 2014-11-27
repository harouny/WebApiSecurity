using System;
using System.Net.Http;

namespace WebApiSecurity.Tests
{
    public static class LogUtility
    {
        public static void LogToConsole(this HttpResponseMessage response)
        {
            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);
        }
    }
}

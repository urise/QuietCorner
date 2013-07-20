using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonClasses.Helpers
{
    public static class RandomHelper
    {
        private static readonly Random _random = new Random();

        public static string GetRandomString(int size)
        {
            const string chars = "ABCDEFGHIJKLMNOPRSTUVWXYZabcdefghijklmnoprstuvwxyz0123456789";
            
            var buffer = new char[size];
            for (int i = 0; i < size; i++)
                buffer[i] = chars[_random.Next(chars.Length)];
            return new string(buffer);
        }
    }
}

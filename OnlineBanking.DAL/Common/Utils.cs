using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineBanking.DAL
{
    public static class Utils
    {
        private static Random random = new Random();

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> data)
        {
            return data == null || !data.Any();
        }

        public static bool IsNullOrEmpty(string data)
        {
            return string.IsNullOrEmpty(data);
        }

        public static bool IsNullOrEmpty<T>(T data)
        {
            return data == null;
        }

        public static bool NotNullOrEmpty<T>(T data)
        {
            return !IsNullOrEmpty(data);
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        private static string GetRandomSalt()
        {
            return BCrypt.Net.BCrypt.GenerateSalt(12);
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, GetRandomSalt());
        }

        public static bool ValidatePassword(string password, string correctHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, correctHash);
            }
            catch (Exception)
            {
                return false;
            }
            
        }
    }
}
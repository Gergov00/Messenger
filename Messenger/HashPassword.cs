using System;
using System.Security.Cryptography;
using System.Text;

namespace Messenger.Data
{
    public static class HashPassword
    {
        private const string key = "akls;djfklhj";

        public static string HashedPassword(string password)
        {
            // Преобразуем пароль и ключ в байты
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Используем HMACSHA256 для хеширования
            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(passwordBytes);

                // Преобразуем хэш в строку в формате Base64
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}

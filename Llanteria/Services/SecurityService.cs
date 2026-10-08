using System;
using System.Security.Cryptography;
using System.Text;

namespace Llanteria.Services
{
    public class SecurityService
    {
        public static string GenerarPasswordAleatorio(string passwordPlano)
        {
            string passHash = ComputeSha256(passwordPlano);
            string randomStr = Guid.NewGuid().ToString("N");
            Random rnd = new Random();
            int index = rnd.Next(0, randomStr.Length - passHash.Length);
            string combined = randomStr.Insert(index, passHash);
            return $"{index}:{combined}";
        }

        public static bool VerificarPassword(string passwordPlano, string storedData)
        {
            try
            {
                var parts = storedData.Split(':');
                if (parts.Length < 2) return false;

                int index = int.Parse(parts[0]);
                string combined = parts[1];
                string passHash = ComputeSha256(passwordPlano);

                if (index < 0 || index + passHash.Length > combined.Length) return false;

                string extractedHash = combined.Substring(index, passHash.Length);
                return passHash == extractedHash;
            }
            catch
            {
                return false;
            }
        }

        private static string ComputeSha256(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
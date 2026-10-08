using System;
using System.Linq;

namespace Llanteria.Services
{
    public class CredentialGenerationService
    {
        private readonly UsuarioService _userSer;

        public CredentialGenerationService(UsuarioService userSer)
        {
            _userSer = userSer;
        }

        public string GenerarUsernameUnico(int minLength = 10, int maxLength = 25)
        {
            // PROTECCIÓN: Evita el error si los parámetros se invierten por accidente
            if (minLength > maxLength)
            {
                int temp = minLength;
                minLength = maxLength;
                maxLength = temp;
            }

            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string nums = "0123456789";
            const string specs = "_-.";

            var existingUsernames = _userSer.GetUsuarios().Select(u => u.Username).ToHashSet();
            Random rnd = new Random();
            int attempts = 0;
            string candidate;

            do
            {
                int length = rnd.Next(minLength, maxLength + 1);
                char[] chars = new char[length];
                chars[0] = upper[rnd.Next(upper.Length)];
                chars[1] = nums[rnd.Next(nums.Length)];
                chars[2] = specs[rnd.Next(specs.Length)];

                string pool = upper + lower + nums + specs;
                for (int i = 3; i < length; i++)
                {
                    chars[i] = pool[rnd.Next(pool.Length)];
                }

                candidate = new string(chars.OrderBy(x => rnd.Next()).ToArray());
                attempts++;

                if (attempts > 500)
                {
                    candidate = $"User_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    break;
                }
            } while (existingUsernames.Contains(candidate));

            return candidate;
        }
    }
}
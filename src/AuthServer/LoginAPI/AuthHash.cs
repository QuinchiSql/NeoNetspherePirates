using System.Security.Cryptography;
using System.Text;

namespace NeoNetsphere.LoginAPI
{
    internal class AuthHash
    {
        public static string GetHash256(string data)
        {
            var hashResult = string.Empty;

            if (data != null)
                using (var sha256 = SHA256.Create())
                {
                    var dataBuffer = Encoding.UTF8.GetBytes(data);
                    var dataBufferHashed = sha256.ComputeHash(dataBuffer);
                    hashResult = GetHashString(dataBufferHashed);
                }
            return hashResult;
        }

        private static string GetHashString(byte[] dataBufferHashed)
        {
            var sb = new StringBuilder();
            foreach (var b in dataBufferHashed)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }
    }
}

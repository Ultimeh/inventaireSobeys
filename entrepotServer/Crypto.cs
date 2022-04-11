using System.Security.Cryptography;

namespace entrepotServer
{
    class Crypto
    {
        public string Encrypt(string inputString)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                byte[] data = System.Text.Encoding.ASCII.GetBytes(inputString);
                data = mySHA256.ComputeHash(data);
                return PrintByteArray(data);
            }
        }

        // Display the byte array in a readable format.
        private string PrintByteArray(byte[] array)
        {
            string hash = "";
            for (int i = 0; i < array.Length; i++)
            {
                hash = hash + ($"{array[i]:X2}");
            }
            return hash;
        }
    }
}

using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace DETicketReader
{
    public class Iso9796_2Decoder
    {
        private RSA m_rsa;

        public Iso9796_2Decoder()
        {
            m_rsa = RSA.Create();
        }

        public void SetRsaParameters(byte[] modulus, ushort modulusSize, byte[] exponent, ushort exponentSize)
        {
            RSAParameters rsaParameters = new RSAParameters
            {
                Modulus = modulus,
                Exponent = exponent
            };
            m_rsa.ImportParameters(rsaParameters);
        }

        public void AddWithRecoveredMessage(byte[] data, int size)
        {
            byte[] outData = new byte[m_rsa.KeySize / 8];
            int outSize = m_rsa.Decrypt(data, outData, RSAEncryptionPadding.Pkcs1);

            if (outSize < 0)
            {
                Console.WriteLine($"RSA error: {RSAErrorString()}");
                return;
            }

            byte[] trimmedData = new byte[outSize];
            Array.Copy(outData, trimmedData, outSize);

            if (trimmedData[0] != 0x6a || trimmedData[trimmedData.Length - 1] != 0xbc || trimmedData.Length < 22)
            {
                Console.WriteLine($"RSA message recovery failed: {BitConverter.ToString(trimmedData)} {outSize}");
                return;
            }

            m_recoveredMsg.AddRange(trimmedData[1..^21]);
        }

        public void Add(byte[] data, int size)
        {
            if (m_recoveredMsg.Count == 0)
            {
                return;
            }
            m_recoveredMsg.AddRange(data);
        }

        public byte[] RecoveredMessage => m_recoveredMsg.ToArray();

        private List<byte> m_recoveredMsg = new List<byte>();

        private string RSAErrorString()
        {
            return "An error occurred during RSA decryption."; // Replace with appropriate error handling.
        }
    }
}

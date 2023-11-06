using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DETicketReader.Models;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.X509;
using static System.Runtime.InteropServices.JavaScript.JSType;
using PemReader = Org.BouncyCastle.OpenSsl.PemReader;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace DETicketReader
{
    public class VDVDataDecryptor
    {
        private string sigM;
        private byte[] decryptedData;
        private string fileNames;
        string caCertificateFilePath;

        
        

        void DecryptData(VDVSignedTicket ticket)
        {
            try
            {
                if (ticket == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ticket object is null");
                    Console.ResetColor();
                    return;
                }

                if (ticket.Tag42ValueData == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No CAR TAG in the ticket");
                    Console.ResetColor();
                    return;
                }
                string folderPath = @"C:\SKIDATA\Bin\Extensions\CCS\BZB\certificates\Keys\";
                string keyFileName = BitConverter.ToString(ticket.Tag42ValueData).Replace("-", "") + ".vdv-cert";
                caCertificateFilePath = Path.Combine(folderPath, keyFileName);

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(folderPath + keyFileName);
                Console.ResetColor();

                //Geting Raw CA certificate data
                Console.WriteLine($"CA Certificate Raw Data: ");
                byte[] caCertificateBytes = LoadCertificate(caCertificateFilePath);
                foreach (byte b in caCertificateBytes)
                {
                    Console.Write($"{b:X2} ");
                }

                

                // Assuming it's an RSA public key
                

                VdvCertificate vdvCertificate = new VdvCertificate(caCertificateBytes);

                Console.WriteLine(vdvCertificate.GetModulus());
                Console.WriteLine(vdvCertificate.GetExponent());

                RsaKeyParameters rsaKeyParameters = new RsaKeyParameters(
                    false,
                    vdvCertificate.GetModulus(),
                    vdvCertificate.GetExponent()
                );

                // Create the signer with ISO9796-2 scheme
                var signer = new Iso9796d2Signer(new RsaEngine(), new Sha1Digest(), true);

                signer.Init(false, rsaKeyParameters);

                signer.UpdateWithRecoveredMessage(ticket.Tag7F21ValueData);
                byte[] recoveredMessage = signer.GetRecoveredMessage();

                Console.WriteLine("Recovered message: " + System.Text.Encoding.UTF8.GetString(recoveredMessage));

            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message );
            }

        }




        public byte[] GetDecryptedData(VDVSignedTicket vdvTicket)
        {
            DecryptData(vdvTicket);
            return decryptedData;
        }

        public byte[] LoadCertificate(string pemFilePath)
        {
            using (var reader = File.OpenRead(pemFilePath))
            {
                byte[] rawData = new byte[reader.Length];
                reader.Read(rawData, 0, (int)reader.Length);
                return rawData;
            }
        }

        static byte[] EncryptData(AsymmetricKeyParameter publicKey, byte[] data)
        {
            byte[] returnData = new byte[0];

            try
            {
                // Initialize cipher engine and encrypt the data
                var encryptEngine = new Pkcs1Encoding(new RsaEngine());
                encryptEngine.Init(true, publicKey);

                returnData = encryptEngine.ProcessBlock(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(ex.ToString());
                Console.ResetColor();
            }
            return returnData;
        }

    }
}

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
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
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

                byte[] caCertBytes = caCertificateBytes;
                byte[] cvCertBytes = ticket.Tag7F21ValueData.ToArray();

                RsaKeyParameters publicKey = (RsaKeyParameters)PublicKeyFactory.CreateKey(caCertificateBytes);


                ISigner signer = SignerUtilities.GetSigner("ISO9796-2");

                // Initialize the signer for verification (to recover the message)
                signer.Init(false, publicKey);

                signer.BlockUpdate(cvCertBytes, 0, cvCertBytes.Length);


                // Verify the signature and try to recover the message
                byte[] recoveredMessage;
   
                if (signer.VerifySignature(cvCertBytes))
                {
                    // Recover the message from the signature
                    recoveredMessage = signer.GenerateSignature();
                    // Continue processing with the recovered message...
                }
                else
                {
                    Console.WriteLine("Failed to verify the signature.");
                }

                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
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

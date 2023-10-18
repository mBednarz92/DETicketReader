using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DETicketReader.Models;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.X509;
using PemReader = Org.BouncyCastle.OpenSsl.PemReader;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace DETicketReader
{
    public class VDVDataDecryptor
    {
        private string sigM;
        private byte[] decryptedData;
        private string [] fileNames;

        ResourceManager rm = new ResourceManager("DETicketReader.Resource1", Assembly.GetExecutingAssembly());
        X509CertificateParser certificateParser = new X509CertificateParser();

        void DecryptData(VDVSignedTicket ticket)
        {
            try
            {
                string folderPath = @"C:\SKIDATA\Bin\Extensions\CCS\BZB\certificates\Keys";
                fileNames = Directory.GetFiles(folderPath);

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Certificates files in folder: ");
                Console.ResetColor();
                foreach (string fileName in fileNames)
                {
                    Console.WriteLine(Path.GetFileName(fileName));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }

            int currentItteration = 0;

            while (decryptedData == null && currentItteration <= fileNames.Length - 1)
            {
                
                Console.WriteLine($"Current iteration: {currentItteration+1}");
                byte[] caCertificateBytes = LoadCertificate($"{fileNames[currentItteration]}");
                foreach (byte b in caCertificateBytes)
                {
                    Console.Write($"{b:X2} ");
                }

                X509Certificate caCertificate = certificateParser.ReadCertificate(caCertificateBytes);
                Console.WriteLine(caCertificate.SubjectDN);
                AsymmetricKeyParameter publicKey =  caCertificate.GetPublicKey();
                X509Certificate cvCertificate = certificateParser.ReadCertificate(ticket.Tag7F21ValueData);
                
                byte[] recoveredMessage;


                if (publicKey != null)
                {
                    try
                    {
                  
                            cvCertificate.Verify(publicKey);

                        ISigner verifier = SignerUtilities.GetSigner("ISO9796-2");

                        verifier.Init(false, cvCertificate.GetPublicKey());

                        verifier.BlockUpdate(ticket.Tag9AValueData, 0, ticket.Tag9AValueData.Length);
                        recoveredMessage = verifier.GenerateSignature();

                        // Display the hexadecimal string
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("recoveredMessage:");
                        Console.ResetColor();
                        Console.WriteLine(recoveredMessage);


                    }
                    catch (CryptographicException cryptoEx)
                    {
                        Console.WriteLine($"Decryption failed. Details: {cryptoEx.Message}");
                        return;
                    }
                    catch (NotSupportedException notSuppEx)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Operation not supported. Details: {notSuppEx.Message}");
                        Console.ResetColor();
                        return;
                    }

                    // Output decrypted data
                    if(decryptedData != null)
                    {
                        string decryptedText = Encoding.UTF8.GetString(decryptedData);
                        Console.WriteLine($"Decrypted Text: {decryptedText}");
                    }   
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"No Public Key found for: {fileNames[currentItteration]}.pem");
                    Console.ResetColor();
                }
                currentItteration++;
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

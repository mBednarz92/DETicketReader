using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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

namespace DETicketReader
{
    public class RSADecryptor
    {
        private string sigM;
        private byte[] decryptedData;
        private string [] fileNames;

        ResourceManager rm = new ResourceManager("DETicketReader.Resource1", Assembly.GetExecutingAssembly());
        

        void DecryptData(byte[] sigM)
        {
            try
            {
                string folderPath = @"C:\SKIDATA\Bin\Extensions\CCS\BZB\certificates";
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
                AsymmetricKeyParameter publicKey = LoadPublicKey($"{fileNames[currentItteration]}");

                if (publicKey != null)
                {
                    try
                    {

                        // Encrypt the data
                        byte[] encryptedData = EncryptData(publicKey, sigM);

                        if(encryptedData.Length > 0)
                        {
                            // Example: Convert encrypted data to Base64 and print
                            string base64EncryptedData = Convert.ToBase64String(encryptedData);

                            byte[] byteData = Convert.FromBase64String(base64EncryptedData);

                            // Convert the byte array to a hexadecimal string
                            string hexString = BitConverter.ToString(byteData).Replace("-", string.Empty);

                            // Display the hexadecimal string
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("Encrypted data in Base64:");
                            Console.WriteLine();
                            Console.WriteLine(base64EncryptedData);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine();
                            Console.WriteLine("Encrypted data in Hexadecimal:");
                            Console.WriteLine();
                            Console.WriteLine(hexString);
                            Console.ResetColor();
                        } 
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Wrong public key");
                            Console.ResetColor();
                        }

                        
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

        public byte[] GetDecryptedData(byte[] sigM)
        {
            DecryptData(sigM);
            return decryptedData;
        }

        static AsymmetricKeyParameter LoadPublicKey(string pemFilePath)
        {
            using (var reader = File.OpenText(pemFilePath))
            {
                var pemReader = new PemReader(reader);
                var certificate = (Org.BouncyCastle.X509.X509Certificate)pemReader.ReadObject();
                if (certificate == null)
                {
                    throw new InvalidOperationException("Could not read the certificate from the PEM file. Ensure the PEM file is a valid certificate.");
                }
                else
                {
                    Console.WriteLine(certificate.ToString());
                }
                return certificate.GetPublicKey();
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

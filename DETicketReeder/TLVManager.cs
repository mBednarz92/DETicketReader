using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace DETicketReader
{
    public class TLVManager
    {
        byte[] tlvData;

        public TLVManager()
        {
            ResourceManager rm = new ResourceManager("DETicketReader.Resource1", Assembly.GetExecutingAssembly());

            string ticketHexString = rm.GetString("InputData", CultureInfo.CurrentCulture);
            string[] ticketHexValues = ticketHexString.Split(new char[] { ' ' });

            tlvData = new byte[ticketHexValues.Length];
            for (int i = 0; i < ticketHexValues.LongLength; i++)
            {
                tlvData[i] = Convert.ToByte(ticketHexValues[i], 16);
            }

        }
        public void ShowRawData()
        {
            string[] hexString = BitConverter.ToString(tlvData).Split('-');

            foreach (string b in hexString)
            {
                Console.Write(" " + b);
            }


        }

        public void ShowOrganizedData()
        {
            using (MemoryStream stream = new MemoryStream(tlvData))
            {
                while (stream.Position < stream.Length)
                {
                    try
                    {
                        // Read the Tag
                        byte tag = (byte)stream.ReadByte();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Tag: {tag:X2}");

                        if (stream.Position == stream.Length)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Error: Missing length and value after tag.");
                            Console.ResetColor();
                            break;
                        }

                        // Read the Length
                        byte lengthIndicator = (byte)stream.ReadByte();
                        int length = lengthIndicator;
                        if (lengthIndicator == 0x81)
                        {
                            if (stream.Position == stream.Length)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Error: Missing extended length byte.");
                                Console.ResetColor();
                                break;
                            }
                            length = stream.ReadByte();
                        }
                        else if (lengthIndicator == 0x82)
                        {
                            if (stream.Length - stream.Position < 2)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Error: Missing some extended length bytes.");
                                Console.ResetColor();
                                break;
                            }
                            length = (stream.ReadByte() << 8) + stream.ReadByte();
                        }

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"Length: {length}");

                        if (stream.Length - stream.Position < length)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Error: Not enough bytes remain in the stream for the declared length of {length} bytes.");
                            var remaining = new byte[stream.Length - stream.Position];
                            stream.Read(remaining, 0, remaining.Length);
                            Console.WriteLine($"Remaining data (possibly corrupt or incomplete): {BitConverter.ToString(remaining).Replace("-", " ")}");
                            Console.ResetColor();
                            break;
                        }

                        // Read the Value
                        byte[] value = new byte[length];
                        stream.Read(value, 0, length);
                        Console.ResetColor();
                        Console.WriteLine($"Value: {BitConverter.ToString(value).Replace("-", " ")}");
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: {ex.Message}");
                        Console.ResetColor();
                        break;
                    }
                }
            }
        }
    }
}
                 


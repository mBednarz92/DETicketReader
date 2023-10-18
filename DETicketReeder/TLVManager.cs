using DETicketReader.Models;
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
        public byte[] tlvData;
        public List<VDVSignedTicket> vdvSignedTicketsArray = new List<VDVSignedTicket>();

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

            VDVSignedTicket tempVDVTicket = new VDVSignedTicket()
            {
                Tag9EValueData = new byte[0],
                Tag9AValueData = new byte[0],
                Tag7F21ValueData = new byte[0]  
            };

            using (MemoryStream stream = new MemoryStream(tlvData))
                while (stream.Position < stream.Length)
                {
                    try
                    {
                        // Tag
                        ushort tag;  // using ushort to accommodate 2-byte tags

                        byte tag1 = (byte)stream.ReadByte();

                        if (tag1 == 0x7F && stream.Position < stream.Length && (byte)stream.ReadByte() == 0x21)
                        {
                            tag = 0x7F21; // Combined two-byte tag

                        }
                        else
                        {
                            tag = tag1;  // Single-byte tag
                        }

                        if (stream.Position == stream.Length)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Error: Missing length and value after tag.");
                            Console.ResetColor();
                            break;
                        }

                        // Length
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

                        // Value
                        byte[] value = new byte[length];
                        stream.Read(value, 0, length);
                        if (tag == 0x9e)
                        {
                            tempVDVTicket.Tag9EValueData = value;
                            Console.WriteLine("Tag 9E Value (Signature) Imported");
                        }
                        else if (tag == 0x9a)
                        {
                            tempVDVTicket.Tag9AValueData = value;
                            Console.WriteLine("Tag 9A Value (SignatureRemainder) Imported");
                        }
                        else if (tag == 0x7F21)
                        {
                            tempVDVTicket.Tag7F21ValueData = value;
                            Console.WriteLine("Tag 7F21 Value (CV Certificate) Imported");
                        }

                        if (tempVDVTicket.Tag9AValueData.Length > 0 && tempVDVTicket.Tag9EValueData.Length > 0 && tempVDVTicket.Tag7F21ValueData.Length > 0)
                        {
                            vdvSignedTicketsArray.Add(tempVDVTicket);
                            Console.WriteLine("Ticket object created");
                            tempVDVTicket = new VDVSignedTicket()
                            {
                                Tag9EValueData = new byte[0],
                                Tag9AValueData = new byte[0],
                                Tag7F21ValueData = new byte[0]                                
                            };
                        }


                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: {ex.Message} {ex.Source}");
                        Console.ResetColor();
                        break;
                    }
                }
        }



        public void ShowRawData()
        {
            string[] hexString = BitConverter.ToString(tlvData).Split('-');
            int currentColumn = 0;

            foreach (string b in hexString)
            {
                if (currentColumn == 16)
                {
                    currentColumn = 0;
                    Console.WriteLine("");
                }
                currentColumn++;
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
                        ushort tag;  // using ushort to accommodate 2-byte tags

                        byte tag1 = (byte)stream.ReadByte();

                        if (tag1 == 0x7F && stream.Position < stream.Length && (byte)stream.ReadByte() == 0x21)
                        {
                            tag = 0x7F21; // Combined two-byte tag

                        }
                        else
                        {
                            tag = tag1;  // Single-byte tag
                        }

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Tag: {tag:X4}"); // Adjusted for possible 4 character output

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
                        Console.WriteLine("Value: ");
                        PrintBytesInRows(value);
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

        private static void PrintBytesInRows(byte[] bytes)
        {
            int bytesPerRow = 16; // max number of bytes to print in a row

            for (int i = 0; i < bytes.Length; i++)
            {
                // Print byte in hexadecimal format
                Console.Write($"{bytes[i]:X2} ");

                // Insert newline after every bytesPerRow bytes, or at end of byte array
                if ((i + 1) % bytesPerRow == 0 || i == bytes.Length - 1)
                {
                    Console.WriteLine();
                }
            }
        }
    }
}

                     


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DETicketReader.Models
{
    public class VDVSignedTicket
    {
        public byte[]? Tag9EValueData { get; set; }
        public byte[]? Tag9AValueData { get; set; }
        public byte[]? Tag7F21ValueData { get; set; }
        public byte[]? Tag42ValueData { get; set; }
    }

    
}

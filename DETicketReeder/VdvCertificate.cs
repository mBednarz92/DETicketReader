using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DETicketReader
{
    public class VdvCertificate
    {
        private readonly byte[] _certificateData;

        public VdvCertificate(byte[] certificateData)
        {
            _certificateData = certificateData;
        }

        public BigInteger GetModulus()
        {
            // Assuming oidSize gives the size of the OID part
            int oidSize = GetOidSize();
            // Assuming the modulus starts right after the OID
            int modulusStart = GetOidStart() + oidSize;
            int modulusSize = GetModulusSize();
            byte[] modulusBytes = new byte[modulusSize];
            Array.Copy(_certificateData, modulusStart, modulusBytes, 0, modulusSize);
            return new BigInteger(1, modulusBytes);
        }

        public BigInteger GetExponent()
        {
            // Assuming the exponent is 4 bytes and comes right after the modulus
            int exponentSize = 4; // Fixed size for exponent
            int modulusSize = GetModulusSize();
            int exponentStart = GetOidStart() + GetOidSize() + modulusSize;
            byte[] exponentBytes = new byte[exponentSize];
            Array.Copy(_certificateData, exponentStart, exponentBytes, 0, exponentSize);
            return new BigInteger(1, exponentBytes);
        }

        private int GetOidStart()
        {
            // You need to determine where the OID starts in your certificate data
            // This is just a placeholder value
            return 0;
        }

        private int GetOidSize()
        {
            // You need to determine the size of the OID in your certificate data
            // This is just a placeholder value
            return 7;
        }

        private int GetModulusSize()
        {
           
            return 1024;
        }
    }
}

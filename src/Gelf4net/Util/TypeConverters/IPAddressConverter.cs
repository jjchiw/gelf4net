using log4net.Util.TypeConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace gelf4net.Util.TypeConverters
{
    /// <summary>
    /// The built-in IPAddressConverter in log4net does not perform well and errors out unecessarily when parsing external IP address.
    /// This converter tries to parse the IP first and doesn't care if it actually exists or has a valid host entry.
    /// </summary>
    internal class IPAddressConverter : IConvertFrom
    {
        public bool CanConvertFrom(Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public object ConvertFrom(object source)
        {
            string hostNameOrAddress = source as string;
            if (hostNameOrAddress != null && hostNameOrAddress.Length > 0)
            {
                try
                {
                    IPAddress address = null;

                    if (!IPAddress.TryParse(hostNameOrAddress, out address))
                    {
                        IPHostEntry hostEntry = Dns.GetHostEntry(hostNameOrAddress);
                        if (hostEntry != null && hostEntry.AddressList != null && hostEntry.AddressList.Length > 0 && hostEntry.AddressList[0] != null)
                            address = hostEntry.AddressList[0];
                    }

                    return address;
                }
                catch (Exception ex)
                {
                    throw ConversionNotSupportedException.Create(typeof(IPAddress), source, ex);
                }
            }
            throw ConversionNotSupportedException.Create(typeof(IPAddress), source);
        }
    }
}

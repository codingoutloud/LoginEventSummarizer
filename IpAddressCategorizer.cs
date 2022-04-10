// what kind of IP address is this?

using System;
using System.Net;
using System.Text.RegularExpressions;

namespace IpAddressCategorizer
{
   public static class Routing
   {
      public static bool IsPrivate(string ip)
      {
         if (Version.IsV4(ip))
            return RoutingV4.IsPrivate(ip);
         else
            return RoutingV6.IsPrivate(ip);
      }
   }

   public static class RoutingV6
   {
      static string NetLocalHost = "::1";

      public static bool IsPrivate(string ip)
      {
         return (ip == NetLocalHost); // TODO: implement the rest of IPv6
      }

      // https://en.wikipedia.org/wiki/Private_network#Private_IPv6_addresses
      // static string NetRangeULAfd00 ...
   }

   public static class RoutingV4
   {
#pragma warning disable CS0414
      // https://en.wikipedia.org/wiki/Reserved_IP_addresses
      static string NetLocalNetwork_prefix = "0.";
      static string NetLocalHost = "127.0.0.1";

      static string NetLinkLocalAddress_prefix = "169.254."; // reserved for Link-local addresses - 169.254.0.0/16
      static string NetRange10_prefix = "10."; // A single reserved class A address - 10.0.0.0/8
      static string NetRange192dot168_prefix = "192.168."; // 256 reserved class C addresses - 192.168.0.0/16

      // range: 100.64.0.0-100.127.255.255
      static string NetRange100_candidate_prefix = "100"; // reserved for carriers - 100.64.0.0/10

      static string NetRange172_candidate_prefix = "172."; // 16 reserved class B addresses

      public static bool IsPrivate(string ip)
      {
         return !IsPubliclyRoutable(ip);
      }

      public static bool IsPubliclyRoutable(string ip)
      {
         if (Version.IsV4(ip))
         {
            if (ip.StartsWith(NetRange10_prefix)) return false;
            if (ip.StartsWith(NetRange192dot168_prefix)) return false;
            if (ip.StartsWith(NetRange10_prefix)) return false;
         }

         return true; // TODO: implement the rest of IPv4
      }
   }

   public static class Version
   {
      // https://stackoverflow.com/a/46079861 ← a more thorough regex for valid IP values
      static Regex IpV4_pattern = new Regex(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$");

      public static bool IsV6(string ip)
      {
         if (IsV4(ip)) return false;
         return ip.Contains(':');  // TODO: implement the rest of IPv6
      }

      public static bool IsV4(string ip)
      {
         // assumes decimal notation in each octet; no support for hex or octal or any other notation
         if (!IpV4_pattern.IsMatch(ip)) return false;

         IPAddress? ipAddr; // = new IPAddress(0x0);
         var isValidIp = IPAddress.TryParse(ip, out ipAddr);
         if (isValidIp) // && ipAddr is not null)
         {
#if VERBOSE
            Console.Error.WriteLine($"{isValidIp} for {ipAddr!.AddressFamily}");
#endif
         }
         return isValidIp;
      }

      // consider an EnsureValidV4 method
   }
}

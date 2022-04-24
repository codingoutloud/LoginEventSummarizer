/// DTO for the Azure Map API that determines the country of an IP address. IPv4 only right now.

namespace EventDownloader
{
   public class CountryRegion
   {
      public string? IsoCode { get; set; }
   }

   public class AzureMapLocation
   {
      public CountryRegion? CountryRegion { get; set; }
      public string? IpAddress { get; set; }
   }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CSO.Firebase
{
    public class FirebaseServicesHelper
    {
        public static IConfiguration Configuration { get; set; }
        public static ServiceProvider ServiceProvider { get; set; }

        public static IFirebase Firebase;

   }
}
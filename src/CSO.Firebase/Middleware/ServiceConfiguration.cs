using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CSO.Firebase
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection AddFirebase(this IServiceCollection service, WebApplicationBuilder builder, dynamic LogObject)
        { 
            service.AddSingleton<IFirebase, Firebase>();

            var sp = service.BuildServiceProvider();

            FirebaseServicesHelper.ServiceProvider = sp;
            FirebaseServicesHelper.Configuration = builder.Configuration;

            FirebaseServicesHelper.Firebase = sp.GetRequiredService<IFirebase>();
      
            LogObject.Info("Firebase setup completed");
            return service;
        }

    }
}

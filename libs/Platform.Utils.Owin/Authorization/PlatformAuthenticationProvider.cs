using System;
using System.Linq;
using System.Threading.Tasks;

namespace Platform.Utils.Owin.Authorization
{
    using System.Configuration;
    using System.Security.Claims;
    using Microsoft.Owin.Security.OAuth;
    using Redis;
    using Storage;

    public class PlatformAuthenticationProvider : OAuthBearerAuthenticationProvider
    {
        private readonly IStorage<ClaimsPrincipal> principalStorage;

        //private readonly IStorage<ClaimsPrincipal> sharedStorage;

        public PlatformAuthenticationProvider()
        {
            this.principalStorage = new RedisStorage<ClaimsPrincipal>(
                  new RedisDatabase(
                      new RedisConnection(ConfigurationManager.ConnectionStrings["RedisServer"].ConnectionString),
                      int.Parse(ConfigurationManager.AppSettings["redisUserStorageDb"])),
                  TimeSpan.Parse(ConfigurationManager.AppSettings["redisUserStorageTimeout"]));

            //this.sharedStorage = new RedisStorage<ClaimsPrincipal>(
            //        new RedisDatabase(
            //            new RedisConnection(ConfigurationManager.ConnectionStrings["SharedRedis"].ConnectionString),
            //            int.Parse(ConfigurationManager.AppSettings["redisUserStorageDb"])),
            //        TimeSpan.Parse(ConfigurationManager.AppSettings["redisUserStorageTimeout"]));
        }
        
        public override Task ValidateIdentity(OAuthValidateIdentityContext context)
        {
            var identity = context.Ticket?.Identity;
            var masterIdClaim = identity?.Claims.FirstOrDefault(x => x.Type == "masterId");
            if (masterIdClaim == null)
            {
                context.Rejected();
            }
            else
            {
                var storedIdentity = this.principalStorage[masterIdClaim.Value];
                if (storedIdentity == null)
                {
                    context.Rejected();
                }
            }
            return base.ValidateIdentity(context);
        }
    }
}

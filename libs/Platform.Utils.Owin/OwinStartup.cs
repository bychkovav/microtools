using System;
using System.Linq;
using System.Linq.Expressions;

namespace Platform.Utils.Owin
{
    using System.Configuration;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.ExceptionHandling;
    using Authorization;
    using Filters;
    using global::Owin;
    using Ioc;
    using Microsoft.Owin.Cors;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Jwt;
    using SimpleInjector.Integration.WebApi;
    using Swashbuckle.Application;

    public class OwinStartup
    {
        private const string DefaultAudience = "PLATFORM_4B2AA9F0-77BD-4C8E-AA62-97F5E391E6D9";

        public virtual void Configuration(IAppBuilder app)
        {
            app.Use(typeof(RequestTracingMiddleware));
            app.UseCors(CorsOptions.AllowAll);
            app.UseRequestScopeContext();

            var resolver = new SimpleInjectorWebApiDependencyResolver(IocContainerProvider.CurrentContainer);
            GlobalConfiguration.Configuration.DependencyResolver = resolver;
            var httpConfig = new HttpConfiguration
            {
                DependencyResolver = resolver
            };

            ConfigureHttp(app, httpConfig);

            /*app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions()
            {
                AuthenticationMode = AuthenticationMode.Active,
                Provider = new PlatformAuthenticationProvider(),
                AllowedAudiences = new[] { DefaultAudience },
                IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[]
               {
                    new SymmetricKeyIssuerSecurityTokenProvider(ConfigurationManager.AppSettings["jwtIssuer"],
                        ConfigurationManager.AppSettings["jwtSecret"])
               }
            });*/

            app.UseWebApi(httpConfig);
        }

        protected virtual void ConfigureHttp(IAppBuilder app, HttpConfiguration httpConfig)
        {
            httpConfig.EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "MyAPI");
            }).EnableSwaggerUi();

            httpConfig.MapHttpAttributeRoutes();
            httpConfig.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            httpConfig.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            httpConfig.Services.Replace(typeof(IExceptionLogger), new PlatformExceptionLogger());
            httpConfig.Services.Replace(typeof(IAssembliesResolver), new PlatformAssembliesResolver());

            httpConfig.Formatters.Remove(httpConfig.Formatters.XmlFormatter);

            httpConfig.Filters.Add(new ExceptionFilterAttribute());

            foreach (var conf in AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(IOwinConfig)
               .IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract))
            {
                var appConfig = Expression.MemberInit(Expression.New(conf));
                var callingMethod = Expression.Call(appConfig, conf.GetMethod("Register"),
                    new Expression[]
                    {
                        Expression.Constant(app, typeof(IAppBuilder)),
                        Expression.Constant(httpConfig, typeof(HttpConfiguration))
                    });
                Expression.Lambda<Action>(callingMethod).Compile().Invoke();
            }
        }
    }
}

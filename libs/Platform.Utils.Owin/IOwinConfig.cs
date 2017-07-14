using System.Web.Http;

namespace Platform.Utils.Owin
{
    using global::Owin;

    public interface IOwinConfig
    {
        void Register(IAppBuilder app, HttpConfiguration config);
    }
}

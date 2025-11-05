using System.Web.Http;
using Newtonsoft.Json.Serialization;

public static class WebApiConfig
{
	public static void Register(HttpConfiguration config)
	{
		config.MapHttpAttributeRoutes();

		config.Routes.MapHttpRoute(
			name: "DefaultApi",
			routeTemplate: "api/{controller}/{id}",
			defaults: new { id = RouteParameter.Optional }
		);
		
		// Prefer JSON over XML for browsers
		config.Formatters.Remove(config.Formatters.XmlFormatter);
		
		config.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
			new CamelCasePropertyNamesContractResolver();
	}
}
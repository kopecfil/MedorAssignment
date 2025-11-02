using System.Web.Http;

public sealed class PingController : ApiController
{
	[HttpGet, Route("api/ping")]
	public IHttpActionResult Get() => Ok(new { ok = true, framework = ".NET Framework Web API 2" });
}
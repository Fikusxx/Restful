using Library.Background;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System.Net;

namespace Library.Filters;

// prevents the action filter methods to be invoked twice
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class ETagFilter : ActionFilterAttribute, IAsyncActionFilter
{
	public override async Task OnActionExecutionAsync(ActionExecutingContext executingContext, ActionExecutionDelegate next)
	{
		var request = executingContext.HttpContext.Request;

		var executedContext = await next();
		var response = executedContext.HttpContext.Response;

		// Computing ETags for Response Caching on GET requests
		if (request.Method == HttpMethod.Get.Method && response.StatusCode == StatusCodes.Status200OK)
		{
			ValidateETagForResponseCaching(executedContext);
		}
	}
	private void ValidateETagForResponseCaching(ActionExecutedContext executedContext)
	{
		if (executedContext.Result == null)
		{
			return;
		}

		var request = executedContext.HttpContext.Request;
		var response = executedContext.HttpContext.Response;

		IBaseEntity result = null;
		IEnumerable<IBaseEntity> results = null;

		if (executedContext.Result is ObjectResult res)
		{
			if(res.Value is IBaseEntity entity)
				result = entity;

			if(res.Value is IEnumerable<IBaseEntity> entities)
				results = entities;
		}

		// generate ETag from LastModified property
		//var etag = GenerateEtagFromLastModified(result.LastModified);

		// generates ETag from the entire response Content
		var etag = GenerateEtagFromResponseBodyWithHash(result);

		if (request.Headers.ContainsKey(HeaderNames.IfNoneMatch))
		{
			// fetch etag from the incoming request header
			var incomingEtag = request.Headers.IfNoneMatch.ToString();

			// if both the etags are equal
			// raise a 304 Not Modified Response
			if (incomingEtag.Equals(etag))
			{
				executedContext.Result = new StatusCodeResult(StatusCodes.Status304NotModified);
			}
		}

		// add ETag response header 
		response.Headers.ETag = etag;
	}

	private string GenerateEtagFromResponseBodyWithHash(IBaseEntity entity)
	{
		if (entity == null)
			return null;

		return entity.LastModified.Ticks.ToString("x");
	}
}

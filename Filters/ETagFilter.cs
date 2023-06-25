using Library.Background;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System.Net;

namespace Library.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class ETagFilter : ActionFilterAttribute, IAsyncActionFilter
{
	public override async Task OnActionExecutionAsync(ActionExecutingContext executingContext, ActionExecutionDelegate next)
	{
		var request = executingContext.HttpContext.Request;
		var executedContext = await next();
		var response = executedContext.HttpContext.Response;

		if (request.Method == HttpMethod.Get.Method && response.StatusCode == StatusCodes.Status200OK)
			ValidateETagForResponseCaching(executedContext);
	}
	private void ValidateETagForResponseCaching(ActionExecutedContext executedContext)
	{
		if (executedContext.Result == null)
			return;

		var request = executedContext.HttpContext.Request;
		var response = executedContext.HttpContext.Response;

		IBaseEntity result = null;
		IEnumerable<IBaseEntity> results = null;

		if (executedContext.Result is ObjectResult res)
		{
			if (res.Value is IBaseEntity entity)
				result = entity;

			if (res.Value is IEnumerable<IBaseEntity> entities)
				results = entities;
		}

		var etag = GenerateEtagFromResponse(result);

		if (request.Headers.ContainsKey(HeaderNames.IfNoneMatch))
		{
			var incomingEtag = request.Headers.IfNoneMatch.ToString();

			if (incomingEtag.Equals(etag))
				executedContext.Result = new StatusCodeResult(StatusCodes.Status304NotModified);
		}

		response.Headers.ETag = etag;
	}

	private string GenerateEtagFromResponse(IBaseEntity entity)
	{
		if (entity == null)
			return null;

		return entity.LastModified.Ticks.ToString("x");
	}
}

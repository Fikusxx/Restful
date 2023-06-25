using Library.Models;
using MediatR;

namespace Library.Caching;

public class GetAuthorsQuery : IRequest<AuthorDTO>
{
	public Guid Id { get; set; }
}

public interface ICachingService<TRequest, TResponse>
{
	public TResponse GetData(TRequest request);
	public void SetData(TRequest request, TResponse response);
}

public abstract class BaseCachingService
{
	protected readonly IHttpContextAccessor httpContextAccessor;

	public BaseCachingService(IHttpContextAccessor httpContextAccessor)
	{
		this.httpContextAccessor = httpContextAccessor;
	}

	protected string GetIncomingETag()
	{
		var incomingEtag = httpContextAccessor.HttpContext!.Request.Headers.IfNoneMatch.ToString();

		return incomingEtag;
	}
}

public class CachingService : BaseCachingService, ICachingService<GetAuthorsQuery, AuthorDTO>
{
	public CachingService(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
	{ }

	public AuthorDTO GetData(GetAuthorsQuery request)
	{
		// #1 cached data
		AuthorDTO cachedData = null;

		// #2 if data was not found
		if (cachedData == null)
			return null;

		// #3 if was was found
		var incomingEtag = GetIncomingETag();
		var cachedDataEtag = "";

		// #1 CASE: if etag that has been sent is equal to a cached one
		// #2 CASE: if user is getting data for the first time his session, thus his etag is abscent
		// #3 CASE: data has been changed since his last fetch and his etag is an actual value
		httpContextAccessor.HttpContext!.Response.Headers.ETag = cachedDataEtag; // refresh etag header
		return cachedData;
	}

	public void SetData(GetAuthorsQuery request, AuthorDTO response) // add expiryTime
	{
		// #1 generate key & ETAG based on the request and response
		var newEtag = "Newly Created ETag";
		var key = "newKey";

		// #2 store data + ETAG based on the response

		// #3 set new etag
		httpContextAccessor.HttpContext!.Response.Headers.ETag = newEtag;
	}
}

public class CachingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
	private readonly IEnumerable<ICachingService<TRequest, TResponse>> services;
	private readonly ICachingService<TRequest, TResponse>? service;

	public CachingBehaviour(IEnumerable<ICachingService<TRequest, TResponse>> services)
	{
		this.services = services;
		this.service = services.FirstOrDefault();
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		if (service == null || IsQuery() == false)
			return await next();

		var data = service.GetData(request);

		if (data != null)
			return data;

		var response = await next();
		service.SetData(request, response);

		return response;
	}

	private static bool IsQuery()
	{
		return typeof(TRequest).Name.EndsWith("Query");
	}
}

public class QueryHandler : IRequestHandler<GetAuthorsQuery, AuthorDTO>
{
	public async Task<AuthorDTO> Handle(GetAuthorsQuery request, CancellationToken cancellationToken)
	{
		await Task.Delay(1);

		return new AuthorDTO() { Name = "From RequestHandler" };
	}
}


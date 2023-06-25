using Library.Models;
using MediatR;

namespace Library.Caching;

public class GetAuthorsQuery : IRequest<AuthorDTO>
{
	public Guid Id { get; set; }
}

public interface ICachingCommandService<TRequest, TResponse>
{
	public void RemoveData(TRequest request);
}

public interface ICachingQueryService<TRequest, TResponse>
{
	public TResponse GetData(TRequest request);
	public void SetData(TRequest request, TResponse response);
}

public class CachingService : ICachingQueryService<GetAuthorsQuery, AuthorDTO>
{
	public CachingService()
	{ }

	public AuthorDTO GetData(GetAuthorsQuery request)
	{
		// #1 cached data
		AuthorDTO cachedData = null;

		// #2 if data was not found
		if (cachedData == null)
			return null;

		// #3 if was was found
		return cachedData;
	}

	public void SetData(GetAuthorsQuery request, AuthorDTO response) // add expiryTime
	{
		// #1 generate key & ETAG based on the request and response
		var key = "newKey";

		// #2 store data + ETAG based on the response

		// #3 set new etag
	}
}

public class CachingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
	private readonly IEnumerable<ICachingQueryService<TRequest, TResponse>> services;
	private readonly ICachingQueryService<TRequest, TResponse>? service;

	public CachingBehaviour(IEnumerable<ICachingQueryService<TRequest, TResponse>> services)
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


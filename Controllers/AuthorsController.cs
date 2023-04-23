using AutoMapper;
using Library.API.Entities;
using Library.API.Services;
using Library.Helpers;
using Library.Models;
using Library.Resources;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Text.Json;

namespace Library.Controllers;

[ApiController]
[Route("api/authors")]
public class AuthorsController : ControllerBase
{
	private readonly IMapper mapper;
	private readonly ILibraryRepository libraryRepository;

	public AuthorsController(ILibraryRepository libraryRepository, IMapper mapper)
	{
		this.libraryRepository = libraryRepository;
		this.mapper = mapper;
	}

	[HttpGet]
	[HttpHead]
	public ActionResult<IEnumerable<AuthorDTO>> GetAuthors([FromQuery] AuthorsResourceParameters parameters)
	{
		var authors = libraryRepository.GetAuthors(parameters);
		var paginationMetaData = new
		{
			totalItemsCount = authors.TotalCount,
			pageSize = authors.PageSize,
			currentPage = authors.CurrentPage,
			totalPages = authors.TotalPages,
			previousPage = authors.HasPrevious ? CreateAuthorsResourceUri(parameters, ResourceUriType.PreviousPage) : null,
			nextPage = authors.HasNext ? CreateAuthorsResourceUri(parameters, ResourceUriType.NextPage) : null
		};
		HttpContext.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetaData));
		var authorsDTO = mapper.Map<List<AuthorDTO>>(authors);

		return Ok(authorsDTO);
	}

	[HttpGet]
	[Route("{authorId}")]
	public ActionResult<AuthorDTO> GetById(Guid authorId)
	{
		var author = libraryRepository.GetAuthor(authorId);

		if (author == null)
			return NotFound(new { IsSuccess = false, Message = nameof(authorId) + " doesnt exist" });

		var authorDTO = mapper.Map<AuthorDTO>(author);

		return Ok(authorDTO);
	}

	[HttpPost]
	public ActionResult<AuthorDTO> CreateAuthor(CreateAuthorDTO authorDTO)
	{
		var author = mapper.Map<Author>(authorDTO);
		libraryRepository.AddAuthor(author);
		libraryRepository.Save();

		var authorToReturn = mapper.Map<AuthorDTO>(author);

		return CreatedAtAction(nameof(GetById), new { authorId = authorToReturn.Id }, authorToReturn);
	}

	[HttpOptions]
	public IActionResult GetAuthorsOptions()
	{
		Response.Headers.Add("Allow", "GET,OPTIONS,POST");
		// Respose.Body describing available options
		return Ok();
	}

	[HttpDelete]
	[Route("{authorId}")]
	public IActionResult DeleteAuthor(Guid authorId)
	{
		var author = libraryRepository.GetAuthor(authorId);

		if (author == null)
			return NotFound(new { IsSuccess = false, Message = nameof(authorId) + " doesnt exist" });

		libraryRepository.DeleteAuthor(author);
		libraryRepository.Save();

		return NoContent();
	}

	[HttpGet]
	[Route("report")]
	public IActionResult GetReport(IEnumerable<CallCenterRowReportExcel> rows)
	{
		var report = GetExcelReport(rows);
		return Ok(report);
	}

	public byte[] GetExcelReport(IEnumerable<CallCenterRowReportExcel> rows)
	{
		ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
		var file = new FileInfo(@"C:\report.xlsx");
		//using var package = new ExcelPackage(new FileInfo("Report"));
		using var package = new ExcelPackage(file);

		var sheet = package.Workbook.Worksheets.Add("Report");
		var rowReports = rows.ToArray().AsSpan();
		var len = rowReports.Length;
		var column = 1;
		var currentRow = 1;

		foreach (var rowReport in rowReports)
		{
			var row = currentRow;
			sheet.Cells[row, column].Value = rowReport.Title;
			rowReport.Values.ForEach(value =>
			{
				column++;
				sheet.Cells[row, column].Value = value;
			});
			currentRow++;
			column = 1;
		}

		// Styling
		using ExcelRange firstRow = sheet.Cells["A1:Z1"];
		firstRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
		firstRow.Style.Font.Bold = true;
		firstRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
		using ExcelRange firstColumn = sheet.Cells[$"A1:A{rowReports.Length}"];
		firstColumn.Style.Font.Bold = true;
		using ExcelRange lastRow = sheet.Cells[$"A{rowReports.Length}:Z{rowReports.Length}"];
		lastRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
		lastRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

		return package.GetAsByteArray();
	}

	private string CreateAuthorsResourceUri(AuthorsResourceParameters parameters, ResourceUriType type)
	{
		switch (type)
		{
			case ResourceUriType.PreviousPage:
				return Url.ActionLink(nameof(GetAuthors),
					values: new
					{
						pageNumber = parameters.PageNumber - 1,
						pageSize = parameters.PageSize,
						mainCategory = parameters.MainCategory,
						searchQuery = parameters.SearchQuery
					})!;

			case ResourceUriType.NextPage:
				return Url.ActionLink(nameof(GetAuthors),
					values: new
					{
						pageNumber = parameters.PageNumber + 1,
						pageSize = parameters.PageSize,
						mainCategory = parameters.MainCategory,
						searchQuery = parameters.SearchQuery
					})!;

			default:
				return Url.ActionLink(nameof(GetAuthors),
					values: new
					{
						pageNumber = parameters.PageNumber,
						pageSize = parameters.PageSize,
						mainCategory = parameters.MainCategory,
						searchQuery = parameters.SearchQuery
					})!;
		}
	}
}

public class CallCenterRowReportExcel
{
	public string Title { get; set; }
	public List<string> Values { get; set; }
}

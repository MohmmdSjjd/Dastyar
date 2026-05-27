namespace Dastyar.Application.Products.Dtos;

public sealed record ExportProductsResultDto(byte[] Content, string FileName, string ContentType);

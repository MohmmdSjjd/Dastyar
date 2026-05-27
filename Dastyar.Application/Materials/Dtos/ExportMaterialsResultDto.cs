namespace Dastyar.Application.Materials.Dtos;

public sealed record ExportMaterialsResultDto(byte[] Content, string FileName, string ContentType);

namespace TickTracker.Api.DTOs;

public record AssetsResponseDto(
    PagingDto Paging,
    List<AssetsDto> Data);

namespace TickTracker.Api.DTOs;

public record PagingDto(
    int Page, 
    int Pages, 
    int Items);

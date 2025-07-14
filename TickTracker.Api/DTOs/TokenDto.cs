namespace TickTracker.Api.DTOs;

public record TokenDto(
    string Access_token, 
    int Expires_in);
namespace TP2_API.Models;

public enum DeleteUserResult
{
    Success,
    UserNotFound,
    NotAuthorized,
    NotInactiveLongEnough
}
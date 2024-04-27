namespace OpenDoors.Exceptions;

public class TenantNotFoundException(string message) : ArgumentException(message)
{
}

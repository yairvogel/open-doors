namespace OpenDoors.Exceptions;

public class AccessGroupNotFoundException(string message) : ArgumentException(message)
{}


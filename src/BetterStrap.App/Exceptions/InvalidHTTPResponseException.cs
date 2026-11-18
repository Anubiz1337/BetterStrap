using System;

namespace BetterStrap.Exceptions;

internal sealed class InvalidHTTPResponseException(string message) : Exception(message)
{
}

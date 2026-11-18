using System;

namespace BetterStrap.Exceptions;

internal sealed class ChecksumFailedException(string message) : Exception(message)
{
}

using System;
using System.Net;

namespace BetterStrap.Exceptions;

public class InvalidChannelException(HttpStatusCode? statusCode) : Exception
{
    public HttpStatusCode? StatusCode = statusCode;
}

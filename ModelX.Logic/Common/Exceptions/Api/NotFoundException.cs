﻿using ModelX.Logic.Common.Exceptions.Base;

namespace ModelX.Logic.Common.Exceptions.Api;

public class NotFoundException : BaseException
{
    public NotFoundException()
        : base("One or more entities were not found.")
    {
        Failures = new Dictionary<string, string[]>();
    }

    public NotFoundException(params string[] errors)
        : this()
    {
        if (errors != null)
        {
            Failures.Add(string.Empty, errors);
        }
    }

    public IDictionary<string, string[]> Failures { get; }
}
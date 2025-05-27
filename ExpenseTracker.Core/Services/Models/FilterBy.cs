
using System;

[Flags]
public enum FilterBy
{
    None = 1,
    Date = 2,
    Category = 4,
    Search = 8,
    Source = 16,
}
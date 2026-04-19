using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string mensaje) : base(mensaje) { }
}

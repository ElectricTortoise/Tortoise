﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core
{
    public class CustomException : Exception
    {
        public CustomException() : base() { }
        public CustomException(string message) : base(message) { }
        public CustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Tests.Diagnostics
{
    public class EqualityOnFloatingPoint
    {
        void test(float f, Double d)
        {
            dynamic din = null;
            if (din == null)
            {
            }

            if (f == 3.14F) //Noncompliant
            {
            }

            if (f != 3.14F) //Noncompliant
            { }

            var b = d == 3.14; //Noncompliant

            if (true && f >= 3.146)
            {
            }

            if (f <= 3.146 && ((f >= 3.146))) // Noncompliant indirect equality test
            {
            }
            var i = 3;
            if (i <= 3 && i >= 3)
            {
            }

            if (i < 4 || i > 4)
            {
            }

            if (f < 3.146 || f > 3.146) // Noncompliant indirect inequality test
            {
            }

            if (f <= 3.146 && true && f >= 3.146) // Not recognized
            {
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FattureWebAuxiliar
{
    public static class DecimalExtensions
    {
        public static decimal ToFixed(this decimal value, int qtdCasas)
        {
            return Math.Round(value, qtdCasas);
        }

        public static decimal? ToFixed(this decimal? value, int qtdCasas)
        {
            if (value.HasValue)
            {
                return Math.Round(value.Value, qtdCasas);
            }
            else
            {
                return null;
            }
        }
    }
}

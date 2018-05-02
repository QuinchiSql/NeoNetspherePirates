using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoNetsphere
{
    class Namecheck
    {
        public static bool IsNameValid(string name)
        {
            return !name.StartsWith("[") && !name.Contains("GM") && !name.Contains("GS") && !name.ToLower().Contains("admin") && name.All(c => char.IsLetterOrDigit(c) || c == '_');
        }
    }
}

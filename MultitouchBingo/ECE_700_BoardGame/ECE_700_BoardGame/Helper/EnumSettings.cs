using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECE_700_BoardGame.Helper
{
    public static class EnumSettings
    {
        //An orientation setter for reflection rather than rotation
        public enum ItemOrientation
        {
            UP,
            DOWN,
            LEFT,
            RIGHT,
        }
    }
}

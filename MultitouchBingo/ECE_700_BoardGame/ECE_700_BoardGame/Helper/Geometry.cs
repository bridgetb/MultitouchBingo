using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Surface.Core;

namespace ECE_700_BoardGame.Helper
{
    public static class Geometry
    {
        public static bool Contains(TouchBounds tb, float posX, float posY)
        {
            if((tb.Left < posX) && (tb.Right > posX)){
                if((tb.Top < posY) && (tb.Bottom > posY)){
                    return true;
                }
            }
            return false;
        }
    }
}

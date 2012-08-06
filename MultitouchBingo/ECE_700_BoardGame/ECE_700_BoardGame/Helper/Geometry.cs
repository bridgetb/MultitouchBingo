using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Surface.Core;

namespace ECE_700_BoardGame.Helper
{
    public static class Geometry
    {
        //Check if a point is within touchbounds
        public static bool Contains(TouchBounds tb, float posX, float posY)
        {
            //Checks equal to aswell as tag points have a touch bound of 0 width and height
            if((tb.Left <= posX) && (tb.Right >= posX)){
                if((tb.Top <= posY) && (tb.Bottom >= posY)){
                    return true;
                }
            }
            return false;
        }
    }
}

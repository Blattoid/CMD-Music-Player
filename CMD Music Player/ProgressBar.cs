using System;
using System.Linq;

namespace CMD_Music_Player
{
    public class ProgressBar
    {
        private static HelperFunctions functions = new HelperFunctions();

        decimal Map(decimal value, decimal fromSource, decimal toSource, decimal fromTarget, decimal toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }

        //this was converted by hand from Python to C#.
        public string GenerateBar(double value, double maxvalue,
            string barprefix = "[", string filledchar = "=", string pointerchar = ">",
            string emptychar = " ", string barsuffix = "]", int barwidth = 50)
        {
            //sanity checks to make sure nothing is going to break anything
            if (value > maxvalue)
            {
                functions.Error("Current value is bigger than max value!");
                return "";
            }
            if (value < 0)
            {
                functions.Error("Current value is smaller than 0!");
                return "";
            }
            if (maxvalue == 0)
            {
                functions.Error("Error generating bar: max length is 0.");
                return "";
            }

            decimal percentage = (decimal)value / (decimal)maxvalue * 100; //calculate the percentage of progress
                                                                           //calculate how much space the non-changing parts of the bar will take up
            int length = (
                barprefix.Length +
                pointerchar.Length +
                barsuffix.Length
            );

            //If that length is smaller than the barwidth, then there will literally be no space left for the bar to move. Oh well.
            //adjust the bar to fit the space it has, using the length we just calculated
            value = Convert.ToInt32(Map((decimal)value, 0, (decimal)maxvalue, 0, barwidth - length));
            maxvalue = barwidth - length;

            //construct the bar as a string
            string bar = barprefix; //start with the bar prefix
            bar += String.Concat(Enumerable.Repeat(filledchar, (int)value)); //the currently filled portion of the bar
            bar += pointerchar;
            bar += String.Concat(Enumerable.Repeat(emptychar, (int)maxvalue - (int)value)); //the empty portion of the bar
            bar += barsuffix + " "; //separator

            return bar; //send the finished thing back
        }
    }
}
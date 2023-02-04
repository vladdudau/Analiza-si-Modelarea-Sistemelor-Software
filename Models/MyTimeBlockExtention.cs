using Itenso.TimePeriod;
using System;
using System.Text;

namespace ProiectAnaliza.Models
{
    public class MyTimeBlockExtention : TimeBlock
    {
        public MyTimeBlockExtention(DateTime x, TimeSpan y)
            : base(x, y)
        {
            // Calling base constructor  
        }

        //Overriding toString Method
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Start.ToShortTimeString());
            sb.Append(" pana la ");
            sb.Append(this.End.ToShortTimeString());
            return sb.ToString();
        }
    }
}
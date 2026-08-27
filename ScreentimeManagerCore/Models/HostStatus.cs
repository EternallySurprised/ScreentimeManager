using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreentimeManagerCore.Models
{
    public class HostStatus
    {
        public virtual string Hostname { get; set; } = "";
        public virtual bool IsOnline { get; set; }
        public virtual TimeSpan ScreentimeLeft { get; set; }
        public virtual TimeSpan ScreentimeLimit { get; set; }
        public virtual double ScreentimeUsedPercent
        {
            get
            {
                double ret = (1 - ScreentimeLeft.Divide(ScreentimeLimit)) * 100.0;
                return double.IsNaN(ret) || double.IsNegative(ret) ? 0.0 : ret;
            }
        }
    }
}

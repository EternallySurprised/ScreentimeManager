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
        public virtual TimeSpan MaximumScreentime { get; set; }
        public virtual double ScreentimeUsedPercent
        {
            get
            {
                return ScreentimeLeft.Divide(MaximumScreentime) * 100.0;
            }
        }
    }
}

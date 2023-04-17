using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFMvvM.Common;

public interface IApp
{
    void LogAndShowCriticalException(string message, Exception? ex = null);
}

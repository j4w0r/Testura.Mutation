﻿using System;
using log4net.Appender;

namespace Cama.Core.Logs
{
    public class MemoryAppenderWithEvents : MemoryAppender
    {
        public event EventHandler Updated;

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            // Append the event as usual
            base.Append(loggingEvent);

            Updated?.Invoke(this, new EventArgs());
        }
    }
}
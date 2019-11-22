﻿using System;

namespace ExpenseTracker.UI
{
    public class MenuAction
    {
        public string Command { get; set; }

        public Func<string> GetDescription { get; set; }

        public Action Callback { get; set; }

        public string Group { get; set; }

        public int Ordinal { get; internal set; }
    }
}
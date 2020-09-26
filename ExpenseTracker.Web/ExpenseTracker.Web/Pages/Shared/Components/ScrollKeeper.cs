using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Web.Pages.Shared.Components
{
    public class ScrollKeeper : ViewComponent
    {
        [BindProperty(SupportsGet = true)]
        public int XPosition { get; set; }

        [BindProperty(SupportsGet = true)]
        public int YPosition { get; set; }

    }
}

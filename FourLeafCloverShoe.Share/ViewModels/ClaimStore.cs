﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FourLeafCloverShoe.Share.ViewModels
{
    public static class ClaimStore
    {
        public static List<Claim> claimsList = new List<Claim>()
        {
            new Claim("Create", "Create"),
            new Claim("Edit", "Edit"),
            new Claim("Delete", "Delete")
        };
    }
}

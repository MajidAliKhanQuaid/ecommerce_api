using freshstore.bll.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Extensions
{
    public static class RequestExtensions
    {   
        public static string GetUserEmail(this HttpRequest request)
        {
            return request.HttpContext?.User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        public static string GetUserName(this HttpRequest request)
        {
            return request.HttpContext?.User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
        }

    }
}

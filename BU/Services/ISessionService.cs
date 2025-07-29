using DAL.DB;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BU.Services;

public interface ISessionService
{
    Task SetCurrentUserAsync(int userId, string userName);
    Task<int?> GetCurrentUserIdAsync();
    Task<string?> GetCurrentUserNameAsync();
    Task ClearSessionAsync();
    bool IsLoggedIn();
}
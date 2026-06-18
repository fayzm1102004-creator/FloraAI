using FloraAI.API.DTOs.Admin;
using System.Threading.Tasks;

namespace FloraAI.API.Services.Interfaces;

public interface IAdminService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}

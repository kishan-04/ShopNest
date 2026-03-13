using ShopNest.API.DTOs;
using ShopNest.API.Repositories;

namespace ShopNest.API.Services;

public class DashboardService
{
    private readonly DashboardRepository _dashboardRepository;

    public DashboardService(DashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<DashboardDto> GetDashboardDataAsync()
    {
        return await _dashboardRepository.GetDashboardDataAsync();
    }
}
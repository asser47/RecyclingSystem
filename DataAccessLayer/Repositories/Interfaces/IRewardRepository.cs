using DataAccessLayer.Entities;
using RecyclingSystem.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories.Interfaces
{
    public interface IRewardRepository : IGenericRepository<Reward>
    {
        Task<IEnumerable<Reward>> GetAvailableRewardsForUserAsync(int userPoints);
        Task<Reward?> GetRewardWithHistoryAsync(int rewardId);
        Task<Reward?> GetRewardForRedemptionAsync(int rewardId);
        Task<IEnumerable<Reward>> GetRewardsByCategoryAsync(string category, int? userPoints = null);
        Task<IEnumerable<Reward>> GetPopularRewardsAsync(int topCount = 10);
        Task<IEnumerable<Reward>> GetLowStockRewardsAsync(int threshold = 10);
        Task<IEnumerable<Reward>> SearchRewardsAsync(string searchTerm, int? userPoints = null);
        Task<IEnumerable<Reward>> GetRewardsInPointRangeAsync(int minPoints, int maxPoints);
        Task<IEnumerable<HistoryReward>> GetUserRedemptionHistoryAsync(string userId);
        Task<IEnumerable<Reward>> GetAlmostAffordableRewardsAsync(int userPoints, int pointsGap = 50);
    }
}

using DataAccessLayer.Context;
using DataAccessLayer.Entities;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using RecyclingSystem.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories.Impementations
{
    public class RewardRepository : GenericRepository<Reward>, IRewardRepository
    {
        public RewardRepository(RecyclingDbContext context) : base(context)
        {
        }
        // Added method to get available rewards for a user based on their points
        public async Task<IEnumerable<Reward>> GetAvailableRewardsForUserAsync(int userPoints)
        {
            if (userPoints < 0)
                return Enumerable.Empty<Reward>();

            return await _dbSet
                .AsNoTracking()
                .Where(r => r.RequiredPoints <= userPoints && r.IsAvailable && r.StockQuantity > 0)
                .OrderBy(r => r.RequiredPoints)
                .ToListAsync();
        }
        // Added method to get rewards by category with optional user points filter
        public async Task<IEnumerable<Reward>> GetRewardsByCategoryAsync(string category, int? userPoints = null)
        {
            var query = _dbSet.AsNoTracking()
                .Where(r => r.Category == category && r.IsAvailable && r.StockQuantity > 0);

            if (userPoints.HasValue)
            {
                query = query.Where(r => r.RequiredPoints <= userPoints.Value);
            }

            return await query.OrderBy(r => r.RequiredPoints).ToListAsync();
        }
        // Added method to get reward details along with its redemption history
        public async Task<Reward?> GetRewardWithHistoryAsync(int rewardId)
        {
            return await _dbSet
                .Include(r => r.HistoryRewards)
                    .ThenInclude(hr => hr.User)
                .FirstOrDefaultAsync(r => r.ID == rewardId);
        }
        // Added method to get the most popular rewards based on redemption count
        public async Task<IEnumerable<Reward>> GetPopularRewardsAsync(int topCount = 10)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(r => r.IsAvailable && r.StockQuantity > 0)
                .Include(r => r.HistoryRewards)
                .OrderByDescending(r => r.HistoryRewards.Count)
                .Take(topCount)
                .ToListAsync();
        }
        // Added method to get rewards that are low in stock
        public async Task<IEnumerable<Reward>> GetLowStockRewardsAsync(int threshold = 10)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(r => r.IsAvailable && r.StockQuantity > 0 && r.StockQuantity <= threshold)
                .OrderBy(r => r.StockQuantity)
                .ToListAsync();
        }
        // Added method to search rewards by name or description with optional user points filter
        public async Task<IEnumerable<Reward>> SearchRewardsAsync(string searchTerm, int? userPoints = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<Reward>();

            var query = _dbSet.AsNoTracking()
                .Where(r => r.IsAvailable && r.StockQuantity > 0 &&
                            (r.Name.Contains(searchTerm) || r.Description.Contains(searchTerm)));

            if (userPoints.HasValue)
            {
                query = query.Where(r => r.RequiredPoints <= userPoints.Value);
            }

            return await query.OrderBy(r => r.RequiredPoints).ToListAsync();
        }
        // Added method to get a reward for redemption, ensuring it's available and in stock
        public async Task<Reward?> GetRewardForRedemptionAsync(int rewardId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.ID == rewardId && r.IsAvailable && r.StockQuantity > 0);
        }
        // Added method to get rewards in a specific point range
        public async Task<IEnumerable<Reward>> GetRewardsInPointRangeAsync(int minPoints, int maxPoints)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(r => r.IsAvailable &&
                            r.StockQuantity > 0 &&
                            r.RequiredPoints >= minPoints &&
                            r.RequiredPoints <= maxPoints)
                .OrderBy(r => r.RequiredPoints)
                .ToListAsync();
        }

        // Added method to get user redemption history
        public async Task<IEnumerable<HistoryReward>> GetUserRedemptionHistoryAsync(string userId)
        {
            return await _context.Set<HistoryReward>()
                .AsNoTracking()
                .Where(hr => hr.UserId == userId)
                .Include(hr => hr.Reward)
                .OrderByDescending(hr => hr.RedeemedAt)
                .ToListAsync();
        }
        // Added method to check if a reward is redeemable by a user based on their points
        public async Task<IEnumerable<Reward>> GetAlmostAffordableRewardsAsync(int userPoints, int pointsGap = 50)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(r => r.IsAvailable &&
                            r.StockQuantity > 0 &&
                            r.RequiredPoints > userPoints &&
                            r.RequiredPoints <= userPoints + pointsGap)
                .OrderBy(r => r.RequiredPoints)
                .ToListAsync();
        }
        //

    }
}
using BussinessLogicLayer.DTOs.Reward;
using BusinessLogicLayer.IServices;
using DataAccessLayer.Repositories.Interfaces;
using RecyclingSystem.DataAccess.Entities;

namespace BusinessLogicLayer.Services
{
    public class RewardService : IRewardService
    {
        private readonly IRewardRepository _rewardRepo;

        public RewardService(IRewardRepository rewardRepo)
        {
            _rewardRepo = rewardRepo;
        }

        // Get all rewards
        public async Task<IEnumerable<RewardDto>> GetAllAsync()
        {
            var rewards = await _rewardRepo.GetAllAsync();
            return rewards.Select(r => new RewardDto
            {
                ID = r.ID,
                Title = r.Name,
                Description = r.Description,
                RequiredPoints = r.RequiredPoints
            });
        }
        // Get reward by ID
        public async Task<RewardDto?> GetByIdAsync(int id)
        {
            var reward = await _rewardRepo.GetByIdAsync(id);
            if (reward == null) return null;

            return new RewardDto
            {
                ID = reward.ID,
                Title = reward.Name,
                Description = reward.Description,
                RequiredPoints = reward.RequiredPoints
            };
        }

        // Add new reward
        public async Task AddAsync(RewardDto dto)
        {
            var reward = new Reward
            {
                Name = dto.Title,
                Description = dto.Description,
                RequiredPoints = dto.RequiredPoints,
                Category = "General",
                StockQuantity = 1,
                IsAvailable = true
            };

            await _rewardRepo.AddAsync(reward);
        }

        // Update existing reward
        public async Task UpdateAsync(RewardDto dto)
        {
            var reward = await _rewardRepo.GetByIdAsync(dto.ID);
            if (reward == null) return;

            reward.Name = dto.Title;
            reward.Description = dto.Description;
            reward.RequiredPoints = dto.RequiredPoints;

            // Use synchronous Update from GenericRepository
            _rewardRepo.Update(reward);
        }

        // Delete reward by ID
        public async Task DeleteAsync(int id)
        {
            var reward = await _rewardRepo.GetByIdAsync(id);
            if (reward == null) return;

            // Use synchronous Remove from GenericRepository
            _rewardRepo.Remove(reward);
        }
        //        REDEEM
        // Redeem a reward: deduct points, decrease stock, mark for redemption
        public async Task<string> RedeemAsync(string userId, int rewardId, int userPoints)
        {
            // 1) Get the reward
            var reward = await _rewardRepo.GetByIdAsync(rewardId);
            if (reward == null)
                return "Reward not found";

            if (!reward.IsAvailable || reward.StockQuantity <= 0)
                return "Reward is not available";

            // 2) Check if the user has enough points
            if (userPoints < reward.RequiredPoints)
                return "Not enough points";

            // 3) Deduct user points
            userPoints -= reward.RequiredPoints;

            // 4) Decrease reward stock
            reward.StockQuantity--;
            if (reward.StockQuantity == 0)
                reward.IsAvailable = false;

            _rewardRepo.Update(reward); // synchronous update

            // 5) Convert points to redeemed product
            // The redemption process is completed here
            // Status can later be updated to Pending -> Completed when the collector receives the reward

            return $"Reward '{reward.Name}' redeemed successfully. Points deducted: {reward.RequiredPoints}";
        }
    }
}

using BusinessLogicLayer.IServices;
using BussinessLogicLayer.DTOs.Reward;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RewardController : ControllerBase
    {
        private readonly IRewardService _rewardService;

        public RewardController(IRewardService rewardService)
        {
            _rewardService = rewardService;
        }

        // GET: api/reward
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rewards = await _rewardService.GetAllAsync();
            return Ok(rewards);
        }

        // GET: api/reward/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reward = await _rewardService.GetByIdAsync(id);
            if (reward == null)
                return NotFound();

            return Ok(reward);
        }

        // GET: api/reward/available
        [Authorize]
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableForUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rewards = await _rewardService.GetAvailableRewardsForUserAsync(userId);
            return Ok(rewards);
        }

        // GET: api/reward/category/Electronics
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rewards = await _rewardService.GetRewardsByCategoryAsync(category, userId);
            return Ok(rewards);
        }

        // GET: api/reward/popular
        [HttpGet("popular")]
        public async Task<IActionResult> GetPopular([FromQuery] int topCount = 10)
        {
            var rewards = await _rewardService.GetPopularRewardsAsync(topCount);
            return Ok(rewards);
        }

        // GET: api/reward/search?term=gift
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string term)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rewards = await _rewardService.SearchRewardsAsync(term, userId);
            return Ok(rewards);
        }

        // GET: api/reward/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _rewardService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        // POST: api/reward
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRewardDto dto)
        {
            var reward = await _rewardService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = reward.ID }, reward);
        }

        // PUT: api/reward/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRewardDto dto)
        {
            if (id != dto.ID)
                return BadRequest();

            var reward = await _rewardService.UpdateAsync(dto);
            return Ok(reward);
        }

        // DELETE: api/reward/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _rewardService.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }

        // POST: api/reward/redeem
        [Authorize]
        [HttpPost("redeem")]
        public async Task<IActionResult> Redeem([FromBody] RedeemRewardDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _rewardService.RedeemRewardAsync(userId, dto);
            return Ok(new { success = result, message = "Reward redeemed successfully" });
        }

        // GET: api/reward/low-stock
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 10)
        {
            var rewards = await _rewardService.GetLowStockRewardsAsync(threshold);
            return Ok(rewards);
        }

        // GET: api/reward/5/stats
        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetStats(int id)
        {
            var stats = await _rewardService.GetRewardWithStatsAsync(id);
            if (stats == null)
                return NotFound();

            return Ok(stats);
        }

        // PATCH: api/reward/5/stock
        [HttpPatch("{id}/stock")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] int quantityChange)
        {
            var result = await _rewardService.UpdateStockAsync(id, quantityChange);
            return result ? Ok() : NotFound();
        }

    }
}

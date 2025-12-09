using BusinessLogicLayer.IServices;
using BussinessLogicLayer.DTOs.Order;
using DataAccessLayer.Entities;
using DataAccessLayer.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        // Points configuration for each material type per kilogram
        private readonly Dictionary<string, int> _pointsPerKg = new()
        {
            { MaterialType.Plastic.ToString(), 5 },
            { MaterialType.Paper.ToString(), 8 },
            { MaterialType.Can.ToString(), 10 }
        };

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ---------- helper mappers ----------
        private static OrderDto ToDto(Order entity)
        {
            return new OrderDto
            {
                ID = entity.ID,
                Status = entity.Status.ToString(),
                OrderDate = entity.OrderDate,
                UserId = entity.UserId,
                CollectorId = entity.CollectorId,
                FactoryId = entity.FactoryId,
                UserName = entity.User?.UserName,
                CollectorName = entity.Collector?.UserName,
                FactoryName = entity.Factory?.Name
            };
        }

        private static Order ToEntity(OrderDto dto)
        {
            return new Order
            {
                ID = dto.ID,
                Status = Enum.Parse<OrderStatus>(dto.Status),
                OrderDate = dto.OrderDate,
                UserId = dto.UserId,
                CollectorId = dto.CollectorId,
                FactoryId = dto.FactoryId
            };
        }

        private static void UpdateEntityFromDto(OrderDto dto, Order entity)
        {
            entity.Status = Enum.Parse<OrderStatus>(dto.Status);
            entity.OrderDate = dto.OrderDate;
            entity.UserId = dto.UserId;
            entity.CollectorId = dto.CollectorId;
            entity.FactoryId = dto.FactoryId;
        }

        // ---------- service methods ----------
        public async Task<IEnumerable<OrderDto>> GetAllAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            return orders.Select(o => ToDto(o));
        }

        public async Task<OrderDto?> GetByIdAsync(int id)
        {
            var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(id);
            return order == null ? null : ToDto(order);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(string userId)
        {
            var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);
            return orders.Select(o => ToDto(o));
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByCollectorIdAsync(string collectorId)
        {
            var orders = await _unitOfWork.Orders.GetOrdersByCollectorIdAsync(collectorId);
            return orders.Select(o => ToDto(o));
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByFactoryIdAsync(int factoryId)
        {
            var orders = await _unitOfWork.Orders.GetOrdersByFactoryIdAsync(factoryId);
            return orders.Select(o => ToDto(o));
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(string status)
        {
            var orderStatus = Enum.Parse<OrderStatus>(status);
            var orders = await _unitOfWork.Orders.GetOrdersByStatusAsync(orderStatus);
            return orders.Select(o => ToDto(o));
        }

        public async Task AddAsync(OrderDto dto)
        {
            var entity = ToEntity(dto);
            await _unitOfWork.Orders.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(OrderDto dto)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(dto.ID);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {dto.ID} not found.");

            UpdateEntityFromDto(dto, order);
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {id} not found.");
                                
            _unitOfWork.Orders.Remove(order);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> CompleteOrderAsync(int orderId)
        {
            // Get order with all details including materials and user
            var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");

            // Check if order is already completed
            if (order.Status == OrderStatus.Completed)
                return false;

            // Check if order is cancelled
            if (order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Cannot complete a cancelled order.");

            // Calculate points based on materials
            int totalPoints = CalculatePointsForOrder(order.Materials);

            // Update order status
            order.Status = OrderStatus.Completed;

            // Award points to user
            var user = await _unitOfWork.Users.GetByIdAsync(order.UserId);
            if (user != null)
            {
                user.Points += totalPoints;
                _unitOfWork.Users.Update(user);
            }

            // Save changes
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private int CalculatePointsForOrder(ICollection<Material> materials)
        {
            int totalPoints = 0;

            foreach (var material in materials)
            {
                if (_pointsPerKg.TryGetValue(material.TypeName ?? string.Empty, out int pointsPerKg))
                {
                    // Size is in kilograms, calculate points
                    int materialPoints = (int)(material.Size * pointsPerKg);
                    totalPoints += materialPoints;
                }
            }

            return totalPoints;
        }

        public async Task<int> GetPointsForOrderAsync(int orderId)
        {
            var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");

            return CalculatePointsForOrder(order.Materials);
        }
    }
}

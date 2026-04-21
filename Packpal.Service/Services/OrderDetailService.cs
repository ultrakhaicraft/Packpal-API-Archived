using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Packpal.BLL.Interface;
using Packpal.BLL.Utilities;
using Packpal.Contract.Repositories.Interface;
using Packpal.DAL.Entity;
using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.Services
{
	public class OrderDetailService : IOrderDetailService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public OrderDetailService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<string> CreateOrderDetailAsync(List<CreateOrderDetailModel> models, Guid orderId)
		{
			try
			{
				_unitOfWork.BeginTransaction();

				var order = await _unitOfWork.GetRepository<Order>().GetByIdAsync(orderId);
				if (order == null)
				{
					return "404";
				}



				foreach (var orderDetail in models)
				{
					var detail = _mapper.Map<OrderDetail>(orderDetail);
					detail.OrderId = orderId;
					await _unitOfWork.GetRepository<OrderDetail>().InsertAsync(detail);
				}

				await _unitOfWork.SaveAsync();
				_unitOfWork.CommitTransaction();

				await UpdatePriceToOrder(orderId);

				return "200";
			}
			catch (Exception e)
			{
				_unitOfWork.RollBack();
				throw new Exception(e.Message);
			}
		}

		public async Task<bool> DeleteOrderDetailAsync(Guid orderDetailId)
		{
			try
			{


				_unitOfWork.BeginTransaction();
				var orderDetail = await _unitOfWork.GetRepository<OrderDetail>().GetByIdAsync(orderDetailId);

				if (orderDetail == null)
					return false;

				_unitOfWork.GetRepository<OrderDetail>().Delete(orderDetail);
				await _unitOfWork.SaveAsync();
				_unitOfWork.CommitTransaction();

				await UpdatePriceToOrder(orderDetail.OrderId);
				return true;
			}
			catch (Exception e)
			{
				_unitOfWork.RollBack();
				throw new Exception(e.Message);
			}
		}

		public async Task<PagingModel<ViewOrderDetailModel>> GetAllOrderDetailsByOrderIdAsync(Guid orderId, int page = 1, int pageSize = 5)
		{
			/*
            var orderDetails = await _unitOfWork.GetRepository<OrderDetail>()
                .Entities
                .Where(od => od.OrderId == orderId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ViewOrderDetailModel>>(orderDetails);

			Previous is IEnumerable<ViewOrderDetailModel>
			*/

			var orderDetails = _unitOfWork.GetRepository<OrderDetail>()
				.Include(o => o.Size!)
				.Include(o => o.Order)
				.Where(o => o.OrderId == orderId);


			var views = orderDetails.Select(o => new ViewOrderDetailModel
			{
				Id = o.Id,
				OrderId = o.OrderId,
				SizeId = o.SizeId,
				SizeDescription = o.Size!.SizeDescription,
				Price = o.Size.Price,
				OrderStatus = o.Order!.Status
			});

			var pagedData = PagingExtension.ToPagingModel(views, page, pageSize);

			return pagedData;
		}
        
        public async Task<ViewOrderDetailModel> GetOrderDetailByIdAsync(Guid orderDetailId)
        {
            try
            {
				var orderDetail = await _unitOfWork.GetRepository<OrderDetail>().GetByIdAsync(orderDetailId);
				if (orderDetail == null)
				{
					return null;
				}
				//Get 2 entities: Size and Order and use it there through orderDetail.Id
				var size = await _unitOfWork.GetRepository<Size>().GetByIdAsync(orderDetail.SizeId);
				if(size == null)
				{
					return null;
				}

				var order = await _unitOfWork.GetRepository<Order>().GetByIdAsync(orderDetail.OrderId);
				if (order == null)
				{
					return null;
				}

				var orderDetailView = _mapper.Map<ViewOrderDetailModel>(orderDetail);
				orderDetailView.SizeDescription = size.SizeDescription;
				orderDetailView.Price = size.Price;
				orderDetailView.OrderStatus = order.Status;

				return orderDetailView;
			}
            catch (Exception e)
            {
                throw new Exception("Error while get order detail by id: " + e.Message);
			}
        }

        public async Task<Guid> UpdateOrderDetailAsync(UpdateOrderDetailModel model)
        {
			try
			{
				_unitOfWork.BeginTransaction();
				var orderDetail = await _unitOfWork.GetRepository<OrderDetail>().GetByIdAsync(model.Id);

				if (orderDetail == null)
				{
					return Guid.Empty;
				}
				_mapper.Map(model, orderDetail);
				_unitOfWork.GetRepository<OrderDetail>().Update(orderDetail);
				await _unitOfWork.SaveAsync();
				_unitOfWork.CommitTransaction();

				
				await UpdatePriceToOrder(orderDetail.OrderId);

				return model.Id;
			}
			catch (Exception e)
			{
				_unitOfWork.RollBack();
				throw new Exception(e.Message);
			}
        }

		private async Task UpdatePriceToOrder(Guid orderId)
		{
			
			//Get all order Detail by Order Id
			var orderDetails = _unitOfWork.GetRepository<OrderDetail>()
				.Include(o => o.Size!)
				.Include(o => o.Order)
				.Where(o => o.OrderId == orderId);

			var order = await _unitOfWork.GetRepository<Order>().GetByIdAsync(orderId);
			
			double totalAmount = 0;
			//Calculate base price: Size.Price × EstimatedDays
			foreach(var orderDetail in orderDetails)
			{
				totalAmount += orderDetail.Size!.Price * order.EstimatedDays;
			}

			order.TotalAmount = totalAmount;
			await _unitOfWork.GetRepository<Order>().UpdateAsync(order);
			await _unitOfWork.SaveAsync();

			return;
		}
		
    }
}

using AutoMapper;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.WebUI.ViewModels;

namespace MexicanRestaurant.Application.MappingProfiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<OrderItem, OrderItemViewModel>()
                .ForPath(dest => dest.Product.Name, opt => opt.MapFrom(src => src.Product.Name))
                .ForPath(dest => dest.Product.ImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl))
                .ReverseMap()
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.Ignore());

            CreateMap<Product, OrderItemViewModel>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.Quantity, opt => opt.Ignore());
        }
    }
}

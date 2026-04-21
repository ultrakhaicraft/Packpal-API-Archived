using AutoMapper;
using Packpal.DAL.Entity;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.MapperProfile
{
	public class StorageProfile : Profile
    {
        public StorageProfile()
        {
            CreateMap<Storage, CreateStorageModel>().ReverseMap();
            CreateMap<Storage, UpdateStorageModel>().ReverseMap();
            CreateMap<Storage, ViewStorageModel>().ReverseMap();
        }
    }
}

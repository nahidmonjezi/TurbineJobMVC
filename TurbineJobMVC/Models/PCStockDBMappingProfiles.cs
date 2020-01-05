﻿using AutoMapper;
using TurbineJobMVC.Models.Entites;
using TurbineJobMVC.Models.ViewModels;

namespace TurbineJobMVC.Models
{
    public class PCStockDBMappingProfiles : Profile
    {
        public PCStockDBMappingProfiles()
        {

            CreateMap<WorkOrderTBL, JobViewModel>()
                .ForMember(x => x.AR, opt => opt.MapFrom(x => x.Amval)).ReverseMap();
            CreateMap<WorkOrder, WorkOrderViewModel>().ReverseMap();
            CreateMap<TahvilForms, TahvilFormsViewModel>().ReverseMap();
        }
    }
}

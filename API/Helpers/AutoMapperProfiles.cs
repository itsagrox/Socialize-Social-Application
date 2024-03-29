using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.Extensions;
using API.Models;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser,MemberModel>()
             .ForMember(dest =>dest.PhotoUrl,opt => opt.MapFrom(src =>
                src.Photos.FirstOrDefault(x=>x.isMain).Url))
            .ForMember(dest =>dest.Age,opt => opt.MapFrom(src =>src.DateOfBirth.CalculateAge()));
            CreateMap<Photo,PhotoModel>();
        }
    }
}
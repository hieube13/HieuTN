using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Examination.Domain.AggregateModels.ExamAggregate;
using Examination.Dtos;

namespace Examination.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Exam, ExamDto>().ReverseMap();
        }
    }
}
using AutoMapper;
using Common.Dto;
using Core.Entities;

namespace Infrastructure.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Employee
            CreateMap<Employee, EmployeeDto>()
                .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department != null ? s.Department.NameEn : null));
            // PositionId, PositionName, GeneratedPassword, no matching source property on Employee, so AutoMapper leaves them

            CreateMap<EmployeeDto, Employee>()
                .ForMember(d => d.Username, o => o.Ignore())          // immutable after creation, and it's the FK to Users.Username
                .ForMember(d => d.Status, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.Status) ? "Active" : s.Status))
                .ForMember(d => d.ProfileImagePath, o => o.Ignore())  // only ever changed via the dedicated photo endpoints
                .ForMember(d => d.CreatedBy, o => o.Ignore())         // must never be overwritten
                .ForMember(d => d.CreationDate, o => o.Ignore());
                

            // Employee -> User 
            CreateMap<EmployeeDto, User>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.NameEn))
                .ForMember(d => d.Status, o => o.MapFrom(s => "Active")); // new accounts always start Active

            // Employee -> new EmployeePosition row (used when UpdateEmployeeAsync detects a position change)
            CreateMap<EmployeeDto, EmployeePosition>()
                .ForMember(d => d.EmployeeId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.ModifiedBy, o => o.Ignore())        // this is a brand-new row, dto.ModifiedBy belongs to the Employee's own edit history, not this record
                .ForMember(d => d.ModificationDate, o => o.Ignore());

            // Department
            CreateMap<Department, DepartmentDto>();
             
            CreateMap<DepartmentDto, Department>()
                .ForMember(d => d.Status, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.Status) ? "Active" : s.Status))
                .ForMember(d => d.CreatedBy, o => o.Ignore())
                .ForMember(d => d.CreationDate, o => o.Ignore());

            // Position
            CreateMap<Position, PositionDto>();

            CreateMap<PositionDto, Position>()
                .ForMember(d => d.CreatedBy, o => o.Ignore())
                .ForMember(d => d.CreationDate, o => o.Ignore());

            // EmployeePosition
            CreateMap<EmployeePosition, EmployeePositionDto>()
                .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee != null ? s.Employee.NameEn : null))
                .ForMember(d => d.PositionName, o => o.MapFrom(s => s.Position != null ? s.Position.NameEn : null));

            CreateMap<AssignPositionDto, EmployeePosition>();
   
        }
    }
}
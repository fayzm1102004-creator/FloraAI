using AutoMapper;
using FloraAI.API.Models.Entities;
using FloraAI.API.DTOs.User;
using FloraAI.API.DTOs.Conditions;
using FloraAI.API.DTOs.UserPlant;
using FloraAI.API.DTOs.ScanHistory;
using FloraAI.API.DTOs.PlantLookup;
using FloraAI.API.DTOs.Sync;

namespace FloraAI.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User Mappings
        CreateMap<User, UserResponseDto>();

        // ConditionsDictionary Mappings
        CreateMap<ConditionsDictionary, ConditionResponseDto>();

        // UserPlant Mappings
        CreateMap<UserPlant, UserPlantResponseDto>()
            .ForMember(dest => dest.ScanCount, opt => opt.MapFrom(src => src.ScanHistories.Count));

        // ScanHistory Mappings
        CreateMap<ScanHistory, ScanHistoryDto>();

        // PlantLookup Mappings
        CreateMap<PlantLookup, PlantLookupDto>();

        // Diagnosis Mappings
        CreateMap<ConditionsDictionary, FloraAI.API.DTOs.Diagnosis.DiagnosisScanResponseDto>()
            .ForMember(dest => dest.ConditionId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ScannedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsNewlyAdded, opt => opt.Ignore())
            .ForMember(dest => dest.CareAdvice, opt => opt.MapFrom(src => new FloraAI.API.DTOs.Diagnosis.CareAdviceDto
            {
                Watering = src.WateringAdvice ?? "غير متوفر",
                Light = src.LightAdvice ?? "غير متوفر",
                Fertilizing = src.FertilizingAdvice ?? "غير متوفر",
                Soil = src.SoilAdvice ?? "غير متوفر",
                Humidity = src.HumidityAdvice ?? "غير متوفر"
            }));

        // Sync Mappings
        CreateMap<ConditionsDictionary, SyncConditionDto>();
        CreateMap<ScanHistory, SyncDiagnosisResultDto>();
        CreateMap<FloraAI.API.DTOs.Diagnosis.DiagnosisScanResponseDto, SyncDiagnosisResultDto>()
            .ForMember(dest => dest.WateringAdvice, opt => opt.MapFrom(src => src.CareAdvice.Watering))
            .ForMember(dest => dest.LightAdvice, opt => opt.MapFrom(src => src.CareAdvice.Light))
            .ForMember(dest => dest.FertilizingAdvice, opt => opt.MapFrom(src => src.CareAdvice.Fertilizing))
            .ForMember(dest => dest.SoilAdvice, opt => opt.MapFrom(src => src.CareAdvice.Soil))
            .ForMember(dest => dest.HumidityAdvice, opt => opt.MapFrom(src => src.CareAdvice.Humidity));
        CreateMap<PendingScanDto, FloraAI.API.DTOs.Diagnosis.DiagnosisScanRequestDto>();
    }
}

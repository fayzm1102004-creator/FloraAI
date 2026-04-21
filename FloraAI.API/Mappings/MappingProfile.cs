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
        CreateMap<ConditionsDictionary, FloraAI.API.DTOs.Diagnosis.DiagnosisScanResponseDto>();

        // Sync Mappings
        CreateMap<ConditionsDictionary, SyncConditionDto>();
        CreateMap<ScanHistory, SyncDiagnosisResultDto>();
        CreateMap<FloraAI.API.DTOs.Diagnosis.DiagnosisScanResponseDto, SyncDiagnosisResultDto>();
        CreateMap<PendingScanDto, FloraAI.API.DTOs.Diagnosis.DiagnosisScanRequestDto>();
    }
}

using Mapster;
using TickTracker.Api.Contracts;
using TickTracker.Api.DTOs;
using TickTracker.Api.Entities;

namespace TickTracker.Api.Configurations;

public static class MapsterConfig
{
    public static void Register(TypeAdapterConfig cfg)
    {
        cfg.NewConfig<Asset, AssetsResponse>();
    }
}
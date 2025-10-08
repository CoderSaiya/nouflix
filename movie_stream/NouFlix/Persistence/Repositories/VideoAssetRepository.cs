using NouFlix.Models.Entities;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class VideoAssetRepository(AppDbContext db) : Repository<VideoAsset>(db), IVideoAssetRepository;
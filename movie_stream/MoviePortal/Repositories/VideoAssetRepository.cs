using MoviePortal.Data;
using MoviePortal.Models.Entities;
using MoviePortal.Repositories.Interfaces;

namespace MoviePortal.Repositories;

public class VideoAssetRepository(AppDbContext db) : Repository<VideoAsset>(db), IVideoAssetRepository;
using MoviePortal.Data;
using MoviePortal.Models.Entities;
using MoviePortal.Repositories.Interfaces;

namespace MoviePortal.Repositories;

public class ImageAssetRepository(AppDbContext db) : Repository<ImageAsset>(db), IImageAssetRepository;
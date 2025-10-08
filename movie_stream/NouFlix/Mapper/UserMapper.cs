using System.Diagnostics;
using NouFlix.DTOs;
using NouFlix.Models.Entities;
using NouFlix.Services;

namespace NouFlix.Mapper;

public static class UserMapper
{
    public static async Task<UserRes> ToUserResAsync(
        this User u,
        MinioObjectStorage storage,
        CancellationToken ct)
    {
        var img = u.Profile.Avatar;

        string? avatarUrl = null;
        if (img is not null) 
            avatarUrl = (await storage.GetReadSignedUrlAsync(
                img.Bucket,
                img.ObjectKey,
                TimeSpan.FromMinutes(10),
                ct)).ToString();
        
        
        return new UserRes(
            u.Id,
            u.Email.Address,
            u.Profile.Name?.FirstName ?? null,
            u.Profile.Name?.LastName ?? null,
            avatarUrl,
            Dob: u.Profile.DateOfBirth ?? null,
            MapRole(u),
            u.CreatedAt
        );
    }
    
    public static string MapRole(this User baseUser) => baseUser switch
    {
        Admin => "Admin",
        _ => "User"
    };
}
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using System.Reflection.Metadata;
using static System.Net.Mime.MediaTypeNames;

Console.WriteLine("Hello, World!");

using var db = new AppContext();

public class Video
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public int YearLaunched { get; private set; }
    public bool Opened { get; private set; }
    public bool Published { get; private set; }
    public int Duration { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Rating Rating { get; private set; }

    public Image? Thumb { get; private set; }
    public Image? ThumbHalf { get; private set; }
    public Image? Banner { get; private set; }

    public Media? Media { get; private set; }
    public Media? Trailer { get; private set; }

    private List<Guid> _categories;
    public IReadOnlyList<Guid> Categories => _categories.AsReadOnly();

    private List<Guid> _genres;
    public IReadOnlyList<Guid> Genres => _genres.AsReadOnly();

    private List<Guid> _castMembers;
    public IReadOnlyList<Guid> CastMembers => _castMembers.AsReadOnly();

    public Video(
        string title,
        string description,
        int yearLaunched,
        bool opened,
        bool published,
        int duration,
        Rating rating)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        YearLaunched = yearLaunched;
        Opened = opened;
        Published = published;
        Duration = duration;
        Rating = rating;

        _categories = new();
        _genres = new();
        _castMembers = new();

        CreatedAt = DateTime.Now;
    }

    public void Validate()
        => (new VideoValidator(this)).Validate();

    public void Update(
        string title,
        string description,
        int yearLaunched,
        bool opened,
        bool published,
        int duration,
        Rating? rating = null)
    {
        Title = title;
        Description = description;
        YearLaunched = yearLaunched;
        Opened = opened;
        Published = published;
        Duration = duration;
        if(rating is not null)
            Rating = (Rating)rating;
    }

    public void UpdateThumb(string path)
        => Thumb = new Image(path);

    public void UpdateThumbHalf(string path)
        => ThumbHalf = new Image(path);

    public void UpdateBanner(string path)
        => Banner = new Image(path);

    public void UpdateMedia(string path)
        => Media = new Media(path);

    public void UpdateTrailer(string path)
        => Trailer = new Media(path);

    public void UpdateAsSentToEncode()
    {
        if(Media is null)
            throw new Exception("There is no Media");
        Media.UpdateAsSentToEncode();
    }

    public void UpdateAsEncoded(string validEncodedPath)
    {
        if(Media is null)
            throw new Exception("There is no Media");
        Media.UpdateAsEncoded(validEncodedPath);
    }

    public void AddCategory(Guid categoryId)
        => _categories.Add(categoryId);

    public void RemoveCategory(Guid categoryId)
        => _categories.Remove(categoryId);

    public void RemoveAllCategories()
        => _categories = new();

    public void AddGenre(Guid genreId)
        => _genres.Add(genreId);

    public void RemoveGenre(Guid genreId)
        => _genres.Remove(genreId);

    public void RemoveAllGenres()
        => _genres = new();

    public void AddCastMember(Guid castMemberId)
        => _castMembers.Add(castMemberId);

    public void RemoveCastMember(Guid castMemberid)
        => _castMembers.Remove(castMemberid);

    public void RemoveAllCastMembers()
        => _castMembers = new();
}

public class Media
{
    public Guid Id { get; private set; }
    public string FilePath { get; private set; }
    public string? EncodedPath { get; private set; }
    public MediaStatus Status { get; private set; }

    public Media(string filePath)
    {
        Id = Guid.NewGuid();
        FilePath = filePath;
        Status = MediaStatus.Pending;
    }

    public void UpdateAsSentToEncode()
        => Status = MediaStatus.Processing;

    public void UpdateAsEncoded(string encodedExamplePath)
    {
        Status = MediaStatus.Completed;
        EncodedPath = encodedExamplePath;
    }
}

public enum MediaStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Error = 3
}

public enum Rating
{
    ER,
    L,
    Rate10,
    Rate12,
    Rate14,
    Rate16,
    Rate18
}

public class Image
{
    public string Path { get; }

    public Image(string path) => Path = path;

    public override bool Equals(object other) =>
        other is Image image &&
        Path == image.Path;
}

public class VideoValidator
{
    private readonly Video _video;

    private const int TitleMaxLength = 255;
    private const int DescriptionMaxLength = 4_000;

    public VideoValidator(Video video)
        => _video = video;

    public void Validate()
    {
        ValidateTitle();

        if(string.IsNullOrWhiteSpace(_video.Description))
            throw new Exception($"'{nameof(_video.Description)}' is required");

        if(_video.Description.Length > DescriptionMaxLength)
            throw new Exception($"'{nameof(_video.Description)}' should be less or equal {DescriptionMaxLength} characters long");
    }

    private void ValidateTitle()
    {
        if(string.IsNullOrWhiteSpace(_video.Title))
            throw new Exception($"'{nameof(_video.Title)}' is required");

        if(_video.Title.Length > 255)
            throw new Exception($"'{nameof(_video.Title)}' should be less or equal {TitleMaxLength} characters long");
    }
}



public class AppContext : DbContext
{
    public DbSet<Video> Employees { get; set; }

    public AppContext() : base()
    {
        Database.EnsureDeleted();
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
            .EnableSensitiveDataLogging()
            .UseSqlServer(@"Server=localhost;Database=test;User Id=sa;Password=Str0ngP455W0RD;trustServerCertificate=true;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Video>(p => {
            p.OwnsOne(x => x.Thumb, thumb => {
                thumb.Property(p => p.Path).HasColumnName("ThumbPath");
            });
            p.OwnsOne(x => x.ThumbHalf, thumb => {
                thumb.Property(p => p.Path).HasColumnName("ThumbHalfPath");
            });
            p.OwnsOne(x => x.Banner, thumb => {
                thumb.Property(p => p.Path).HasColumnName("BannerPath");
            });
        });
    }
}
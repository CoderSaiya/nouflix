namespace NouFlix.Models.ValueObject;

public record MovieSimWeights(
    double Director = 1.0,
    double Genre = 0.8,
    double Studio = 0.5,
    double Year = 0.3,
    double Language = 0.2,
    double Country = 0.1);
namespace SimpleMaps.MapObjects;

public abstract record MapObject
{
    public int RenderingOrder { get; init; } = 0;

    public object? Context { get; init; }
}

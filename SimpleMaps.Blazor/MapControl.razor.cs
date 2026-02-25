using Microsoft.AspNetCore.Components;
using Mapsui.Projections;
using SimpleMaps.Coordinates;
using SimpleMaps.MapEngine;

namespace SimpleMaps.Blazor;

public partial class MapControl : ComponentBase
{
    private Mapsui.UI.Blazor.MapControl? _mapControl;

    /// <summary>
    /// The SimpleMaps map engine to render.
    /// </summary>
    [Parameter]
    public IMapEngine? MapEngine { get; set; }

    /// <summary>
    /// Raised when the user clicks a location on the map.
    /// </summary>
    [Parameter]
    public EventCallback<MapEventArgs> MapClicked { get; set; }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (firstRender && _mapControl is not null && MapEngine is not null)
        {
#pragma warning disable BL0005 // Setting component parameter directly is required for Mapsui MapControl integration
            _mapControl.Map = (Mapsui.Map)MapEngine.NativeMap;
#pragma warning restore BL0005

            _mapControl.Map.Info += OnMapInfo;
        }
    }

    private void OnMapInfo(object? sender, Mapsui.MapInfoEventArgs e)
    {
        if (e.WorldPosition is not null && MapClicked.HasDelegate)
        {
            var (lon, lat) = SphericalMercator.ToLonLat(e.WorldPosition.X, e.WorldPosition.Y);
            var coordinate = new WGS84Coordinate(lat, lon);
            MapClicked.InvokeAsync(new MapEventArgs(coordinate));
        }

        e.Handled = true;
    }
}

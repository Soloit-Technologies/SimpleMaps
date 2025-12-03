using SimpleMaps.MapEngine;
using SimpleMaps.MapEngine.Implementations.Mapsui;
using SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;

namespace SimpleMaps.Maui;

public partial class MapControl : ContentView
{
	public static readonly BindableProperty MapEngineProperty =
		BindableProperty.Create(
			nameof(MapEngine),
			typeof(IMapEngine),
			typeof(MapControl),
			null,
			BindingMode.TwoWay,
			propertyChanged: OnMapEngineChanged);

	public IMapEngine? MapEngine
	{
		get => (IMapEngine?)GetValue(MapEngineProperty);
		set => SetValue(MapEngineProperty, value);
	}

	/// <summary>
	/// Raised when the user presses a pointer (mouse, touch, pen) on the map.
	/// </summary>
	public event EventHandler<MapEventArgs>? MapPointerPressed;

	public MapControl()
	{
		InitializeComponent();
	}

	private static void OnMapEngineChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is not MapControl mapControl)
			return;

		if (newValue is MapsuiMapEngine mapsuiEngine)
		{
			// Successfully cast to MapsuiMapEngine
			mapControl.BindToMapsuiEngine(mapsuiEngine);
		}
		else if (newValue is not null)
		{
			// Value is IMapEngine but not MapsuiMapEngine
			System.Diagnostics.Debug.WriteLine($"Warning: MapEngine is {newValue.GetType().Name}, expected MapsuiMapEngine");
		}
	}

	private void BindToMapsuiEngine(MapsuiMapEngine mapsuiEngine)
	{
		// Get the Mapsui.MapControl from the XAML
		var mapsuiControl = this.FindByName<Mapsui.UI.Maui.MapControl>("mapControl");

        // Bind the Mapsui Map to the MapControl
        if (mapsuiControl is not null)
        {            
            // Subscribe to the Mapsui pointer press event
            mapsuiControl.MapPointerPressed += OnMapsuiPointerPressed;
        }
    }

	private void OnMapsuiPointerPressed(object? sender, Mapsui.MapEventArgs e)
	{
		var args = new MapEventArgs(e.WorldPosition.ToCoordinate());

		MapPointerPressed?.Invoke(this, args);
	}
}
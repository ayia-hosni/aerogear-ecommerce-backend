namespace SportEcommerce.Domain.Entities;

/// <summary>A colourway. The catalogue only has one photo per product, so a colour is that photo with its hue rotated.</summary>
public sealed record ProductColor(string Name, string Hex, int HueShift);

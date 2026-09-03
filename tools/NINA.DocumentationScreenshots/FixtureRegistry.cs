#region "copyright"

/*
    Copyright (c) 2016 - 2026 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NINA.DocumentationScreenshots;

public sealed class FixtureRegistry {
    private readonly IReadOnlyDictionary<string, Func<ScreenshotAsset, FrameworkElement>> fixtures;

    public FixtureRegistry() {
        fixtures = new Dictionary<string, Func<ScreenshotAsset, FrameworkElement>>(StringComparer.OrdinalIgnoreCase) {
            ["view"] = DocumentationApplicationHost.Instance.CreateProductionView,
            ["application"] = DocumentationApplicationHost.Instance.CreateApplicationView,
            ["sequencer"] = DocumentationApplicationHost.Instance.CreateAdvancedSequencer,
            ["sequencer-entity"] = DocumentationApplicationHost.Instance.CreateSequencerEntity,
            ["autofocus-chart"] = DocumentationApplicationHost.Instance.CreateAutoFocusChart,
            ["guider-chart"] = DocumentationApplicationHost.Instance.CreateProductionView,
            ["framing-assistant"] = DocumentationApplicationHost.Instance.CreateProductionView,
            ["settings-group"] = DocumentationApplicationHost.Instance.CreateSettingsGroup,
            ["resource-icon"] = CreateResourceIcon
        };
    }

    public bool Contains(string fixture) => fixtures.ContainsKey(fixture);

    public FrameworkElement Create(ScreenshotAsset asset) {
        if (asset.Fixture is null || !fixtures.TryGetValue(asset.Fixture, out Func<ScreenshotAsset, FrameworkElement>? fixture)) {
            throw new CatalogException($"Screenshot '{asset.Id}' refers to unknown fixture '{asset.Fixture}'.");
        }
        return fixture(asset);
    }

    private static FrameworkElement CreateViewWithFallback(ScreenshotAsset asset, string fallback) {
        string viewType = !string.IsNullOrWhiteSpace(asset.ViewType)
            ? asset.ViewType
            : asset.State?.StartsWith("NINA.", StringComparison.Ordinal) == true ? asset.State : fallback;
        return InstantiateView(viewType, asset.Id);
    }

    private static FrameworkElement CreateView(ScreenshotAsset asset) {
        string? viewType = asset.ViewType ?? asset.State;
        if (string.IsNullOrWhiteSpace(viewType)) {
            throw new CatalogException($"Screenshot '{asset.Id}' must specify the view type in 'state'.");
        }
        return InstantiateView(viewType, asset.Id);
    }

    private static Brush ResourceBrush(string key, Brush fallback) => Application.Current?.TryFindResource(key) as Brush ?? fallback;

    private static FrameworkElement CreateResourceIcon(ScreenshotAsset asset) {
        string? resourceKey = asset.Icon ?? asset.State;
        if (string.IsNullOrWhiteSpace(resourceKey)) {
            throw new CatalogException($"Screenshot '{asset.Id}' must specify the geometry resource key in 'state'.");
        }

        object? resource = Application.Current?.TryFindResource(resourceKey);
        if (resource is not Geometry geometry) {
            throw new CatalogException($"Screenshot '{asset.Id}' refers to an unavailable geometry resource '{resourceKey}'.");
        }

        Brush foreground = Application.Current?.TryFindResource("ButtonForegroundBrush") as Brush ?? Brushes.White;
        return new Viewbox {
            Margin = new Thickness(Math.Max(2, Math.Min(asset.Width, asset.Height) * 0.08)),
            Stretch = Stretch.Uniform,
            Child = new System.Windows.Shapes.Path {
                Data = geometry,
                Fill = foreground,
                Stretch = Stretch.Uniform
            }
        };
    }

    private static FrameworkElement InstantiateView(string typeName, string screenshotId) {
        Type? type = Type.GetType($"{typeName}, NINA", throwOnError: false, ignoreCase: false);
        if (type is null || !typeof(FrameworkElement).IsAssignableFrom(type)) {
            throw new CatalogException($"Screenshot '{screenshotId}' refers to an unavailable NINA view '{typeName}'.");
        }

        try {
            FrameworkElement element = (FrameworkElement)(Activator.CreateInstance(type) ?? throw new InvalidOperationException("The constructor returned null."));
            if (element.DataContext is null && Application.Current?.Resources["ProfileService"] is object profileService) {
                element.DataContext = profileService;
            }
            return element;
        } catch (Exception ex) {
            throw new CatalogException($"Screenshot '{screenshotId}' could not construct '{typeName}': {ex.GetBaseException().Message}");
        }
    }
}

public static class ScreenshotChrome {
    public static FrameworkElement Wrap(
        FrameworkElement content,
        int width,
        int height,
        IReadOnlyList<ScreenshotCallout> callouts) {
        Brush background = Application.Current?.TryFindResource("BackgroundBrush") as Brush
            ?? throw new CatalogException("NINA's production BackgroundBrush resource is unavailable.");
        Grid root = new() {
            Width = width,
            Height = height,
            Background = background,
            ClipToBounds = true
        };
        root.Children.Add(content);

        if (callouts.Count > 0) {
            Canvas overlay = new() { IsHitTestVisible = false };
            foreach (ScreenshotCallout callout in callouts) {
                switch (callout.Kind) {
                    case ScreenshotCalloutKind.Arrow:
                        AddArrow(overlay, callout, width, height);
                        break;
                    case ScreenshotCalloutKind.Label:
                        AddLabel(overlay, callout, width, height);
                        break;
                    case ScreenshotCalloutKind.Box:
                        AddBox(overlay, callout, width, height);
                        break;
                    default:
                        AddBadge(overlay, callout, width, height);
                        break;
                }
            }
            root.Children.Add(overlay);
        }
        return root;
    }

    private static void AddBadge(Canvas overlay, ScreenshotCallout callout, int width, int height) {
        Border badge = new() {
            MinWidth = 28,
            MinHeight = 28,
            Padding = new Thickness(7, 3, 7, 3),
            CornerRadius = new CornerRadius(14),
            BorderThickness = new Thickness(2),
            BorderBrush = Brushes.White,
            Background = new SolidColorBrush(Color.FromRgb(196, 45, 45)),
            Child = new TextBlock {
                Text = callout.Text,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        Canvas.SetLeft(badge, Math.Clamp(callout.X * width - 14, 0, width - 28));
        Canvas.SetTop(badge, Math.Clamp(callout.Y * height - 14, 0, height - 28));
        overlay.Children.Add(badge);
    }

    private static void AddLabel(Canvas overlay, ScreenshotCallout callout, int width, int height) {
        Brush annotationBrush = new SolidColorBrush(Color.FromRgb(255, 45, 45));
        Border label = new() {
            Width = callout.Width!.Value * width,
            Padding = new Thickness(8, 4, 8, 4),
            BorderThickness = new Thickness(2),
            BorderBrush = annotationBrush,
            Background = new SolidColorBrush(Color.FromArgb(224, 18, 18, 18)),
            Child = new TextBlock {
                Text = callout.Text,
                Foreground = annotationBrush,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap
            }
        };
        label.Measure(new Size(width, height));
        Canvas.SetLeft(label, Math.Clamp(callout.X * width, 0, Math.Max(0, width - label.DesiredSize.Width)));
        Canvas.SetTop(label, Math.Clamp(callout.Y * height, 0, Math.Max(0, height - label.DesiredSize.Height)));
        overlay.Children.Add(label);
    }

    private static void AddBox(Canvas overlay, ScreenshotCallout callout, int width, int height) {
        System.Windows.Shapes.Rectangle box = new() {
            Width = callout.Width!.Value * width,
            Height = callout.Height!.Value * height,
            Stroke = new SolidColorBrush(Color.FromRgb(255, 45, 45)),
            StrokeThickness = 3
        };
        Canvas.SetLeft(box, callout.X * width);
        Canvas.SetTop(box, callout.Y * height);
        overlay.Children.Add(box);
    }

    private static void AddArrow(Canvas overlay, ScreenshotCallout callout, int width, int height) {
        Brush annotationBrush = new SolidColorBrush(Color.FromRgb(255, 45, 45));
        PointCollection points = new(callout.Points.Select(point => new Point(point.X * width, point.Y * height)));
        System.Windows.Shapes.Polyline line = new() {
            Points = points,
            Stroke = annotationBrush,
            StrokeThickness = 3,
            StrokeLineJoin = PenLineJoin.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };
        overlay.Children.Add(line);

        Point end = points[^1];
        Point previous = points.Reverse().Skip(1).First(point => point != end);
        Vector direction = end - previous;
        direction.Normalize();
        Vector perpendicular = new(-direction.Y, direction.X);
        const double arrowLength = 12;
        const double arrowHalfWidth = 6;
        Point arrowBase = end - direction * arrowLength;
        System.Windows.Shapes.Polygon arrowHead = new() {
            Fill = annotationBrush,
            Points = new PointCollection {
                end,
                arrowBase + perpendicular * arrowHalfWidth,
                arrowBase - perpendicular * arrowHalfWidth
            }
        };
        overlay.Children.Add(arrowHead);
    }
}

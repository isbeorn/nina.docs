#region "copyright"

/*
    Copyright (c) 2016 - 2026 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

namespace NINA.DocumentationScreenshots;

internal static class DocumentationAstronomy {
    internal static DateTime ReferenceDate { get; } =
        new(2026, 8, 31, 12, 0, 0, DateTimeKind.Local);

    internal const double Latitude = 52.52;
    internal const double Longitude = 13.405;
    internal const double Elevation = 34;

    internal static void AlignAltitudeChart(
            NINA.Sequencer.Container.IDeepSkyObjectContainer target) {
        target.Target.SetPosition(
            NINA.Astrometry.Angle.ByDegree(Latitude),
            NINA.Astrometry.Angle.ByDegree(Longitude));
        target.Target.DeepSkyObject.SetDateAndPosition(
            ReferenceDate,
            Latitude,
            Longitude);
    }
}

#region "copyright"

/*
    Copyright (c) 2016 - 2026 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Equipment.Interfaces;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.ViewModel.Equipment;

namespace NINA.DocumentationScreenshots;

internal sealed class DocumentationDeviceChooserVM<T> : DeviceChooserVM<T> where T : IDevice {
    public DocumentationDeviceChooserVM(
            IProfileService profileService,
            T device,
            IEquipmentProviders<T> equipmentProviders)
        : base(profileService, equipmentProviders) {
        Devices = [device];
        SelectedDevice = device;
    }

    public override Task GetEquipment() => Task.CompletedTask;
}

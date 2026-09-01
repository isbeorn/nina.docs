#region "copyright"

/*
    Copyright (c) 2016 - 2026 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Sequencer.Conditions;
using NINA.Sequencer.Container;
using NINA.Sequencer.SequenceItem.Camera;
using NINA.Sequencer.SequenceItem.Dome;
using NINA.Sequencer.SequenceItem.FilterWheel;
using NINA.Sequencer.SequenceItem.Expressions;
using NINA.Sequencer.SequenceItem.FlatDevice;
using NINA.Sequencer.SequenceItem.Imaging;
using NINA.Sequencer.SequenceItem.Telescope;
using NINA.Sequencer.SequenceItem.Utility;
using NINA.Sequencer.Trigger;
using NINA.Sequencer.Trigger.Autofocus;
using NINA.Sequencer.Trigger.Guider;
using NINA.ViewModel.Sequencer;

namespace NINA.DocumentationScreenshots;

/// <summary>
/// Builds deterministic documentation examples with NINA's production sequencer entities.
/// It changes model data only and does not create screenshot-specific visual elements.
/// </summary>
internal static class SequencerFixtureState {
    public static void Apply(ISequence2VM viewModel, ScreenshotAsset asset) {
        string state = Normalize(asset.State + " " + asset.Output);
        if (state.Contains("sequencerflow", StringComparison.Ordinal)) {
            BuildFlow(viewModel);
        } else if (state.Contains("sequencersequentialinstructions", StringComparison.Ordinal)) {
            BuildSequentialInstructions(viewModel);
        } else if (state.Contains("sequencerparallelinstructions", StringComparison.Ordinal)) {
            BuildParallelInstructions(viewModel);
        } else if (state.Contains("sequencernestedconditions", StringComparison.Ordinal)) {
            BuildNestedConditions(viewModel);
        } else if (state.Contains("sequencernestedtriggers", StringComparison.Ordinal)) {
            BuildNestedTriggers(viewModel);
        } else if (state.Contains("sequencerloopconditions", StringComparison.Ordinal)) {
            BuildLoopConditions(viewModel);
        } else if (state.Contains("sequencertriggers", StringComparison.Ordinal)) {
            BuildTriggers(viewModel);
        } else if (state.Contains("sequencerdsoset", StringComparison.Ordinal)) {
            BuildDeepSkyObject(viewModel);
        } else if (state.Contains("sequencerinstructionsdetails", StringComparison.Ordinal)) {
            BuildInstructionDetails(viewModel);
        } else if (state.Contains("sequencerissues", StringComparison.Ordinal)) {
            BuildIssues(viewModel);
        } else if (state.Contains("sequencerdefineconstant", StringComparison.Ordinal)) {
            BuildDefinedConstants(viewModel);
        } else if (state.Contains("sequencerdefinevariable", StringComparison.Ordinal)) {
            BuildDefinedSymbols(viewModel);
        } else if (state.Contains("sequencerapplytarget", StringComparison.Ordinal)
            || state.Contains("sequenceraddtargettotargettab", StringComparison.Ordinal)
            || state.Contains("sequencerdroptargettotab", StringComparison.Ordinal)) {
            BuildTargetWorkflow(viewModel);
        } else if (state.Contains("sequencersaveastemplate", StringComparison.Ordinal)) {
            TargetArea(viewModel).Add(CreateRgbLoop(viewModel));
        } else if (state.Contains("sequencerdragdrop", StringComparison.Ordinal)) {
            StartArea(viewModel).Add(NewContainer<SequentialContainer>(viewModel, "Startup instructions"));
        } else if (state.Contains("sequenceraddtrigger", StringComparison.Ordinal)
            || state.Contains("sequenceraddloopcondition", StringComparison.Ordinal)) {
            TargetArea(viewModel).Add(NewContainer<SequentialContainer>(viewModel, "Imaging instructions"));
        }
    }

    private static void BuildFlow(ISequence2VM viewModel) {
        ISequenceContainer targetArea = (ISequenceContainer)viewModel.Sequencer.MainContainer.Items[1];
        AddWaits(viewModel, targetArea, 3);

        SequentialContainer repeated = viewModel.SequencerFactory.GetContainer<SequentialContainer>();
        repeated.Name = "Repeat captures";
        LoopCondition loop = viewModel.SequencerFactory.GetCondition<LoopCondition>();
        loop.Iterations = 5;
        ((IConditionable)repeated).Add(loop);
        AddWaits(viewModel, repeated, 2);
        targetArea.Add(repeated);

        AddWaits(viewModel, targetArea, 2);
    }

    private static void AddWaits(ISequence2VM viewModel, ISequenceContainer container, int count) {
        for (int index = 0; index < count; index++) {
            WaitForTimeSpan wait = viewModel.SequencerFactory.GetItem<WaitForTimeSpan>();
            wait.Time = index + 1;
            container.Add(wait);
        }
    }

    private static void BuildSequentialInstructions(ISequence2VM viewModel) {
        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, "RGB exposures");
        foreach (string filterName in new[] { "R", "G", "B" }) {
            SwitchFilter filter = viewModel.SequencerFactory.GetItem<SwitchFilter>();
            filter.ComboBoxText = filterName;
            sequence.Add(filter);
            sequence.Add(NewExposure(viewModel, 10));
        }
        TargetArea(viewModel).Add(sequence);
    }

    private static void BuildParallelInstructions(ISequence2VM viewModel) {
        ParallelContainer sequence = NewContainer<ParallelContainer>(viewModel, "Open observatory");
        sequence.Add(viewModel.SequencerFactory.GetItem<CoolCamera>());
        sequence.Add(viewModel.SequencerFactory.GetItem<UnparkScope>());
        sequence.Add(viewModel.SequencerFactory.GetItem<OpenDomeShutter>());
        sequence.Add(viewModel.SequencerFactory.GetItem<OpenCover>());
        TargetArea(viewModel).Add(sequence);
    }

    private static void BuildLoopConditions(ISequence2VM viewModel) {
        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, "RGB until dawn");
        TimeCondition until = viewModel.SequencerFactory.GetCondition<TimeCondition>();
        until.DateTime = DocumentationApplicationHost.FixedDateTime;
        until.SelectedProvider = until.DateTimeProviders.First(provider => provider.Name == "Astronomical Dawn");
        ((IConditionable)sequence).Add(until);
        foreach (string filterName in new[] { "R", "G", "B" }) {
            SwitchFilter filter = viewModel.SequencerFactory.GetItem<SwitchFilter>();
            filter.ComboBoxText = filterName;
            sequence.Add(filter);
            sequence.Add(NewExposure(viewModel, 10));
        }
        TargetArea(viewModel).Add(sequence);
    }

    private static void BuildNestedConditions(ISequence2VM viewModel) {
        SequentialContainer outer = NewContainer<SequentialContainer>(viewModel, "Image until 23:30");
        TimeCondition until = viewModel.SequencerFactory.GetCondition<TimeCondition>();
        until.DateTime = DocumentationApplicationHost.FixedDateTime;
        until.SelectedProvider = until.DateTimeProviders.First(provider => provider.Name == "Time");
        until.Hours = 23;
        until.Minutes = 30;
        until.Seconds = 0;
        ((IConditionable)outer).Add(until);
        outer.Add(ExposureLoop(viewModel, "Take 2 x 10s exposures", 2, 10));
        outer.Add(ExposureLoop(viewModel, "Take 3 x 30s exposures", 3, 30));
        TargetArea(viewModel).Add(outer);
    }

    private static void BuildTriggers(ISequence2VM viewModel) {
        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, "RGB imaging");
        AutofocusAfterExposures autofocus = viewModel.SequencerFactory.GetTrigger<AutofocusAfterExposures>();
        autofocus.AfterExposures = 5;
        ((ITriggerable)sequence).Add(autofocus);
        DitherAfterExposures dither = viewModel.SequencerFactory.GetTrigger<DitherAfterExposures>();
        dither.AfterExposures = 2;
        ((ITriggerable)sequence).Add(dither);
        foreach (string filterName in new[] { "R", "G", "B" }) {
            SwitchFilter filter = viewModel.SequencerFactory.GetItem<SwitchFilter>();
            filter.ComboBoxText = filterName;
            sequence.Add(filter);
            sequence.Add(NewExposure(viewModel, 10));
        }
        TargetArea(viewModel).Add(sequence);
    }

    private static void BuildNestedTriggers(ISequence2VM viewModel) {
        SequentialContainer outer = NewContainer<SequentialContainer>(viewModel, "Autofocus across nested sets");
        AutofocusAfterExposures autofocus = viewModel.SequencerFactory.GetTrigger<AutofocusAfterExposures>();
        autofocus.AfterExposures = 5;
        ((ITriggerable)outer).Add(autofocus);
        outer.Add(ExposureLoop(viewModel, "Take 20 x 10s exposures", 20, 10));
        outer.Add(ExposureLoop(viewModel, "Take 5 x 20s exposures", 5, 20));
        TargetArea(viewModel).Add(outer);
    }

    private static void BuildDeepSkyObject(ISequence2VM viewModel) {
        DeepSkyObjectContainer target = NewDeepSkyObject(viewModel);
        TargetArea(viewModel).Add(target);
    }

    private static void BuildTargetWorkflow(ISequence2VM viewModel) {
        DeepSkyObjectContainer target = NewDeepSkyObject(viewModel);
        SwitchFilter filter = viewModel.SequencerFactory.GetItem<SwitchFilter>();
        filter.ComboBoxText = "L";
        target.Add(filter);
        target.Add(NewExposure(viewModel, 60));
        TargetArea(viewModel).Add(target);
    }

    private static DeepSkyObjectContainer NewDeepSkyObject(ISequence2VM viewModel) {
        DeepSkyObjectContainer target = viewModel.SequencerFactory.GetContainer<DeepSkyObjectContainer>();
        target.Name = "Triangulum Pinwheel";
        target.Target.TargetName = "Triangulum Pinwheel";
        target.Target.InputCoordinates.Coordinates = new NINA.Astrometry.Coordinates(
            NINA.Astrometry.Angle.ByHours(1.564),
            NINA.Astrometry.Angle.ByDegree(30.66),
            NINA.Astrometry.Epoch.J2000);
        target.Target.PositionAngle = 0;
        return target;
    }

    private static void BuildInstructionDetails(ISequence2VM viewModel) {
        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, "Instruction details");
        CoolCamera cool = viewModel.SequencerFactory.GetItem<CoolCamera>();
        cool.Temperature = -10;
        cool.Duration = 0;
        sequence.Add(cool);

        WaitForTime wait = viewModel.SequencerFactory.GetItem<WaitForTime>();
        wait.SelectedProvider = wait.DateTimeProviders.First(provider => provider.Name == "Time");
        wait.Hours = 20;
        wait.Minutes = 16;
        sequence.Add(wait);

        SwitchFilter filter = viewModel.SequencerFactory.GetItem<SwitchFilter>();
        filter.ComboBoxText = "L";
        sequence.Add(filter);
        sequence.Add(NewExposure(viewModel, 10));
        TargetArea(viewModel).Add(sequence);
    }

    private static void BuildIssues(ISequence2VM viewModel) {
        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, "Validation example");
        List<TakeExposure> exposures = [];
        for (int index = 0; index < 6; index++) {
            TakeExposure exposure = NewExposure(viewModel, 10 + index * 5);
            exposure.Gain = 500 + index;
            exposure.Offset = 500 + index;
            exposures.Add(exposure);
            sequence.Add(exposure);
        }
        TargetArea(viewModel).Add(sequence);
        foreach (TakeExposure exposure in exposures) {
            exposure.Validate();
        }
    }

    private static void BuildDefinedSymbols(ISequence2VM viewModel) {
        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, "Defined symbols");
        GlobalVariable lastFilter = viewModel.SequencerFactory.GetItem<GlobalVariable>();
        GlobalVariable maxAltitude = viewModel.SequencerFactory.GetItem<GlobalVariable>();
        sequence.Add(lastFilter);
        sequence.Add(maxAltitude);
        TargetArea(viewModel).Add(sequence);
        ConfigureVariable(lastFilter, "LastFilter", "'ASKAR_D2'");
        ConfigureVariable(maxAltitude, "MaxAltitude", "80");
    }

    private static void BuildDefinedConstants(ISequence2VM viewModel) {
        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, "Defined constants");
        GlobalConstant exposureTime = NewConstant(viewModel, sequence, "ExposureTime", "60");
        GlobalConstant targetTemperature = NewConstant(viewModel, sequence, "TargetTemp", "-15");
        sequence.Add(exposureTime);
        sequence.Add(targetTemperature);
        TargetArea(viewModel).Add(sequence);
        exposureTime.Validate();
        targetTemperature.Validate();
    }

    private static GlobalConstant NewConstant(
            ISequence2VM viewModel,
            ISequenceContainer context,
            string identifier,
            string definition) {
        GlobalConstant constant = viewModel.SequencerFactory.GetItem<GlobalConstant>();
        constant.Identifier = identifier;
        constant.Expr = new NINA.Sequencer.Logic.Expression(definition, context, constant);
        constant.Expr.Evaluate();
        return constant;
    }

    internal static SequentialContainer CreateRgbLoop(ISequence2VM viewModel) {
        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, "RGB Loop");
        AutofocusAfterHFRIncreaseTrigger autofocus = viewModel.SequencerFactory.GetTrigger<AutofocusAfterHFRIncreaseTrigger>();
        autofocus.Amount = 5;
        ((ITriggerable)sequence).Add(autofocus);

        TimeCondition until = viewModel.SequencerFactory.GetCondition<TimeCondition>();
        until.DateTime = DocumentationApplicationHost.FixedDateTime;
        until.SelectedProvider = until.DateTimeProviders.First(provider => provider.Name == "Time");
        until.Hours = 23;
        until.Minutes = 30;
        until.Seconds = 0;
        ((IConditionable)sequence).Add(until);

        foreach (string filterName in new[] { "R", "G", "B" }) {
            SwitchFilter filter = viewModel.SequencerFactory.GetItem<SwitchFilter>();
            filter.ComboBoxText = filterName;
            sequence.Add(filter);
            sequence.Add(NewExposure(viewModel, 10));
        }
        return sequence;
    }

    private static void ConfigureVariable(Variable variable, string identifier, string definition) {
        variable.Identifier = identifier;
        variable.OriginalDefinition = definition;
        variable.Expr.Definition = definition;
        variable.Executed = true;
        variable.Expr.Evaluate();
        variable.Validate();
    }

    private static SequentialContainer ExposureLoop(
            ISequence2VM viewModel,
            string name,
            int iterations,
            double exposureSeconds) {
        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, name);
        LoopCondition loop = viewModel.SequencerFactory.GetCondition<LoopCondition>();
        loop.Iterations = iterations;
        ((IConditionable)sequence).Add(loop);
        sequence.Add(NewExposure(viewModel, exposureSeconds));
        return sequence;
    }

    private static TakeExposure NewExposure(ISequence2VM viewModel, double seconds) {
        TakeExposure exposure = viewModel.SequencerFactory.GetItem<TakeExposure>();
        exposure.ExposureTime = seconds;
        exposure.Gain = 50;
        exposure.Offset = 25;
        exposure.ImageType = "LIGHT";
        return exposure;
    }

    private static T NewContainer<T>(ISequence2VM viewModel, string name) where T : ISequenceContainer {
        T container = viewModel.SequencerFactory.GetContainer<T>();
        container.Name = name;
        return container;
    }

    private static ISequenceContainer TargetArea(ISequence2VM viewModel) =>
        (ISequenceContainer)viewModel.Sequencer.MainContainer.Items[1];

    private static ISequenceContainer StartArea(ISequence2VM viewModel) =>
        (ISequenceContainer)viewModel.Sequencer.MainContainer.Items[0];

    private static string Normalize(string value) =>
        new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
}

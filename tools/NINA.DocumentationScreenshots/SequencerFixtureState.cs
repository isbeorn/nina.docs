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
using NINA.Sequencer.Logic;
using NINA.Sequencer.SequenceItem.Camera;
using NINA.Sequencer.SequenceItem.Dome;
using NINA.Sequencer.SequenceItem.FilterWheel;
using NINA.Sequencer.SequenceItem.Expressions;
using NINA.Sequencer.SequenceItem.FlatDevice;
using NINA.Sequencer.SequenceItem.Autofocus;
using NINA.Sequencer.SequenceItem.Guider;
using NINA.Sequencer.SequenceItem.Imaging;
using NINA.Sequencer.SequenceItem.Platesolving;
using NINA.Sequencer.SequenceItem.Telescope;
using NINA.Sequencer.SequenceItem.Utility;
using NINA.Sequencer.Trigger;
using NINA.Sequencer.Trigger.Autofocus;
using NINA.Sequencer.Trigger.Guider;
using NINA.Sequencer.Trigger.MeridianFlip;
using NINA.Sequencer.Trigger.SafetyMonitor;
using NINA.Sequencer.Trigger.Utility;
using NINA.ViewModel.Sequencer;

namespace NINA.DocumentationScreenshots;

/// <summary>
/// Builds deterministic documentation examples with NINA's production sequencer entities.
/// It changes model data only and does not create screenshot-specific visual elements.
/// </summary>
internal static class SequencerFixtureState {
    public static void Apply(ISequence2VM viewModel, ScreenshotAsset asset, ISymbolBroker symbolBroker) {
        string state = Normalize(asset.State + " " + asset.Output);
        if (state.Contains("sequencerflow", StringComparison.Ordinal)) {
            BuildFlow(viewModel);
        } else if (state.Contains("simpletoadvanced", StringComparison.Ordinal)) {
            BuildSimpleToAdvanced(viewModel, state, symbolBroker);
        } else if (state.Contains("sequencersidebarloopuntiltime", StringComparison.Ordinal)) {
            BuildFilteredSidebar(viewModel, "Loop Until Time", false);
        } else if (state.Contains("sequencersidebarafaftertime", StringComparison.Ordinal)) {
            BuildFilteredSidebar(viewModel, "AF After Time", false);
        } else if (state.Contains("sequencersidebarsettings", StringComparison.Ordinal)) {
            BuildFilteredSidebar(viewModel, string.Empty, true);
        } else if (state.Contains("instructionsettings", StringComparison.Ordinal)) {
            BuildInstructionSettings(viewModel);
        } else if (state.Contains("sequencerexpressionwarninggone", StringComparison.Ordinal)) {
            BuildExpressionVariableExample(viewModel, symbolBroker, true);
        } else if (state.Contains("sequencerexpressionwarning", StringComparison.Ordinal)) {
            BuildExpressionVariableExample(viewModel, symbolBroker, false);
        } else if (ContainsAny(
            state,
            "sequencerexpressionexample",
            "sequencersymbolexampleinstruction",
            "sequencerexpressionvalue",
            "sequencersymbolvalues",
            "sequencerundefined")) {
            BuildExpressionExample(viewModel, symbolBroker);
        } else if (state.Contains("sequencerslewtoradec", StringComparison.Ordinal)) {
            BuildSlewToRaDec(viewModel);
        } else if (ContainsAny(
            state,
            "instructiongeneric",
            "instructionname",
            "instructionoptions",
            "instructionbuttons",
            "instructionvalidation")) {
            BuildInstructionAnatomy(viewModel, state);
        } else if (state.Contains("customtrigger", StringComparison.Ordinal)) {
            BuildTriggerExample<CustomTrigger>(viewModel, "Custom trigger example");
        } else if (state.Contains("triggeronunsafe", StringComparison.Ordinal)) {
            BuildTriggerExample<TriggerOnUnsafe>(viewModel, "Safety trigger example");
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

    private static void BuildFilteredSidebar(
            ISequence2VM viewModel,
            string filter,
            bool settingsMode) {
        viewModel.SequencerFactory.ViewFilter = filter;
        if (viewModel.SequencerFactory is not NINA.Sequencer.SequencerFactory factory) {
            throw new InvalidOperationException("The documentation fixture requires NINA's production SequencerFactory.");
        }
        factory.SettingsMode = settingsMode;
    }

    private static void BuildInstructionSettings(ISequence2VM viewModel) {
        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, "Instruction settings example");
        TakeExposure exposure = NewExposure(viewModel, 180);
        exposure.Attempts = 3;
        exposure.ErrorBehavior = NINA.Sequencer.Utility.InstructionErrorBehavior.SkipInstructionSetOnError;
        exposure.ShowMenu = true;
        sequence.Add(exposure);
        TargetArea(viewModel).Add(sequence);
    }

    private static void BuildExpressionExample(
            ISequence2VM viewModel,
            ISymbolBroker symbolBroker) {
        SequentialContainer constants = NewContainer<SequentialContainer>(viewModel, "Expression constants");
        StartArea(viewModel).Add(constants);
        AddConstant(viewModel, constants, symbolBroker, "ExposureTime", "60");
        AddConstant(viewModel, constants, symbolBroker, "FudgeFactor", "-15");

        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, "Expression example");
        TargetArea(viewModel).Add(sequence);

        TakeExposure exposure = NewExposure(viewModel, 60);
        sequence.Add(exposure);
        ConfigureExpression(exposure.ExposureTimeExpression, symbolBroker, "ExposureTime + FudgeFactor");
        ConfigureExpression(exposure.GainExpression, symbolBroker, "CameraGain");
        exposure.Validate();
    }

    private static void BuildExpressionVariableExample(
            ISequence2VM viewModel,
            ISymbolBroker symbolBroker,
            bool executed) {
        SequentialContainer constants = NewContainer<SequentialContainer>(viewModel, "Expression constants");
        StartArea(viewModel).Add(constants);
        AddConstant(viewModel, constants, symbolBroker, "ExposureTime", "60");
        AddConstant(viewModel, constants, symbolBroker, "FudgeFactor", "-15");

        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, "Variable expression example");
        TargetArea(viewModel).Add(sequence);

        GlobalVariable variable = viewModel.SequencerFactory.GetItem<GlobalVariable>();
        sequence.Add(variable);
        variable.SymbolBroker = symbolBroker;
        variable.Identifier = "CameraGain";
        variable.OriginalExpr.SymbolBroker = symbolBroker;
        variable.OriginalDefinition = "100";
        variable.OriginalExpr.Evaluate(true);
        variable.Expr.SymbolBroker = symbolBroker;
        variable.Executed = executed;
        if (executed) {
            variable.Expr.Definition = "100";
            variable.Expr.Evaluate(true);
        } else {
            variable.Expr.Definition = string.Empty;
            variable.Expr.IsExpression = true;
            variable.Expr.Error = "Not evaluated";
        }
        variable.Validate();

        TakeExposure exposure = NewExposure(viewModel, 60);
        sequence.Add(exposure);
        ConfigureExpression(exposure.ExposureTimeExpression, symbolBroker, "ExposureTime + FudgeFactor");
        ConfigureExpression(exposure.GainExpression, symbolBroker, "CameraGain");
        exposure.Validate();
    }

    private static void AddConstant(
            ISequence2VM viewModel,
            ISequenceContainer sequence,
            ISymbolBroker symbolBroker,
            string identifier,
            string definition) {
        GlobalConstant constant = viewModel.SequencerFactory.GetItem<GlobalConstant>();
        sequence.Add(constant);
        constant.SymbolBroker = symbolBroker;
        constant.Identifier = identifier;
        constant.Expr = new NINA.Sequencer.Logic.Expression(definition, sequence, constant) {
            SymbolBroker = symbolBroker
        };
        constant.Expr.Evaluate(true);
        constant.Validate();
    }

    private static void ConfigureExpression(
            NINA.Sequencer.Logic.Expression expression,
            ISymbolBroker symbolBroker,
            string definition) {
        expression.SymbolBroker = symbolBroker;
        expression.Definition = definition;
        expression.Evaluate(true);
    }

    private static void BuildSlewToRaDec(ISequence2VM viewModel) {
        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, "Coordinate example");
        SlewScopeToRaDec slew = viewModel.SequencerFactory.GetItem<SlewScopeToRaDec>();
        sequence.Add(slew);
        TargetArea(viewModel).Add(sequence);
        slew.RaExpression.Definition = "11.3758333";
        slew.RaExpression.Evaluate(true);
        slew.DecExpression.Definition = "-30.7198333";
        slew.DecExpression.Evaluate(true);
        slew.Validate();
    }

    private static void BuildSimpleToAdvanced(
            ISequence2VM viewModel,
            string state,
            ISymbolBroker symbolBroker) {
        if (state.Contains("startarea", StringComparison.Ordinal)) {
            BuildSimpleStartArea(viewModel);
        } else if (state.Contains("endarea", StringComparison.Ordinal)) {
            BuildSimpleEndArea(viewModel);
        } else if (state.Contains("targetarea", StringComparison.Ordinal)) {
            BuildSimpleTargetArea(viewModel);
        } else if (state.Contains("targetpreparation", StringComparison.Ordinal)) {
            BuildSimpleTargetPreparation(viewModel);
        } else if (state.Contains("targetclosure", StringComparison.Ordinal)) {
            BuildSimpleTargetClosure(viewModel);
        } else if (state.Contains("targetimaging", StringComparison.Ordinal)) {
            TargetArea(viewModel).Add(CreateSimpleImagingSet(viewModel, symbolBroker, 8, 40, 4, true, false, false));
        } else if (state.Contains("loopfortwo", StringComparison.Ordinal)) {
            TargetArea(viewModel).Add(CreateSimpleImagingSet(viewModel, symbolBroker, 4, 40, 4, true, true, false));
        } else if (state.Contains("loopuntiltime", StringComparison.Ordinal)) {
            SequentialContainer sequence = CreateSimpleImagingSet(viewModel, symbolBroker, 4, 40, 4, true, true, false);
            AddNauticalDawnCondition(viewModel, sequence);
            TargetArea(viewModel).Add(sequence);
        } else if (state.Contains("withoffsets", StringComparison.Ordinal)) {
            SequentialContainer sequence = CreateSimpleImagingSet(viewModel, symbolBroker, 4, 4, 4, false, false, false);
            AddNauticalDawnCondition(viewModel, sequence);
            TargetArea(viewModel).Add(sequence);
        } else if (state.Contains("betterdither", StringComparison.Ordinal)) {
            SequentialContainer sequence = CreateSimpleImagingSet(viewModel, symbolBroker, 4, 4, 0, false, false, true);
            AddNauticalDawnCondition(viewModel, sequence);
            TargetArea(viewModel).Add(sequence);
        }
    }

    private static void BuildSimpleStartArea(ISequence2VM viewModel) {
        ((ITriggerable)viewModel.Sequencer.MainContainer).Add(
            viewModel.SequencerFactory.GetTrigger<MeridianFlipTrigger>());
        CoolCamera coolCamera = viewModel.SequencerFactory.GetItem<CoolCamera>();
        coolCamera.Temperature = -40;
        coolCamera.Duration = 0;
        StartArea(viewModel).Add(coolCamera);
        StartArea(viewModel).Add(viewModel.SequencerFactory.GetItem<UnparkScope>());
    }

    private static void BuildSimpleEndArea(ISequence2VM viewModel) {
        ParallelContainer endInstructions = NewContainer<ParallelContainer>(viewModel, "End instructions");
        WarmCamera warmCamera = viewModel.SequencerFactory.GetItem<WarmCamera>();
        warmCamera.Duration = 0;
        endInstructions.Add(warmCamera);
        endInstructions.Add(viewModel.SequencerFactory.GetItem<ParkScope>());
        EndArea(viewModel).Add(endInstructions);
    }

    private static void BuildSimpleTargetArea(ISequence2VM viewModel) {
        DeepSkyObjectContainer target = NewDeepSkyObject(viewModel);
        target.Name = "M33 Pinwheel Galaxy";
        target.Target.TargetName = "M33 Pinwheel Galaxy";
        target.Add(NewCollapsedContainer(viewModel, "Target preparation instructions"));
        target.Add(NewCollapsedContainer(viewModel, "Target imaging instructions"));
        target.Add(NewCollapsedContainer(viewModel, "Target closure instructions"));
        TargetArea(viewModel).Add(target);
    }

    private static SequentialContainer NewCollapsedContainer(ISequence2VM viewModel, string name) {
        SequentialContainer container = NewContainer<SequentialContainer>(viewModel, name);
        container.IsExpanded = false;
        return container;
    }

    private static void BuildSimpleTargetPreparation(ISequence2VM viewModel) {
        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, "Target preparation instructions");
        SwitchFilter filter = viewModel.SequencerFactory.GetItem<SwitchFilter>();
        filter.ComboBoxText = "L";
        sequence.Add(filter);
        sequence.Add(viewModel.SequencerFactory.GetItem<CenterAndRotate>());
        sequence.Add(viewModel.SequencerFactory.GetItem<StartGuiding>());
        sequence.Add(viewModel.SequencerFactory.GetItem<RunAutofocus>());
        TargetArea(viewModel).Add(sequence);
    }

    private static void BuildSimpleTargetClosure(ISequence2VM viewModel) {
        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, "Target closure instructions");
        sequence.Add(viewModel.SequencerFactory.GetItem<StopGuiding>());
        TargetArea(viewModel).Add(sequence);
    }

    private static SequentialContainer CreateSimpleImagingSet(
            ISequence2VM viewModel,
            ISymbolBroker symbolBroker,
            int exposureCount,
            int iterations,
            int ditherEvery,
            bool autofocusAfterFilterChange,
            bool loopTwice,
            bool addDitherInstruction) {
        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, "Target imaging instructions");
        if (autofocusAfterFilterChange) {
            ((ITriggerable)sequence).Add(
                viewModel.SequencerFactory.GetTrigger<AutofocusAfterFilterChange>());
        }
        if (loopTwice) {
            LoopCondition loop = viewModel.SequencerFactory.GetCondition<LoopCondition>();
            loop.Iterations = 2;
            ((IConditionable)sequence).Add(loop);
        }
        string[] filters = ["L", "R", "G", "B"];
        for (int index = 0; index < exposureCount; index++) {
            sequence.Add(NewSmartExposure(
                viewModel,
                symbolBroker,
                filters[index % filters.Length],
                iterations,
                ditherEvery));
        }
        if (addDitherInstruction) {
            sequence.Add(viewModel.SequencerFactory.GetItem<Dither>());
        }
        return sequence;
    }

    private static SmartExposure NewSmartExposure(
            ISequence2VM viewModel,
            ISymbolBroker symbolBroker,
            string filterName,
            int iterations,
            int ditherEvery) {
        SmartExposure exposure = viewModel.SequencerFactory.GetItem<SmartExposure>();
        exposure.Iterations = iterations;
        exposure.GetLoopCondition().Iterations = iterations;
        exposure.GetTakeExposure().ExposureTime = 60;
        exposure.GetTakeExposure().ImageType = "LIGHT";
        exposure.GetTakeExposure().Gain = 139;
        exposure.GetTakeExposure().Offset = 21;
        SwitchFilter filter = exposure.GetSwitchFilter();
        filter.XfilterExpression.SymbolBroker = symbolBroker;
        filter.ComboBoxText = filterName;
        filter.XfilterExpression.Evaluate(true);
        exposure.GetDitherAfterExposures().AfterExposures = ditherEvery;
        return exposure;
    }

    private static void AddNauticalDawnCondition(
            ISequence2VM viewModel,
            SequentialContainer sequence) {
        TimeCondition until = viewModel.SequencerFactory.GetCondition<TimeCondition>();
        until.DateTime = DocumentationApplicationHost.FixedDateTime;
        until.SelectedProvider = until.DateTimeProviders.First(provider => provider.Name == "Nautical Dawn");
        ((IConditionable)sequence).Add(until);
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

    private static void BuildInstructionAnatomy(ISequence2VM viewModel, string state) {
        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, "Instruction example");
        TakeExposure exposure = NewExposure(viewModel, 180);
        if (state.Contains("instructionvalidation", StringComparison.Ordinal)) {
            exposure.Gain = 500;
            exposure.Offset = 500;
            exposure.Validate();
        }
        sequence.Add(exposure);
        TargetArea(viewModel).Add(sequence);
    }

    private static void BuildTriggerExample<TTrigger>(ISequence2VM viewModel, string name)
            where TTrigger : ISequenceTrigger {
        SequentialContainer sequence = NewContainer<SequentialContainer>(viewModel, name);
        ((ITriggerable)sequence).Add(viewModel.SequencerFactory.GetTrigger<TTrigger>());
        sequence.Add(NewExposure(viewModel, 180));
        TargetArea(viewModel).Add(sequence);
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
        DocumentationAstronomy.AlignAltitudeChart(target);
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

    private static ISequenceContainer EndArea(ISequence2VM viewModel) =>
        (ISequenceContainer)viewModel.Sequencer.MainContainer.Items[2];

    private static string Normalize(string value) =>
        new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

    private static bool ContainsAny(string value, params string[] candidates) =>
        candidates.Any(candidate => value.Contains(candidate, StringComparison.Ordinal));
}

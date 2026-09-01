#region "copyright"

/*
    Copyright (c) 2016 - 2026 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NUnit.Framework;

namespace NINA.DocumentationScreenshots.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class NamedStateControllerTests {
    [OneTimeSetUp]
    public void SetUp() {
        WpfBootstrap.Initialize();
    }

    [Test]
    public void Apply_SelectsProductionTemplatesTab() {
        ScreenshotAsset asset = new() {
            Id = "sequencer-templates",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/sequencer/Sequencer_Templates.png",
            Width = 410,
            Height = 510,
            Fixture = "sequencer",
            State = "sequencer-templates",
            ViewType = "NINA.View.Sequencer.AdvancedSequencer.AdvancedSequencerView"
        };
        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        Window host = new() { Width = asset.Width, Height = asset.Height, Content = fixture, ShowInTaskbar = false };
        try {
            host.Show();
            fixture.Measure(new Size(asset.Width, asset.Height));
            fixture.Arrange(new Rect(0, 0, asset.Width, asset.Height));
            fixture.UpdateLayout();

            NamedStateController.Apply(fixture, asset);

            TabControl sidebar = FindDescendants<TabControl>(fixture).Single(control => control.Items.Count == 5);
            TabItem selected = (TabItem)sidebar.SelectedItem;
            object viewModel = fixture.DataContext;
            NINA.Sequencer.TemplateController templates = (NINA.Sequencer.TemplateController)(viewModel.GetType()
                .GetProperty("TemplateController")?.GetValue(viewModel)
                ?? throw new AssertionException("The production sequencer view model has no TemplateController."));
            Assert.Multiple(() => {
                Assert.That(FindDescendants<TextBlock>(selected).Any(text => text.Text == "Templates"), Is.True);
                Assert.That(templates.Templates.Select(template => template.Container.Name), Is.EquivalentTo(new[] {
                    "Basic Sequence End",
                    "Basic Sequence Startup",
                    "Basic Sequence Target"
                }));
            });
        } finally {
            CloseMenus(fixture);
            host.Close();
        }
    }

    [Test]
    public void Apply_SelectsProductionTargetsTabWithDeterministicRealTargets() {
        ScreenshotAsset asset = SequencerAsset(
            "sequencer-targets",
            "docs/images/sequencer/Sequencer_TargetsTab.png");
        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        Window host = new() { Width = 1450, Height = 900, Content = fixture, ShowInTaskbar = false };
        try {
            host.Show();
            fixture.UpdateLayout();

            NamedStateController.Apply(fixture, asset);

            object viewModel = fixture.DataContext;
            NINA.Sequencer.TargetController targets = (NINA.Sequencer.TargetController)(viewModel.GetType()
                .GetProperty("TargetController")?.GetValue(viewModel)
                ?? throw new AssertionException("The production sequencer view model has no TargetController."));
            Assert.That(targets.Targets.Select(target => target.Name), Is.EqualTo(new[] {
                "Andromeda Galaxy",
                "Orion Nebula",
                "Triangulum Galaxy"
            }));
        } finally {
            CloseMenus(fixture);
            host.Close();
        }
    }

    [Test]
    public void AdvancedSequencer_UsesPopulatedProductionSymbolBroker() {
        ScreenshotAsset asset = new() {
            Id = "sequencer-symbols",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/sequencer/Sequencer_Symbols.png",
            Width = 313,
            Height = 687,
            Fixture = "sequencer",
            State = "sequencer-symbols",
            ViewType = "NINA.View.Sequencer.AdvancedSequencer.AdvancedSequencerView"
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        object viewModel = fixture.DataContext;
        object broker = viewModel.GetType().GetProperty("SymbolBroker")?.GetValue(viewModel)
            ?? throw new AssertionException("The production sequencer view model has no SymbolBroker.");

        Assert.Multiple(() => {
            Assert.That(broker, Is.TypeOf<NINA.Sequencer.Logic.SymbolBroker>());
            Assert.That(((NINA.Sequencer.Logic.ISymbolBroker)broker).GetSymbols(), Has.Count.GreaterThan(20));
            Assert.That(((NINA.Sequencer.Logic.ISymbolBroker)broker).GetFunctions(), Has.Count.GreaterThan(5));
        });
    }

    [Test]
    public void AdvancedSequencer_DoesNotInventACommandLineSequenceFile() {
        FrameworkElement fixture = new FixtureRegistry().Create(SequencerAsset(
            "sequencer-overview",
            "docs/images/generated/sequencer/Sequencer_Overview.png"));
        object viewModel = fixture.DataContext;
        object commandLineOptions = viewModel.GetType()
            .GetField("commandLineOptions", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?.GetValue(viewModel)
            ?? throw new AssertionException("The production sequencer view model has no command-line options service.");
        object? sequenceFile = commandLineOptions.GetType().GetProperty("SequenceFile")?.GetValue(commandLineOptions);

        Assert.That(sequenceFile, Is.Null);
    }

    [Test]
    public void AdvancedSequencer_FlowStateBuildsARealNestedProductionSequence() {
        ScreenshotAsset asset = new() {
            Id = "sequencer-flow",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/sequencer/Sequencer_Flow.png",
            Width = 1117,
            Height = 622,
            Fixture = "sequencer",
            State = "sequencer-flow",
            ViewType = "NINA.View.Sequencer.AdvancedSequencer.AdvancedSequencerView"
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        NINA.ViewModel.Sequencer.ISequence2VM viewModel =
            (NINA.ViewModel.Sequencer.ISequence2VM)fixture.DataContext;
        NINA.Sequencer.Container.ISequenceContainer targetArea =
            (NINA.Sequencer.Container.ISequenceContainer)viewModel.Sequencer.MainContainer.Items[1];
        NINA.Sequencer.Container.SequentialContainer repeated =
            targetArea.Items.OfType<NINA.Sequencer.Container.SequentialContainer>().Single();

        Assert.Multiple(() => {
            Assert.That(repeated.Name, Is.EqualTo("Repeat captures"));
            Assert.That(repeated.Conditions.OfType<NINA.Sequencer.Conditions.LoopCondition>().Single().Iterations, Is.EqualTo(5));
            Assert.That(repeated.Items.OfType<NINA.Sequencer.SequenceItem.Utility.WaitForTimeSpan>().ToList(), Has.Count.EqualTo(2));
            Assert.That(targetArea.Items.OfType<NINA.Sequencer.SequenceItem.Utility.WaitForTimeSpan>().ToList(), Has.Count.EqualTo(5));
        });
    }

    [Test]
    public void AdvancedSequencer_SequentialInstructionsStateUsesProductionFilterAndExposureItems() {
        FrameworkElement fixture = new FixtureRegistry().Create(SequencerAsset(
            "sequencer-sequential-instructions",
            "docs/images/generated/sequencer/Sequencer_SequentialInstructions.png"));
        NINA.ViewModel.Sequencer.ISequence2VM viewModel =
            (NINA.ViewModel.Sequencer.ISequence2VM)fixture.DataContext;
        NINA.Sequencer.Container.ISequenceContainer targetArea =
            (NINA.Sequencer.Container.ISequenceContainer)viewModel.Sequencer.MainContainer.Items[1];
        NINA.Sequencer.Container.SequentialContainer sequence =
            targetArea.Items.OfType<NINA.Sequencer.Container.SequentialContainer>().Single();

        Assert.Multiple(() => {
            Assert.That(sequence.Items.OfType<NINA.Sequencer.SequenceItem.FilterWheel.SwitchFilter>().ToList(), Has.Count.EqualTo(3));
            Assert.That(sequence.Items.OfType<NINA.Sequencer.SequenceItem.Imaging.TakeExposure>().ToList(), Has.Count.EqualTo(3));
            Assert.That(sequence.Items.OfType<NINA.Sequencer.SequenceItem.FilterWheel.SwitchFilter>()
                .Select(item => item.ComboBoxText), Is.EqualTo(new[] { "R", "G", "B" }));
            Assert.That(sequence.Items.OfType<NINA.Sequencer.SequenceItem.FilterWheel.SwitchFilter>()
                .Select(item => (int)item.Filter.Position), Is.EqualTo(new[] { 1, 2, 3 }));
        });
    }

    [Test]
    public void AdvancedSequencer_NestedTriggersStateUsesProductionTriggerAndContainers() {
        FrameworkElement fixture = new FixtureRegistry().Create(SequencerAsset(
            "sequencer-nested-triggers",
            "docs/images/generated/sequencer/Sequencer_NestedTriggers.png"));
        NINA.ViewModel.Sequencer.ISequence2VM viewModel =
            (NINA.ViewModel.Sequencer.ISequence2VM)fixture.DataContext;
        NINA.Sequencer.Container.ISequenceContainer targetArea =
            (NINA.Sequencer.Container.ISequenceContainer)viewModel.Sequencer.MainContainer.Items[1];
        NINA.Sequencer.Container.SequentialContainer sequence =
            targetArea.Items.OfType<NINA.Sequencer.Container.SequentialContainer>().Single();

        Assert.Multiple(() => {
            Assert.That(sequence.Triggers.OfType<NINA.Sequencer.Trigger.Autofocus.AutofocusAfterExposures>().Single().AfterExposures, Is.EqualTo(5));
            Assert.That(sequence.Items.OfType<NINA.Sequencer.Container.SequentialContainer>().ToList(), Has.Count.EqualTo(2));
            Assert.That(sequence.Items.OfType<NINA.Sequencer.Container.SequentialContainer>()
                .SelectMany(container => container.Items)
                .OfType<NINA.Sequencer.SequenceItem.Imaging.TakeExposure>().ToList(), Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void AdvancedSequencer_InstructionDetailsStateUsesRealConfiguredItems() {
        FrameworkElement fixture = new FixtureRegistry().Create(SequencerAsset(
            "sequencer-instructions-details",
            "docs/images/generated/sequencer/Sequencer_InstructionsDetails.png"));
        NINA.ViewModel.Sequencer.ISequence2VM viewModel =
            (NINA.ViewModel.Sequencer.ISequence2VM)fixture.DataContext;
        NINA.Sequencer.Container.ISequenceContainer targetArea =
            (NINA.Sequencer.Container.ISequenceContainer)viewModel.Sequencer.MainContainer.Items[1];
        NINA.Sequencer.Container.SequentialContainer sequence =
            targetArea.Items.OfType<NINA.Sequencer.Container.SequentialContainer>().Single();

        Assert.Multiple(() => {
            Assert.That(sequence.Items[0], Is.TypeOf<NINA.Sequencer.SequenceItem.Camera.CoolCamera>());
            Assert.That(sequence.Items[1], Is.TypeOf<NINA.Sequencer.SequenceItem.Utility.WaitForTime>());
            Assert.That(sequence.Items[2], Is.TypeOf<NINA.Sequencer.SequenceItem.FilterWheel.SwitchFilter>());
            Assert.That(sequence.Items[3], Is.TypeOf<NINA.Sequencer.SequenceItem.Imaging.TakeExposure>());
            Assert.That(((NINA.Sequencer.SequenceItem.Camera.CoolCamera)sequence.Items[0]).Temperature, Is.EqualTo(-10));
            Assert.That(((NINA.Sequencer.SequenceItem.FilterWheel.SwitchFilter)sequence.Items[2]).ComboBoxText, Is.EqualTo("L"));
            Assert.That(((NINA.Sequencer.SequenceItem.Imaging.TakeExposure)sequence.Items[3]).ExposureTime, Is.EqualTo(10));
        });
    }

    [Test]
    public void AdvancedSequencer_IssuesStateUsesRealValidationFailures() {
        FrameworkElement fixture = new FixtureRegistry().Create(SequencerAsset(
            "sequencer-issues",
            "docs/images/sequencer/Sequencer_Issues.png"));
        NINA.ViewModel.Sequencer.ISequence2VM viewModel =
            (NINA.ViewModel.Sequencer.ISequence2VM)fixture.DataContext;
        NINA.Sequencer.Container.ISequenceContainer targetArea =
            (NINA.Sequencer.Container.ISequenceContainer)viewModel.Sequencer.MainContainer.Items[1];
        List<NINA.Sequencer.SequenceItem.Imaging.TakeExposure> exposures = targetArea.Items
            .OfType<NINA.Sequencer.Container.SequentialContainer>()
            .Single()
            .Items
            .OfType<NINA.Sequencer.SequenceItem.Imaging.TakeExposure>()
            .ToList();

        Assert.Multiple(() => {
            Assert.That(exposures, Has.Count.EqualTo(6));
            Assert.That(exposures.Select(exposure => exposure.Issues), Has.All.Not.Empty);
            Assert.That(exposures[0].Issues, Has.Some.Contains("500"));
        });
    }

    [Test]
    public void AdvancedSequencer_DefineSymbolsStateUsesProductionSymbolItems() {
        FrameworkElement fixture = new FixtureRegistry().Create(SequencerAsset(
            "sequencer-define-variable",
            "docs/images/sequencer/Sequencer_DefineVariable.png"));
        NINA.ViewModel.Sequencer.ISequence2VM viewModel =
            (NINA.ViewModel.Sequencer.ISequence2VM)fixture.DataContext;
        NINA.Sequencer.Container.ISequenceContainer targetArea =
            (NINA.Sequencer.Container.ISequenceContainer)viewModel.Sequencer.MainContainer.Items[1];
        NINA.Sequencer.Container.SequentialContainer sequence =
            targetArea.Items.OfType<NINA.Sequencer.Container.SequentialContainer>().Single();

        Assert.Multiple(() => {
            Assert.That(sequence.Items, Has.All.TypeOf<NINA.Sequencer.SequenceItem.Expressions.GlobalVariable>());
            Assert.That(sequence.Items.OfType<NINA.Sequencer.SequenceItem.Expressions.GlobalVariable>()
                .Select(item => item.Name), Has.None.StartsWith("MISSING LABEL"));
            Assert.That(sequence.Items.OfType<NINA.Sequencer.SequenceItem.Expressions.GlobalVariable>()
                .Select(item => item.Identifier), Is.EqualTo(new[] { "LastFilter", "MaxAltitude" }));
            Assert.That(sequence.Items.OfType<NINA.Sequencer.SequenceItem.Expressions.GlobalVariable>()
                .Select(item => item.OriginalDefinition), Is.EqualTo(new[] { "'ASKAR_D2'", "80" }));
            Assert.That(sequence.Items.OfType<NINA.Sequencer.SequenceItem.Expressions.GlobalVariable>()
                .Select(item => item.Issues), Has.All.Empty);
        });
    }

    [Test]
    public void AdvancedSequencer_DefineConstantStateUsesProductionConstantItems() {
        FrameworkElement fixture = new FixtureRegistry().Create(SequencerAsset(
            "sequencer-define-constant",
            "docs/images/generated/sequencer/Sequencer_DefineConstant.png"));
        NINA.ViewModel.Sequencer.ISequence2VM viewModel =
            (NINA.ViewModel.Sequencer.ISequence2VM)fixture.DataContext;
        NINA.Sequencer.Container.ISequenceContainer targetArea =
            (NINA.Sequencer.Container.ISequenceContainer)viewModel.Sequencer.MainContainer.Items[1];
        NINA.Sequencer.Container.SequentialContainer sequence =
            targetArea.Items.OfType<NINA.Sequencer.Container.SequentialContainer>().Single();

        Assert.Multiple(() => {
            Assert.That(sequence.Items, Has.All.TypeOf<NINA.Sequencer.SequenceItem.Expressions.GlobalConstant>());
            Assert.That(sequence.Items.OfType<NINA.Sequencer.SequenceItem.Expressions.GlobalConstant>()
                .Select(item => item.Identifier), Is.EqualTo(new[] { "ExposureTime", "TargetTemp" }));
            Assert.That(sequence.Items.OfType<NINA.Sequencer.SequenceItem.Expressions.GlobalConstant>()
                .Select(item => item.Expr.Definition), Is.EqualTo(new[] { "60", "-15" }));
            Assert.That(sequence.Items.OfType<NINA.Sequencer.SequenceItem.Expressions.GlobalConstant>()
                .Select(item => item.Issues), Has.All.Empty);
        });
    }

    [Test]
    public void AdvancedSequencer_DefinedSymbolsRemainValidAcrossRepeatedFixtures() {
        _ = new FixtureRegistry().Create(SequencerAsset(
            "sequencer-define-variable",
            "docs/images/sequencer/Sequencer_DefineVariable.png"));

        FrameworkElement fixture = new FixtureRegistry().Create(SequencerAsset(
            "sequencer-define-variable-repeat",
            "docs/images/sequencer/Sequencer_DefineVariable.png"));
        NINA.ViewModel.Sequencer.ISequence2VM viewModel =
            (NINA.ViewModel.Sequencer.ISequence2VM)fixture.DataContext;
        NINA.Sequencer.Container.ISequenceContainer targetArea =
            (NINA.Sequencer.Container.ISequenceContainer)viewModel.Sequencer.MainContainer.Items[1];
        List<NINA.Sequencer.SequenceItem.Expressions.GlobalVariable> variables = targetArea.Items
            .OfType<NINA.Sequencer.Container.SequentialContainer>()
            .Single()
            .Items
            .OfType<NINA.Sequencer.SequenceItem.Expressions.GlobalVariable>()
            .ToList();

        Assert.Multiple(() => {
            Assert.That(variables.Select(item => item.Identifier), Is.EqualTo(new[] { "LastFilter", "MaxAltitude" }));
            Assert.That(variables.Select(item => item.Issues), Has.All.Empty);
        });
    }

    [Test]
    public void AdvancedSequencer_UserTemplateStateUsesAProductionTemplatedContainer() {
        FrameworkElement fixture = new FixtureRegistry().Create(SequencerAsset(
            "sequencer-user-template",
            "docs/images/generated/sequencer/Sequencer_UserTemplate.png"));
        object viewModel = fixture.DataContext;
        NINA.Sequencer.TemplateController templates = (NINA.Sequencer.TemplateController)(viewModel.GetType()
            .GetProperty("TemplateController")?.GetValue(viewModel)
            ?? throw new AssertionException("The production sequencer view model has no TemplateController."));

        NINA.Sequencer.TemplatedSequenceContainer userTemplate = templates.UserTemplates.Single();
        NINA.Sequencer.Container.SequentialContainer sequence =
            (NINA.Sequencer.Container.SequentialContainer)userTemplate.Container;
        Assert.Multiple(() => {
            Assert.That(sequence.Name, Is.EqualTo("RGB Loop"));
            Assert.That(sequence.Triggers, Has.Exactly(1).TypeOf<NINA.Sequencer.Trigger.Autofocus.AutofocusAfterHFRIncreaseTrigger>());
            Assert.That(sequence.Triggers.OfType<NINA.Sequencer.Trigger.Autofocus.AutofocusAfterHFRIncreaseTrigger>()
                .Single().Issues, Is.Empty);
            Assert.That(sequence.Conditions, Has.Exactly(1).TypeOf<NINA.Sequencer.Conditions.TimeCondition>());
            Assert.That(sequence.Items.OfType<NINA.Sequencer.SequenceItem.FilterWheel.SwitchFilter>().ToList(), Has.Count.EqualTo(3));
            Assert.That(sequence.Items.OfType<NINA.Sequencer.SequenceItem.Imaging.TakeExposure>().ToList(), Has.Count.EqualTo(3));
        });
    }

    [Test]
    public void Apply_ExpandsTheProductionUserTemplatePreview() {
        ScreenshotAsset asset = SequencerAsset(
            "sequencer-user-template",
            "docs/images/generated/sequencer/Sequencer_UserTemplate.png");
        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        Window host = new() { Width = 1900, Height = 1000, Content = fixture, ShowInTaskbar = false };
        try {
            host.Show();
            fixture.UpdateLayout();

            NamedStateController.Apply(fixture, asset);

            Expander template = FindDescendants<Expander>(fixture)
                .Single(expander => expander.DataContext is NINA.Sequencer.TemplatedSequenceContainer item
                    && item.Container.Name == "RGB Loop");
            Assert.That(template.IsExpanded, Is.True);
        } finally {
            CloseMenus(fixture);
            host.Close();
        }
    }

    [TestCase("sequencer-apply-target")]
    [TestCase("sequencer-add-target-to-target-tab")]
    [TestCase("sequencer-drop-target-to-tab")]
    public void AdvancedSequencer_TargetWorkflowStatesUseAProductionDeepSkyObjectContainer(string state) {
        FrameworkElement fixture = new FixtureRegistry().Create(SequencerAsset(
            state,
            $"docs/images/sequencer/Sequencer_{state}.png"));
        NINA.ViewModel.Sequencer.ISequence2VM viewModel =
            (NINA.ViewModel.Sequencer.ISequence2VM)fixture.DataContext;
        NINA.Sequencer.Container.ISequenceContainer targetArea =
            (NINA.Sequencer.Container.ISequenceContainer)viewModel.Sequencer.MainContainer.Items[1];

        NINA.Sequencer.Container.DeepSkyObjectContainer target = targetArea.Items
            .OfType<NINA.Sequencer.Container.DeepSkyObjectContainer>()
            .Single();
        Assert.Multiple(() => {
            Assert.That(target.Target.TargetName, Is.EqualTo("Triangulum Pinwheel"));
            Assert.That(target.Items, Is.Not.Empty);
        });
    }

    [Test]
    public void AdvancedSequencer_DragDropStateUsesAProductionEmptyInstructionSetInTheStartArea() {
        FrameworkElement fixture = new FixtureRegistry().Create(SequencerAsset(
            "sequencer-drag-drop",
            "docs/images/sequencer/Sequencer_DragDrop.png"));
        NINA.ViewModel.Sequencer.ISequence2VM viewModel =
            (NINA.ViewModel.Sequencer.ISequence2VM)fixture.DataContext;
        NINA.Sequencer.Container.ISequenceContainer startArea =
            (NINA.Sequencer.Container.ISequenceContainer)viewModel.Sequencer.MainContainer.Items[0];

        NINA.Sequencer.Container.SequentialContainer instructionSet = startArea.Items
            .OfType<NINA.Sequencer.Container.SequentialContainer>()
            .Single();
        Assert.Multiple(() => {
            Assert.That(instructionSet.Name, Is.EqualTo("Startup instructions"));
            Assert.That(instructionSet.Items, Is.Empty);
        });
    }

    [Test]
    public void Apply_PinsProductionAltitudeChartNowMarkerToFixtureClock() {
        ScreenshotAsset asset = SequencerAsset(
            "sequencer-dso-set",
            "docs/images/generated/sequencer/Sequencer_DSOSet.png");
        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        Window host = new() { Width = 1450, Height = 900, Content = fixture, ShowInTaskbar = false };
        try {
            host.Show();
            fixture.UpdateLayout();

            NamedStateController.Apply(fixture, asset);

            OxyPlot.Wpf.LineAnnotation marker = FindDescendants<OxyPlot.Wpf.Plot>(fixture)
                .SelectMany(plot => plot.Annotations.OfType<OxyPlot.Wpf.LineAnnotation>())
                .Single(annotation => annotation.Text == "Now");
            Assert.Multiple(() => {
                Assert.That(System.Windows.Data.BindingOperations.GetBindingBase(marker, OxyPlot.Wpf.LineAnnotation.XProperty), Is.Null);
                Assert.That(marker.X, Is.EqualTo(OxyPlot.Axes.DateTimeAxis.ToDouble(
                    new DateTime(2026, 8, 31, 20, 0, 0, DateTimeKind.Local))));
            });
        } finally {
            CloseMenus(fixture);
            host.Close();
        }
    }

    [TestCase("Sequencer_DragDrop.png")]
    [TestCase("Sequencer_AddTarget.png")]
    [TestCase("Sequencer_ApplyTarget.png")]
    [TestCase("Sequencer_DropTargetToTab.png")]
    [TestCase("Sequencer_SaveAsTemplateDragDrop.png")]
    public void Apply_UsesProductionAdornersForSequencerDragStates(string fileName) {
        ScreenshotAsset asset = SequencerAsset(
            Path.GetFileNameWithoutExtension(fileName),
            $"docs/images/sequencer/{fileName}");
        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        Window host = new() { Width = 1900, Height = 1000, Content = fixture, ShowInTaskbar = false };
        try {
            host.Show();
            fixture.UpdateLayout();

            NamedStateController.Apply(fixture, asset);

            List<DependencyObject> descendants = FindDescendants<DependencyObject>(fixture).ToList();
            Assert.Multiple(() => {
                Assert.That(descendants.Count(element => element.GetType().Name == "DragDropAdorner"), Is.EqualTo(1));
                Assert.That(descendants.Count(element => element.GetType().Name == "DragOverAdorner"), Is.EqualTo(1));
            });
        } finally {
            CloseMenus(fixture);
            host.Close();
        }
    }

    [Test]
    public void Apply_OpensTheProductionSaveTargetTooltip() {
        ScreenshotAsset asset = SequencerAsset(
            "sequencer-add-target-to-target-tab",
            "docs/images/sequencer/Sequencer_AddTargetToTargetTab.png");
        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        Window host = new() { Width = 1900, Height = 700, Content = fixture, ShowInTaskbar = false };
        try {
            host.Show();
            fixture.UpdateLayout();

            NamedStateController.Apply(fixture, asset);

            Button saveTarget = FindDescendants<Button>(fixture)
                .Single(button => button.Name == "TargetContainerButton"
                    && button.DataContext is NINA.Sequencer.Container.DeepSkyObjectContainer);
            Assert.That(saveTarget.ToolTip, Is.TypeOf<ToolTip>());
            Assert.That(((ToolTip)saveTarget.ToolTip).IsOpen, Is.True);
        } finally {
            CloseMenus(fixture);
            host.Close();
        }
    }

    [Test]
    public void Apply_OpensProductionValidationContentInATooltip() {
        ScreenshotAsset asset = SequencerAsset(
            "sequencer-issues",
            "docs/images/sequencer/Sequencer_Issues.png");
        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        Window host = new() { Width = 1500, Height = 900, Content = fixture, ShowInTaskbar = false };
        try {
            host.Show();
            fixture.UpdateLayout();

            NamedStateController.Apply(fixture, asset);

            Border issue = FindDescendants<Border>(fixture)
                .First(border => border.DataContext is NINA.Sequencer.SequenceItem.Imaging.TakeExposure exposure
                    && exposure.Issues.Count > 0
                    && border.ToolTip is ToolTip { IsOpen: true });
            Assert.That(((ToolTip)issue.ToolTip).Content, Is.TypeOf<ItemsControl>());
        } finally {
            CloseMenus(fixture);
            host.Close();
        }
    }

    [TestCase("Sequencer_AddTrigger.png", "AddTriggerButton")]
    [TestCase("Sequencer_AddLoopCondition.png", "AddConditionButton")]
    public void Apply_OpensTheRealProductionContainerMenu(string fileName, string buttonName) {
        ScreenshotAsset asset = SequencerAsset(
            Path.GetFileNameWithoutExtension(fileName),
            $"docs/images/sequencer/{fileName}");
        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        Window host = new() { Width = 1450, Height = 900, Content = fixture, ShowInTaskbar = false };
        try {
            host.Show();
            fixture.UpdateLayout();

            NamedStateController.Apply(fixture, asset);

            Button button = FindDescendants<Button>(fixture).Single(control => control.Name == buttonName && control.ContextMenu?.IsOpen == true);
            Assert.Multiple(() => {
                Assert.That(button.ContextMenu, Is.Not.Null);
                Assert.That(button.ContextMenu!.IsOpen, Is.True);
                Assert.That(button.ContextMenu.Items.Count, Is.GreaterThan(3));
            });
        } finally {
            CloseMenus(fixture);
            host.Close();
        }
    }

    [TestCase("Sequencer_AddInstruction.png", "Camera")]
    [TestCase("Sequencer_AddInstructionSet.png", "Instruction Set")]
    public void Apply_ExpandsTheRealProductionRootMenu(string fileName, string expectedCategory) {
        ScreenshotAsset asset = SequencerAsset(
            Path.GetFileNameWithoutExtension(fileName),
            $"docs/images/sequencer/{fileName}");
        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        Window host = new() { Width = 1450, Height = 900, Content = fixture, ShowInTaskbar = false };
        try {
            host.Show();
            fixture.UpdateLayout();

            NamedStateController.Apply(fixture, asset);

            List<MenuItem> expanded = PresentationSource.CurrentSources
                .OfType<System.Windows.Interop.HwndSource>()
                .Where(source => source.RootVisual is not null)
                .SelectMany(source => FindDescendants<MenuItem>(source.RootVisual!))
                .Where(item => item.IsSubmenuOpen)
                .ToList();
            Assert.That(expanded.Any(item => FindDescendants<TextBlock>(item)
                .Any(text => text.Text.Contains(expectedCategory, StringComparison.OrdinalIgnoreCase))), Is.True);
        } finally {
            CloseMenus(fixture);
            host.Close();
        }
    }

    private static ScreenshotAsset SequencerAsset(string id, string output) => new() {
        Id = id,
        Classification = ScreenshotClassification.NinaUi,
        Output = output,
        Width = 1200,
        Height = 700,
        Fixture = "sequencer",
        State = id,
        ViewType = "NINA.View.Sequencer.AdvancedSequencer.AdvancedSequencerView"
    };

    private static void CloseMenus(DependencyObject fixture) {
        foreach (Button button in FindDescendants<Button>(fixture).Where(button => button.ContextMenu?.IsOpen == true)) {
            button.ContextMenu!.IsOpen = false;
        }
        foreach (System.Windows.Interop.HwndSource source in PresentationSource.CurrentSources
            .OfType<System.Windows.Interop.HwndSource>()
            .Where(source => source.RootVisual is not null)) {
            foreach (MenuItem item in FindDescendants<MenuItem>(source.RootVisual!).Reverse()) {
                item.IsSubmenuOpen = false;
            }
        }
        Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() => { }));
    }

    private static IEnumerable<T> FindDescendants<T>(DependencyObject root) where T : DependencyObject {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++) {
            DependencyObject child = VisualTreeHelper.GetChild(root, index);
            if (child is T result) {
                yield return result;
            }
            foreach (T descendant in FindDescendants<T>(child)) {
                yield return descendant;
            }
        }
    }
}

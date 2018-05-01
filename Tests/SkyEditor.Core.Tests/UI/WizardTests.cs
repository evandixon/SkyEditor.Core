using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.ConsoleCommands;
using SkyEditor.Core.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SkyEditor.Core.Tests.UI
{
    [TestClass]
    public class WizardTests
    {
        public const string TestCategory = "UI - Wizard Tests"; 

        public class AddingWizard : Wizard
        {
            public AddingWizard(PluginManager pluginManager) : base(pluginManager)
            {
                Term1Step = new AddingWizardTerm1();
                Term2Step = new AddingWizardTerm2();
                ResultStep = new AddingWizardResultView(this);

                StepsInternal.Add(Term1Step);
                StepsInternal.Add(Term2Step);
                StepsInternal.Add(ResultStep);
            }

            public AddingWizardTerm1 Term1Step { get; set; }
            public AddingWizardTerm2 Term2Step { get; set; }
            public AddingWizardResultView ResultStep { get; set; }

            public override string Name => "Adding Wizard";
        }

        public class AddingWizardTerm1 : IWizardStepViewModel, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public string Name => "Term 1";

            public int? Term1
            {
                get
                {
                    return _term1;
                }
                set
                {
                    _term1 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Term1)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsComplete)));
                }
            }
            private int? _term1;

            public bool IsComplete => Term1.HasValue;

            public ConsoleCommand GetConsoleCommand()
            {
                throw new NotImplementedException();
            }
        }

        public class AddingWizardTerm2 : IWizardStepViewModel, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public string Name => "Term 2";

            public int? Term2
            {
                get
                {
                    return _term2;
                }
                set
                {
                    _term2 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Term2)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsComplete)));
                }
            }
            private int? _term2;

            public bool IsComplete => Term2.HasValue;

            public ConsoleCommand GetConsoleCommand()
            {
                throw new NotImplementedException();
            }
        }

        public class AddingWizardResultView : IWizardStepViewModel, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public AddingWizardResultView(AddingWizard wizard)
            {
                Wizard = wizard;
            }

            protected AddingWizard Wizard { get; set; }

            public string Name => "Result";

            public int Result => Wizard.Term1Step.Term1.Value + Wizard.Term2Step.Term2.Value;

            public bool ResultApproved
            {
                get
                {
                    return _resultApproved;
                }
                set
                {
                    _resultApproved = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultApproved)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsComplete)));
                }
            }
            private bool _resultApproved;

            public bool IsComplete => ResultApproved;

            public ConsoleCommand GetConsoleCommand()
            {
                throw new NotImplementedException();
            }
        }        

        public PluginManager CurrentPluginManager { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            CurrentPluginManager = new PluginManager();
            CurrentPluginManager.LoadCore(new BasicTestCoreMod()).Wait();
        }

        [TestCleanup]
        public void Cleanup()
        {
            CurrentPluginManager.Dispose();
        }        

        [TestMethod]
        [TestCategory(TestCategory)]
        public void TestWizardForwardWorkflow()
        {
            var wizard = new AddingWizard(CurrentPluginManager);

            Assert.AreEqual(3, wizard.Steps.Count, "Wizard does not have the correct number of steps");

            var r = new Random();
            var term1 = r.Next(0, 10000);
            var term2 = r.Next(0, 10000);

            // Go Forward

            Assert.AreEqual(0, wizard.CurrentStepIndex, "Wizard step index is not 0");
            Assert.AreEqual("Term 1", wizard.CurrentStep.Name);
            Assert.IsInstanceOfType(wizard.CurrentStep, typeof(AddingWizardTerm1));
            Assert.IsFalse(wizard.CanGoForward, "Shouldn't be able to proceed when step 1 is not complete");

            (wizard.CurrentStep as AddingWizardTerm1).Term1 = term1;

            Assert.IsTrue(wizard.CanGoForward, "Should be able to proceed when step 1 is complete");
            Assert.IsFalse(wizard.CanGoBack, "Shouldn't be able to go back when on the first step");

            wizard.GoForward();

            Assert.AreEqual(1, wizard.CurrentStepIndex, "Wizard step index is not 1");
            Assert.AreEqual("Term 2", wizard.CurrentStep.Name);
            Assert.IsInstanceOfType(wizard.CurrentStep, typeof(AddingWizardTerm2));
            Assert.IsFalse(wizard.CanGoForward, "Shouldn't be able to proceed when step 2 is not complete");

            (wizard.CurrentStep as AddingWizardTerm2).Term2 = term2;

            Assert.IsTrue(wizard.CanGoForward, "Should be able to proceed when step 2 is complete");
            Assert.IsTrue(wizard.CanGoBack, "Should be able to go back to first step");

            wizard.GoForward();

            Assert.AreEqual(2, wizard.CurrentStepIndex, "Wizard step index is not 2");
            Assert.AreEqual("Result", wizard.CurrentStep.Name);
            Assert.IsInstanceOfType(wizard.CurrentStep, typeof(AddingWizardResultView));
            Assert.IsFalse(wizard.CanGoForward, "Shouldn't be able to proceed when result view is not complete");
            Assert.AreEqual(term1 + term2, (wizard.CurrentStep as AddingWizardResultView).Result, "Incorrect addition result");

            (wizard.CurrentStep as AddingWizardResultView).ResultApproved = true;

            Assert.IsFalse(wizard.CanGoForward, "Shouldn't be able to proceed on final step");
            Assert.IsTrue(wizard.CanGoBack, "Should be able to go back to second step");
            Assert.IsTrue(wizard.IsComplete, "Wizard should be complete");
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WizardCantGoTooFarForward()
        {
            var wizard = new AddingWizard(CurrentPluginManager);

            Assert.AreEqual(3, wizard.Steps.Count, "Wizard does not have the correct number of steps");

            var r = new Random();
            var term1 = r.Next(0, 10000);
            var term2 = r.Next(0, 10000);

            // Go Forward normally
            (wizard.CurrentStep as AddingWizardTerm1).Term1 = term1;
            wizard.GoForward();
            (wizard.CurrentStep as AddingWizardTerm2).Term2 = term2;
            wizard.GoForward();
            (wizard.CurrentStep as AddingWizardResultView).ResultApproved = true;

            wizard.GoForward();
            wizard.GoForward();
            wizard.GoForward();
            wizard.GoForward();
            wizard.GoForward();
            wizard.GoForward();
            wizard.GoForward();
            wizard.GoForward();
            wizard.GoForward();

            Assert.IsInstanceOfType(wizard.CurrentStep, typeof(AddingWizardResultView));
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void TestWizardBackwardWorkflow()
        {
            var wizard = new AddingWizard(CurrentPluginManager);

            Assert.AreEqual(3, wizard.Steps.Count, "Wizard does not have the correct number of steps");

            var r = new Random();
            var term1 = r.Next(0, 10000);
            var term2 = r.Next(0, 10000);

            // Go Forward
            (wizard.CurrentStep as AddingWizardTerm1).Term1 = term1;
            wizard.GoForward();
            (wizard.CurrentStep as AddingWizardTerm2).Term2 = term2;
            wizard.GoForward();
            (wizard.CurrentStep as AddingWizardResultView).ResultApproved = true;

            // We're in the third step (verified by another test)

            // Go Backward

            (wizard.CurrentStep as AddingWizardResultView).ResultApproved = false;
            Assert.IsFalse(wizard.CanGoForward, "Shouldn't be able to proceed on final step");
            Assert.IsTrue(wizard.CanGoBack, "Going back should be unaffected by completeness");
            Assert.IsFalse(wizard.IsComplete, "Wizard should no longer be complete");

            wizard.GoBack();

            Assert.AreEqual(1, wizard.CurrentStepIndex, "Wizard step index is not 1 after going back");
            Assert.AreEqual("Term 2", wizard.CurrentStep.Name);
            Assert.IsInstanceOfType(wizard.CurrentStep, typeof(AddingWizardTerm2));
            Assert.IsTrue(wizard.CanGoForward, "Step 2 should still be complete");
            Assert.IsTrue(wizard.CanGoBack, "Should still be able to go back to first step");

            wizard.GoBack();

            Assert.AreEqual(0, wizard.CurrentStepIndex, "Wizard step index is not 0");
            Assert.AreEqual("Term 1", wizard.CurrentStep.Name);
            Assert.IsInstanceOfType(wizard.CurrentStep, typeof(AddingWizardTerm1));
            Assert.IsTrue(wizard.CanGoForward, "Step 1 should still be complete");
            Assert.IsFalse(wizard.CanGoBack, "Should still not be able to go before first step");

        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WizardCantGoTooFarBackward()
        {
            var wizard = new AddingWizard(CurrentPluginManager);

            Assert.AreEqual(3, wizard.Steps.Count, "Wizard does not have the correct number of steps");

            var r = new Random();
            var term1 = r.Next(0, 10000);
            var term2 = r.Next(0, 10000);

            // Go Forward normally
            (wizard.CurrentStep as AddingWizardTerm1).Term1 = term1;
            wizard.GoForward();
            (wizard.CurrentStep as AddingWizardTerm2).Term2 = term2;
            wizard.GoForward();
            (wizard.CurrentStep as AddingWizardResultView).ResultApproved = true;


            wizard.GoBack();
            wizard.GoBack();
            wizard.GoBack();
            wizard.GoBack();
            wizard.GoBack();
            wizard.GoBack();
            wizard.GoBack();
            wizard.GoBack();
            wizard.GoBack();

            Assert.IsInstanceOfType(wizard.CurrentStep, typeof(AddingWizardTerm1));
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WizardCallsPropertyChangedEventForCanGoNext()
        {
            // Arrange
            var wizard = new AddingWizard(CurrentPluginManager);
            var eventFired = false;
            void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(wizard.CanGoForward))
                {
                    eventFired = true;
                }
            }
            wizard.PropertyChanged += OnPropertyChanged;

            // Sanity check
            Assert.IsFalse(wizard.CanGoForward);

            // Act
            eventFired = false;
            wizard.Term1Step.Term1 = 10;

            // Assert
            Assert.IsTrue(wizard.CanGoForward, "Wizard should be able to go forward");
            Assert.IsTrue(eventFired, "Wizard.PropertyChanged did not fire for Wizard.CanGoForward");

            // Cleanup
            wizard.PropertyChanged -= OnPropertyChanged;
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WizardCallsPropertyChangedEventForCanGoNextStep2()
        {
            // Arrange
            var wizard = new AddingWizard(CurrentPluginManager);
            var eventFired = false;
            void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(wizard.CanGoForward))
                {
                    eventFired = true;
                }
            }
            wizard.PropertyChanged += OnPropertyChanged;
            wizard.Term1Step.Term1 = 10;
            wizard.GoForward();

            // Sanity check
            Assert.IsFalse(wizard.CanGoForward);

            // Act
            eventFired = false;
            wizard.Term2Step.Term2 = 10;

            // Assert
            Assert.IsTrue(wizard.CanGoForward, "Wizard should be able to go forward");
            Assert.IsTrue(eventFired, "Wizard.PropertyChanged did not fire for Wizard.CanGoForward");

            // Cleanup
            wizard.PropertyChanged -= OnPropertyChanged;
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public void WizardCallsPropertyChangedEventForCanFinish()
        {
            // Arrange
            var wizard = new AddingWizard(CurrentPluginManager);
            var eventFired = false;
            void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(wizard.IsComplete))
                {
                    eventFired = true;
                }
            }
            wizard.PropertyChanged += OnPropertyChanged;
            wizard.Term1Step.Term1 = 10;
            wizard.GoForward();
            wizard.Term2Step.Term2 = 10;
            wizard.GoForward();

            // Sanity check
            Assert.IsFalse(wizard.CanGoForward);
            Assert.IsFalse(wizard.IsComplete);

            // Act
            eventFired = false;
            wizard.ResultStep.ResultApproved = true;

            // Assert
            Assert.IsFalse(wizard.CanGoForward, "Wizard should not be able to go forward because it's the last step");
            Assert.IsTrue(wizard.IsComplete, "Wizard should be complete.");
            Assert.IsTrue(eventFired, "Wizard.PropertyChanged did not fire for Wizard.IsComplete");

            // Cleanup
            wizard.PropertyChanged -= OnPropertyChanged;
        }
    }
}

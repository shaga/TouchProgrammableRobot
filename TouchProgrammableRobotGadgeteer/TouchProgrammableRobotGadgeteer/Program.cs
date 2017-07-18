using Gadgeteer.Modules.GHIElectronics;
using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using TouchProgrammableRobotGadgeteer.View;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace TouchProgrammableRobotGadgeteer
{
    public partial class Program
    {
        private bool _isRunning;

        private int _idxCommand;

        private bool _isReversing;

        private TouchInterface _ti;

        private readonly ECommand[] _commands = new ECommand[Common.CommandCount];

        private StepperController _stepper;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/


            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");

            _stepper = new StepperController(3);

            _stepper.Initialize();
            _stepper.Completed += OnStepperCompleted;

            _ti = new TouchInterface(displayT35);
            _ti.Initialize();
            _ti.TouchRun += OnTouchedRun;
            _ti.UpdateCommandState += OnUpdatedCommandState;

            //var idx = 0;

            //var commands = new ECommand[Common.CommandCount];

            //var stepper = new StepperController(3);

            //stepper.Initialize();

            //var ti = new TouchInterface(displayT35);
            //ti.Initialize();

            //stepper.Completed += (s, e) =>
            //{
            //    ti.DeactiveCommandButton(idx++);

            //    if (commands[idx] == ECommand.Stop)
            //    {
            //        idx = 0;
            //        ti.IsEnableCommand = true;
            //        return;
            //    }


            //    ti.ActivateCommandButton(idx);
            //    switch (commands[idx])
            //    {
            //        case ECommand.Fore:
            //            stepper.GoForward();
            //            break;
            //        case ECommand.Back:
            //            stepper.GoBack();
            //            break;
            //        case ECommand.TurnLeft:
            //            stepper.TurnLeft();
            //            break;
            //        case ECommand.TurnRight:
            //            stepper.TurnRight();
            //            break;
            //        case ECommand.TurnBack:
            //            stepper.Turn();
            //            break;
            //    }
            //};


            //ti.TouchRun += (s, e) =>
            //{
            //    if (idx > 0 || commands[idx] == ECommand.Stop) return;

            //    ti.IsEnableCommand = false;
            //    ti.ActivateCommandButton(idx);
            //    switch (commands[idx])
            //    {
            //        case ECommand.Fore:
            //            stepper.GoForward();
            //            break;
            //        case ECommand.Back:
            //            stepper.GoBack();
            //            break;
            //        case ECommand.TurnLeft:
            //            stepper.TurnLeft();
            //            break;
            //        case ECommand.TurnRight:
            //            stepper.TurnRight();
            //            break;
            //        case ECommand.TurnBack:
            //            stepper.Turn();
            //            break;
            //    }
            //};

            //ti.UpdateCommandState += (s, e) =>
            //{
            //    var button = s as CommandToggleButton;

            //    if (button == null) return;

            //    commands[button.Idx] = button.Command;
            //};
        }

        private void OnUpdatedCommandState(object sender, EventArgs e)
        {
            if (_isRunning) return;

            var button = sender as CommandToggleButton;

            if (button == null) return;

            _commands[button.Idx] = button.Command;
        }

        private void OnTouchedRun(object sender, EventArgs e)
        {
            if (_isRunning)
            {
                _ti.DeactiveCommandButton(_idxCommand);
                _isRunning = false;
                _idxCommand = 0;
                _isReversing = false;
                _stepper.SetCommand(ECommand.Stop);
                _ti.IsReversing = false;
                //_ti.IsEnableCommand = true;
                _ti.IsRunning = false;
                return;
            }

            _isRunning = true;
            _isReversing = false;
            _idxCommand = 0;
            //_ti.IsEnableCommand = false;
            _ti.IsRunning = true;
            RunCommand();
        }

        private void OnStepperCompleted(object sender, EventArgs e)
        {
            if (!_isRunning) return;

            _ti.DeactiveCommandButton(_idxCommand);

            if (_isReversing) _idxCommand--;
            else _idxCommand++;

            if (_idxCommand < 0 || _idxCommand >= Common.CommandCount || _commands[_idxCommand] == ECommand.Stop)
            {
                SetNextState();
                return;
            }

            RunCommand();
        }

        private void SetNextState()
        {
            switch (_ti.Mode)
            {
                case EMode.Normal:
                    _idxCommand = 0;
                    //_ti.IsEnableCommand = true;
                    _ti.IsRunning = false;
                    _ti.IsReversing = false;
                    _isRunning = false;
                    break;
                case EMode.Repeat:
                    _idxCommand = 0;
                    RunCommand();
                    break;
                case EMode.Reverse:
                    if (_isReversing)
                    {
                        _idxCommand = 0;
                        //_ti.IsEnableCommand = true;
                        _ti.IsRunning = false;
                        _ti.IsReversing = false;
                        _isRunning = false;
                        _isReversing = false;
                    }
                    else
                    {
                        _idxCommand--;
                        _isReversing = true;
                        _ti.IsReversing = true;
                        RunCommand();
                    }
                    break;
                case EMode.ReverseRepeat:
                    if (_isReversing)
                    {
                        _idxCommand = 0;
                        _isReversing = false;
                        _ti.IsReversing = false;
                    }
                    else
                    {
                        _idxCommand--;
                        _isReversing = true;
                        _ti.IsReversing = true;
                    }
                    RunCommand();
                    break;
            }
        }


        private void RunCommand()
        {
            _ti.ActivateCommandButton(_idxCommand);
            var command = _commands[_idxCommand];

            if (_isReversing)
            {
                switch (command)
                {
                    case ECommand.Fore:
                        command = ECommand.Back;
                        break;
                    case ECommand.Back:
                        command = ECommand.Fore;
                        break;
                    case ECommand.TurnLeft:
                        command = ECommand.TurnRight;
                        break;
                    case ECommand.TurnRight:
                        command = ECommand.TurnLeft;
                        break;
                }
            }
            _stepper.SetCommand(command);
        }
    }
}


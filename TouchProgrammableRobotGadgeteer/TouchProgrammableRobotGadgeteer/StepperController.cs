using System;
using System.Threading;
using Gadgeteer;
using Gadgeteer.SocketInterfaces;
using Microsoft.SPOT;

namespace System.Diagnostics
{
    public enum DebuggerBrowsableState
    {
        Never,
        Collapsed,
        RootHidden
    }
}

namespace TouchProgrammableRobotGadgeteer
{
    class StepperController
    {
        private byte MotorPrescaler = 0;
        private ushort MotorPps = 800;

        private const Pca9629AStepper.EDirection LForeDir = Pca9629AStepper.EDirection.CW;
        private const Pca9629AStepper.EDirection RForeDir = Pca9629AStepper.EDirection.CCW;
        private const Pca9629AStepper.EDirection LBackDir = Pca9629AStepper.EDirection.CCW;
        private const Pca9629AStepper.EDirection RBackDir = Pca9629AStepper.EDirection.CW;

        private const ushort MoveStraight = 900;
        private const ushort MoveTurn90 = 1200;
        private const ushort MoveTurn180 = 2400;

        public event EventHandler Completed;

        private readonly Pca9629AStepper _left;
        private readonly Pca9629AStepper _right;

        private InterruptInput _interrupt;

        public StepperController(int socketNum)
        {
            _left = new Pca9629AStepper(0x22, socketNum);
            _right = new Pca9629AStepper(0x20, socketNum);

            var socket = Socket.GetSocket(socketNum, false, null, null);

            _interrupt = InterruptInputFactory.Create(socket, Socket.Pin.Three, GlitchFilterMode.On,
                ResistorMode.PullUp, InterruptMode.FallingEdge, null);

            _interrupt.Interrupt += OnMotorCompleted;
        }

        private void OnMotorCompleted(InterruptInput sender, bool value)
        {
            _left.ClearInterrupt();
            _right.ClearInterrupt();

            Debug.Print("interrupt:" + value.ToString());

            Thread.Sleep(500);
            Completed?.Invoke(this, EventArgs.Empty);
        }

        public void Initialize()
        {
            _left.Initialize();
            _left.SetSpeed(LForeDir, MotorPrescaler, MotorPps);
            _left.SetSpeed(LBackDir, MotorPrescaler, MotorPps);

            _right.Initialize();
            _right.SetSpeed(RForeDir, MotorPrescaler, MotorPps);
            _right.SetSpeed(RBackDir, MotorPrescaler, MotorPps);
        }

        public void SetCommand(ECommand command)
        {
            switch (command)
            {
                case ECommand.Fore:
                    GoForward();
                    break;
                case ECommand.Back:
                    GoBack();
                    break;
                case ECommand.TurnLeft:
                    TurnLeft();
                    break;
                case ECommand.TurnRight:
                    TurnRight();
                    break;
                case ECommand.TurnBack:
                    Turn();
                    break;
                default:
                    Stop();
                    break;
            }
        }

        public void Stop()
        {
            _left.Stop();
            _right.Stop();
        }

        public void GoForward()
        {
            _left.SetStepCount(LForeDir, MoveStraight);
            _right.SetStepCount(RForeDir, MoveStraight);

            _right.Start(RForeDir);
            _left.Start(LForeDir);
        }

        public void GoBack()
        {
            _left.SetStepCount(LBackDir, MoveStraight);
            _right.SetStepCount(RBackDir, MoveStraight);

            _right.Start(RBackDir);
            _left.Start(LBackDir);
        }

        public void TurnRight()
        {
            _left.SetStepCount(LForeDir, MoveTurn90);
            _right.SetStepCount(RBackDir, MoveTurn90);

            _right.Start(RBackDir);
            _left.Start(LForeDir);
        }

        public void TurnLeft()
        {
            _left.SetStepCount(LBackDir, MoveTurn90);
            _right.SetStepCount(RForeDir, MoveTurn90);

            _right.Start(RForeDir);
            _left.Start(LBackDir);
        }

        public void Turn()
        {
            _left.SetStepCount(LForeDir, MoveTurn180);
            _right.SetStepCount(RBackDir, MoveTurn180);
            _right.Start(RBackDir);
            _left.Start(LForeDir);
        }
    }
}

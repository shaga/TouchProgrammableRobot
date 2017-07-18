using System;
using Gadgeteer;
using Microsoft.SPOT.Hardware;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;

namespace TouchProgrammableRobotGadgeteer
{
    class Pca9629AStepper
    {
        private const int I2CClock = 400;

        private const int StepResolution = 33333;

        public const ushort DefaultAddress = 0x22;

        private const byte McntlAddr = 0x1A;

        private const byte IntStateAddr = 0x06;

        private const byte CwStepCountLow = 0x12;
        private const byte CwStepCountHigh = 0x13;
        private const byte CcwStepCountLow = 0x14;
        private const byte CcwStepCountHight = 0x15;
        private const byte CwStepPlusWidthLow = 0x16;
        private const byte CwStepPlusWidthHigh = 0x17;
        private const byte CcwStepPlusWidthLow = 0x18;
        private const byte CcwStepPlusWidthHigh = 0x19;

        public enum EDirection
        {
            CW = 0x00,
            CCW = 0x01,
        }

        private GTI.I2CBus _i2c;

        public Pca9629AStepper(ushort address, int socketNum, GTM.Module module = null)
        {
            var socket = Socket.GetSocket(socketNum, false, module, null);

            _i2c = GTI.I2CBusFactory.Create(socket, address, I2CClock, module);
        }

        public void Initialize()
        {
            var initData = new byte[]
            {
                0x80,
                0x00, 0x00, 0x07, 0x03, 0x13, 0x07,
                0x00, 0x00, 0x68, 0x00, 0x00,
                0x10, 0x80,
                0x09, 0x09, 0x01, 0x7D, 0x7D,
                0xFF, 0x01, 0xFE, 0x01, 0x05, 0x0D, 0x05, 0x0D,
                0x20,
                0xE2, 0xE4, 0xE6, 0xE0,
            };

            var transaction = I2CDevice.CreateWriteTransaction(initData);

            _i2c.Execute(transaction);
        }

        public void Start(EDirection direction)
        {
            Write(McntlAddr, (byte)(0xC0 | (int)direction));
        }

        public void Stop()
        {
            Write(McntlAddr, 0x20);
        }

        public void SetStepCount(EDirection direction, ushort count)
        {
            Write(direction == EDirection.CW ? CwStepCountLow : CcwStepCountLow, count);
        }

        public void SetSpeed(EDirection direction, byte prescaler, ushort pps)
        {
            var address = direction == EDirection.CW ? CwStepPlusWidthLow : CcwStepPlusWidthLow;

            var width = (ushort)(((prescaler & 0x07) << 13) | (pps & 0x1FFF));

            Write(address, width);
        }

        public void SetSpeed(EDirection direction, float pulsePerSec)
        {
            byte p = 0;
            var ratio = (byte)(40.6901 / pulsePerSec);

            p = (ratio & 0x01) > 0 ? (byte)1 : p;
            p = (ratio & 0x02) > 0 ? (byte)2 : p;
            p = (ratio & 0x04) > 0 ? (byte)3 : p;
            p = (ratio & 0x08) > 0 ? (byte)4 : p;
            p = (ratio & 0x10) > 0 ? (byte)5 : p;
            p = (ratio & 0x20) > 0 ? (byte)6 : p;
            p = (ratio & 0x40) > 0 ? (byte)7 : p;

            SetSpeed(direction, p, (ushort)pulsePerSec);
        }

        public void ClearInterrupt()
        {
            Write(IntStateAddr, 0x00);
        }

        private void Write(byte address, byte data)
        {
            var transaction = I2CDevice.CreateWriteTransaction(new byte[] { address, data });

            _i2c.Execute(transaction);
        }

        private void Write(byte address, ushort data)
        {
            address = (byte)(0x80 | address);

            var transaction =
                I2CDevice.CreateWriteTransaction(new byte[] { address, (byte)(data & 0xff), (byte)(data >> 8) });

            _i2c.Execute(transaction);
        }
    }
}

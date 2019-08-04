using System;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.WiringPi;

namespace Winofy.HomeConsole
{
    public class D7sAddresses
    {
        public const int Address = 0b10101011; //0x55

        public const int RegState = 0x1000;

        public const int RegAxisState = 0x1001;

        public const int RegEvent = 0x1002;

        public const int RegMode = 0x1003;

        public const int RegMainSI_H = 0x2000;
    }

    public enum D7sState
    {
        Normal = 0x00,
        NormalNotInStandby = 0x01,
        InitialInstallation = 0x02,
        OffsetAcquisition = 0x03,
        SelfTest = 0x04
    }

    public enum D7sMode
    {
        Normal = 0x00,
        InitialInstallation = 0x02,
        OffsetAcquisition = 0x03,
        SelfTest = 0x04
    }

    public enum D7sAxis
    {
        YZ = 0,
        XZ = 1,
        XY = 2,
    }

    public class D7s
    {
        public II2CDevice Device { get; }

        public D7s(int deviceId = D7sAddresses.Address)
        {
            Device = Pi.I2C.AddDevice(deviceId);
        }

        public void Reset()
        {
            SetMode(D7sMode.InitialInstallation);
            System.Threading.Thread.Sleep(2000);
        }

        public float ReadSI()
        {
            var si = Device.ReadAddressWord(D7sAddresses.RegMainSI_H);
            return si / 10f;
        }

        public D7sState GetState()
        {
            var state = (D7sState)(Device.ReadAddressByte(D7sAddresses.RegState) & 0b00000111);
            return state;
        }

        public D7sAxis GetAxis()
        {
            var axis = (D7sAxis)(Device.ReadAddressByte(D7sAddresses.RegAxisState) & 0b00000011);
            return axis;
        }

        public void SetMode(D7sMode mode)
        {
            Device.WriteAddressByte(D7sAddresses.RegMode, (byte)mode);
        }
    }
}

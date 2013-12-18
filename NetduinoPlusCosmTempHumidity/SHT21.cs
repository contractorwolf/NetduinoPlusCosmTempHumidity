using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;

class SHT21
{
    byte[] CommandReadRelativeHumidity = new byte[1] { 0xE5 };//0xE5 or 11100101 is the "read relative humidity" command
    byte[] RelativeHumidityValueBytes = new byte[3];

    byte[] CommandReadTemperature = new byte[1] { 0xE3 };//0xE3 or 11100011 is the "read temperature" command
    byte[] TemperatureValueBytes = new byte[3];

    double dec;
    double temperature;
    double humidity;
    double dbl;
    double fahrenheit;
    int result;

    I2CDevice.Configuration config = new I2CDevice.Configuration(0x40, 100);  //device address is 0x40 or 1000000 (7 bit address)
    I2CDevice MyI2C;

    // setup the transactions
    I2CDevice.I2CTransaction[] xActions = new I2CDevice.I2CTransaction[2];
    int TempReadCount = 0;
    int HumidityReadCount = 0;



    public SHT21()
    {
        Thread.Sleep(500);
        MyI2C = new I2CDevice(config);
    }

    public double GetHumidity()
    {
        // create write buffer, send "read humidity" command
        xActions[0] = I2CDevice.CreateWriteTransaction(CommandReadRelativeHumidity);

        // create read buffer to read the returned data
        xActions[1] = I2CDevice.CreateReadTransaction(RelativeHumidityValueBytes);

        //run the write/ read
        result = MyI2C.Execute(xActions, 1000);

        //RelativeHumidityValueBytes[0] is MSB
        //RelativeHumidityValueBytes[1] is LSB
        //RelativeHumidityValueBytes[2] is checksum

        dec = ConvertMSBLSBToDouble(RelativeHumidityValueBytes[0], RelativeHumidityValueBytes[1]);

        humidity = (125 * (dec / 65536)) - 6;

        HumidityReadCount++;

        return (humidity);

    }


    public double GetTemperature()
    {

        // create write buffer (we need one byte)
        xActions[0] = I2CDevice.CreateWriteTransaction(CommandReadTemperature);

        // create read buffer to read the returned data
        xActions[1] = I2CDevice.CreateReadTransaction(TemperatureValueBytes);

        //run the write/ read
        MyI2C.Execute(xActions, 1000);

        //TemperatureValueBytes[0] is MSB
        //TemperatureValueBytes[1] is LSB
        //TemperatureValueBytes[2] is checksum


        dec = ConvertMSBLSBToDouble(TemperatureValueBytes[0], TemperatureValueBytes[1]);

        //
        temperature = (175.72 * (dec / 65536)) - 46.85;

        TempReadCount++;

        return (CovertCelciusToFahrenheit(temperature));

    }

    private double ConvertMSBLSBToDouble(byte MSB, byte LSB)
    {
        dbl = (MSB + (LSB / 256)) * 256;

        return (dbl);
    }

    private double CovertCelciusToFahrenheit(double celcius)
    {
        fahrenheit = ((celcius * 9) / 5) + 32;

        return (fahrenheit);
    }
}


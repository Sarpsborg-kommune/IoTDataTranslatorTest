using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sarpsborgkommune.iot
{
    // See https://www.elsys.se/en/elsys-payload/
    public enum ElsysMeasurementType : byte
    {
        TEMP = 0x01,   // Temp 2 bytes -3276.8°C --> 3276.7°C
        RH = 0x02,   // Humidity 1 byte 0-100%
        ACC = 0x03,   // Acceleration 3 bytes X,Y,Z -127 --> 127 +/-63=1G
        LIGHT = 0x04,   // Light 2 bytes 0-->65535 Lux
        MOTION = 0x05,   // No of motion 1 byte 0-255
        CO2 = 0x06,   // CO₂ 2 bytes 0-65535 ppm
        VDD = 0x07,   // VDD(Battery Level) 2 bytes 0-65535mV
        ANALOG1 = 0x08,   // VDD 2 bytes 0-65535mV
        GPS = 0x09,   // 3 bytes lat 3 bytes long binary
        PULSE1 = 0x0A,   // 2 bytes relative pulse count
        PULSE1_ABS = 0x0B,   // 4 bytes no 0->0xFFFFFFFF
        EXT_TEMP1 = 0x0C,   // 2 bytes -3276.5°C-->3276.5°C
        EXT_DIGITAL = 0x0D,   // 1 bytes value 1 or 0
        EXT_DISTANCE = 0x0E,   // 2 bytes distance in mm
        ACC_MOTION = 0x0F,   // 1 byte number of vibration/motion
        IR_TEMP = 0x10,   // 2 bytes internal temp 2 bytes external temp -3276.5°C-->3276.5°C
        OCCUPANCY = 0x11,   // 1 byte data
        WATERLEAK = 0x12,   // 1 byte data 0-255
        GRIDEYE = 0x13,   // 65 byte temperature data 1 byte ref+64byte external temp
        PRESSURE = 0x14,   // 4 byte pressure data (hPa)
        SOUND = 0x15,   // 2 byte sound data (peak/avg)
        PULSE2 = 0x16,   // 2 bytes 0-->0xFFFF
        PULSE2_ABS = 0x17,   // 4 bytes no 0->0xFFFFFFFF
        ANALOG2 = 0x18,   // 2 bytes voltage in mV
        EXT_TEMP2 = 0x19,   // 2 bytes -3276.5°C-->3275.5°C
        EXT_DIGITAL2 = 0x1A,   // 1 bytes value 1 or 0
        EXT_ANALOG_UV = 0x1B,   // 4 bytes signd int (uV)
        DEBUG = 0x3D    // 4 bytes debug
    }

    public class Acc
    {
        public int x;
        public int y;
        public int z; 
    }

    public class Gps
    {
        public int lat;
        public int lon; 
    }

    public class IrTemp
    {
        public int i;
        public int e;
    }

    public class Sound
    {
        public int peak;
        public int avg;
    }

    public class Grideye
    {
        public float reference;
        public float[] grideye;
    }

    public class ElsysMessage
    {
        public float? temp { get; }
        public int? rh { get; }
        public Acc acc { get; }
        public int? light { get; }
        public int? motion { get; }
        public int? co2 { get; }
        public int? vdd { get; }
        public int? analog1 { get; }
        public Gps gps { get; }
        public int? pulse1 { get; }
        public int? pulse1_abs { get; }
        public float? ext_temp1 { get; }
        public bool? ext_digital { get; }
        public int? ext_distance { get; }
        public int? acc_motion { get; }
        public IrTemp ir_temp { get; }
        public int? occupancy { get; }
        public int? waterleak { get; }
        public Grideye grideye { get; }
        public int? pressure { get; }
        public Sound sound { get; }
        public int? pulse2 { get; }
        public int? pulse2_abs { get; }
        public int? analog2 { get; }
        public float? ext_temp2 { get; }
        public bool? ext_digital2 { get; }
        public int? ext_analog_uv { get; }
        public byte[] debug { get; }

        public ElsysMessage(byte[] data)
        {
            for (int counter = 0; counter < data.Length; counter ++)
            {
                switch (data[counter])
                {
                    case (byte)ElsysMeasurementType.TEMP:
                        this.temp = (data[counter + 1] << 8 | data[counter + 2]) / 10;
                        counter += 2;
                        break;
                    case (byte)ElsysMeasurementType.RH:
                        this.rh = (int)data[counter + 1];
                        counter += 1;
                        break;
                    case (byte)ElsysMeasurementType.ACC:
                        this.acc = new Acc();
                        this.acc.x = data[counter + 1];
                        this.acc.y = data[counter + 2];
                        this.acc.z = data[counter + 3];
                        counter += 3;
                        break;
                    case (byte)ElsysMeasurementType.LIGHT:
                        this.light = (data[counter + 1] << 8) | data[counter + 2];
                        counter += 2;
                        break;
                    case (byte)ElsysMeasurementType.MOTION:
                        this.motion = (int)data[counter + 1];
                        counter += 1;
                        break;
                    case (byte)ElsysMeasurementType.CO2:
                        this.co2 = (data[counter + 1] << 8) | data[counter + 2];
                        counter += 2;
                        break;
                    case (byte)ElsysMeasurementType.VDD:
                        this.vdd = (data[counter + 1] << 8) | data[counter + 2];
                        counter += 2;
                        break;
                    case (byte)ElsysMeasurementType.ANALOG1:
                        this.analog1 = (data[counter + 1] << 8) | data[counter + 2];
                        counter += 2;
                        break;
                    case (byte)ElsysMeasurementType.GPS:
                        // The coordinates must be converted from a signed 24 bit number to int a 
                        // signed 32 bit number (= int)
                        this.gps = new Gps();
                        this.gps.lat = data[counter + 1] | data[counter + 2] << 8 | data[counter + 4] << 16;
                        if ((data[counter + 3] & 0x80) != 0) this.gps.lat |= 0xff << 24;
                        this.gps.lat /= 10000;
                        this.gps.lat = data[counter + 4] | data[counter + 5] << 8 | data[counter + 6] << 16;
                        if ((data[counter + 6] & 0x80) != 0) this.gps.lat |= 0xff << 24;
                        this.gps.lat /= 10000;
                        counter += 6;
                        break;
                    case (byte)ElsysMeasurementType.PULSE1:
                        this.pulse1 = data[counter + 1] << 8 | data[counter + 2];
                        counter += 2;
                        break;
                    case (byte)ElsysMeasurementType.PULSE1_ABS:
                        this.pulse1_abs = (data[counter + 1] << 24) | (data[counter + 2] << 16) | (data[counter + 3] << 8) | data[counter + 4];
                        counter += 4;
                        break;
                    case (byte)ElsysMeasurementType.EXT_TEMP1:
                        this.ext_temp1 = helperfunctions.Bin16Dec(data[counter + 1], data[counter + 2]);
                        counter += 2;
                        break;
                    case (byte)ElsysMeasurementType.EXT_DIGITAL:
                        this.ext_digital = data[counter + 1] == 0 ? false : true;
                        counter += 1;
                        break;
                    case (byte)ElsysMeasurementType.EXT_DISTANCE:
                        this.ext_distance = (data[counter + 1] << 8) | data[counter + 2];
                        counter += 2;
                        break;
                    case (byte)ElsysMeasurementType.ACC_MOTION:
                        this.acc_motion = data[counter + 1];
                        counter += 1;
                        break;
                    case (byte)ElsysMeasurementType.IR_TEMP:
                        this.ir_temp = new IrTemp();
                        this.ir_temp.i = helperfunctions.Bin16Dec(data[counter + 1], data[counter + 2]) / 10;
                        this.ir_temp.e = helperfunctions.Bin16Dec(data[counter + 3], data[counter + 4]) / 10;
                        counter += 4;
                        break;
                    case (byte)ElsysMeasurementType.OCCUPANCY:
                        this.occupancy = data[counter + 1];
                        counter += 1;
                        break;
                    case (byte)ElsysMeasurementType.WATERLEAK:
                        this.waterleak = data[counter + 1];
                        counter += 1;
                        break;
                    case (byte)ElsysMeasurementType.GRIDEYE:
                        this.grideye = new Grideye();
                        this.grideye.reference = data[counter + 1];
                        counter++;
                        this.grideye.grideye = new float[64];
                        for (int innercounter = 0; innercounter < 64; innercounter++)
                            this.grideye.grideye[innercounter] = this.grideye.reference + (data[1 + counter + innercounter] / (float)10.0);
                        counter += 64;
                        break;
                    case (byte)ElsysMeasurementType.PRESSURE:
                        this.pressure = ((data[counter + 1] << 24) | (data[counter + 2] << 16) | (data[counter + 3] << 8) | data[counter + 4]) / 1000;
                        break;
                    case (byte)ElsysMeasurementType.SOUND:
                        this.sound = new Sound();
                        this.sound.peak = data[counter + 1];
                        this.sound.avg = data[counter + 2];
                        counter += 2;
                        break;
                    case (byte)ElsysMeasurementType.PULSE2:
                        this.pulse2 = (data[counter + 1] << 8) | data[counter + 2];
                        counter += 2;
                        break;
                    case (byte)ElsysMeasurementType.PULSE2_ABS:
                        this.pulse2_abs = (data[counter + 1] << 24) | (data[counter + 2] << 16) | (data[counter + 3] << 8) | data[counter + 4];
                        counter += 4;
                        break;
                    case (byte)ElsysMeasurementType.ANALOG2:
                        this.analog2 = (data[counter + 1] << 8) | data[counter + 2];
                        counter += 2;
                        break;
                    case (byte)ElsysMeasurementType.EXT_TEMP2:
                        this.ext_temp2 = helperfunctions.Bin16Dec(data[counter + 1], data[counter + 2]) / 10;
                        counter += 2;
                        break;
                    case (byte)ElsysMeasurementType.EXT_DIGITAL2:
                        this.ext_digital = data[counter + 1] == 0 ? false : true;
                        counter += 1;
                        break;
                    case (byte)ElsysMeasurementType.EXT_ANALOG_UV:
                        this.ext_analog_uv = (data[counter + 1] << 24) | (data[counter + 2] << 16) | (data[counter + 3] << 8) | data[counter + 4];
                        counter += 4;
                        break;
                    default:
                        break;
                }
            }
        }

        public override string ToString()
        {
            string output = "";

            if (!(this.temp is null)) output += $"Temperature: {this.temp}°C\n";
            if (!(this.rh is null)) output += $"Humidity: {this.rh}%\n";
            if (!(this.acc is null)) output += $"Acceleration: {this.acc.x},{this.acc.y},{this.acc.z}\n";
            if (!(this.light is null)) output += $"Light: {this.light} Lux\n";
            if (!(this.motion is null)) output += $"Motion: {this.motion}\n";
            if (!(this.co2 is null)) output += $"CO₂: {this.co2} ppm\n";
            if (!(this.vdd is null)) output += $"Battery Level: {this.vdd} mV\n";
            if (!(this.analog1 is null)) output += $"Analog1: {this.analog1} mV\n";
            if (!(this.gps is null)) output += $"GPS: lat:{this.gps.lat} lon:{this.gps.lon}\n";
            if (!(this.pulse1 is null)) output += $"Relative pulse count 1: {this.pulse1}\n";
            if (!(this.pulse1_abs is null)) output += $"Absolute pulse count 1: {this.pulse1_abs}\n";
            if (!(this.ext_temp1 is null)) output += $"External temperature 1: {this.ext_temp1}°C\n";
            if (!(this.ext_digital is null)) output += $"Digital signal: {this.ext_digital}\n";
            if (!(this.ext_distance is null)) output += $"Distance: {this.ext_distance}mm\n";
            if (!(this.acc_motion is null)) output += $"Vibration/Motion: {this.acc_motion}\n";
            if (!(this.ir_temp is null))
            {
                output += $"Internal temperature: {this.ir_temp.i}°C\n";
                output += $"External temperature: {this.ir_temp.e}°C\n";
            }
            if (!(this.occupancy is null)) output += $"Occupancy: {this.occupancy}\n";
            if (!(this.waterleak is null)) output += $"Waterleak: {this.occupancy}\n";
            if (!(this.grideye is null))
            {
                output += $"Grideye - Reference temperature: {this.grideye.reference}°C\n";
                for (int y = 1; y <= 8; y++)
                {
                    for (int x = 1; x <= 8; x++)
                    {
                        output += $"{grideye.grideye[x * y]}  ";
                    }
                    output += "\n";
                }
                    
            }
            if (!(this.pressure is null)) output += $"Pressure: {this.pressure}hPa\n";
            if (!(this.sound is null)) output += $"Sound: Peak:{this.sound.peak} Average:{this.sound.avg}\n";
            if (!(this.pulse2 is null)) output += $"Relative pulse count 2: {this.pulse2}\n";
            if (!(this.pulse2_abs is null)) output += $"Absolute pulse count 2: {this.pulse2_abs}\n";
            if (!(this.analog2 is null)) output += $"Analog 2: {this.analog2}mV\n";
            if (!(this.ext_temp2 is null)) output += $"External temperature 2: {this.ext_temp2}°C\n";
            if (!(this.ext_digital2 is null)) output += $"Digital Signal 2: {this.ext_digital2}\n";
            if (!(this.ext_analog_uv is null)) output += $"Analog UV: {this.ext_analog_uv}uV\n";

            return output;
        }

        public string toJson()
        {
            string jsonString;
            var options = new JsonSerializerOptions { IgnoreNullValues = true };
            jsonString = JsonSerializer.Serialize(this, options);
            return jsonString;
        }
    }
}

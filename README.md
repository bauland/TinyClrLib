# TinyClr Library
Version: __1.0.0.10002-preview3__

Note: If environment variable __NUGET_REPOSITORY__ is define, some project generate a Nuget package for them, and copy generated package into it.

Documentation is in progress (each file of module documentation is in project folder with MarkDown File)

***
:construction: : Work In Progress
***

## Core:
Module              | Roles       | Package                                                                                            
------------------- | ----------- | -------------------------------------------------------------------------------------------------- 
 __Application__ | Class with Initialization and loop function | Not yet
__Led__ | Class to help with test easily |
__Button__ | Class to help with test easily |
__Pins__ | Include some boards which don't appear in GHI.TinyClr.Pins | [Package](https://www.nuget.org/packages/Bauland.Pins/)

## Modules:

### Adafruit

Module              | State       | Package                                                                                             | Documentation
------------------- | ----------- | --------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------
Color Sensor (1334) | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Adafruit.ColorSensor1334/) | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Adafruit/ColorSensor1334/ColorSensor1334.md) 
NeoPixel (Stick, Shield ...) | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Adafruit.NeoPixel/) | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Adafruit/ColorSensor1334/ColorSensor1334.md) 

### Gadgeteer

Module           | State       | Package                                                                                               | Documentation
---------------- | ----------- | ----------------------------------------------------------------------------------------------------- | -------------
AccelG248        | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Gadgeteer.AccelG248/)        | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Gadgeteer/AccelG248/AccelG248.md)
Bluetooth		 | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Gadgeteer.Bluetooth/)        | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Gadgeteer/Bluetooth/Bluetooth.md)
Button           | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Gadgeteer.Button/)           | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Gadgeteer/Button/Button.md)
CharacterDisplay | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Gadgeteer.CharacterDisplay/) | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Gadgeteer/CharacterDisplay/CharacterDisplay.md)
DisplayN18       | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Gadgeteer.DisplayN18/)       | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Gadgeteer/DisplayN18/DisplayN18.md)
Gyro             | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Gadgeteer.Gyro/)             | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Gadgeteer/Gyro/Gyro.md)
Led7C            | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Gadgeteer.Led7C/)            | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Gadgeteer/Led7C/Led7C.md)
Led7R            | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Gadgeteer.Led7R/)            | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Gadgeteer/Led7R/Led7R.md)
LedStrip         | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Gadgeteer.LedStrip/)         | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Gadgeteer/LedStrip/LedStrip.md)
LightSense       | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Gadgeteer.LightSense/)       | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Gadgeteer/LightSense/LightSense.md)
MotorDriverL298  | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Gadgeteer.MotorDriverL298/)  | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Gadgeteer/MotorDriverL298/MotorDriverL298.md)
Music            | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Gadgeteer.Music/)            | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Gadgeteer/Music/Music.md)
Potentiometer    | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Gadgeteer.Potentiometer/)    | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Gadgeteer/Potentiometer/Potentiometer.md)
RotaryH1         | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Gadgeteer.RotaryH1/)         | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Gadgeteer/RotaryH1/RotaryH1.md)
TempHumid        | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Gadgeteer.TempHumid/)        | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Gadgeteer/TempHumid/TempHumid.md)
Tunes            | __Working__ | [Package](https://www.nuget.org/packages/Bauland.Gadgeteer.Tunes/)            | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Gadgeteer/Tunes/Tunes.md)

### Grove

Module          | State          | Package                                                                                           | Documentation                                                                                                           | Notes
--------------- | -------------- | ------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- | -----
Button          | __Working__    | [Package](https://www.nuget.org/packages/Bauland.Grove.Button/)           | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Grove/Button/Button.md)                       | 
Buzzer          | __Working__    | [Package](https://www.nuget.org/packages/Bauland.Grove.Buzzer/)           | [Documentation](https://github.com/bauland/TinyClrLib/tree/master/Modules/Grove/Buzzer/Buzzer.md)                       |
4-Digit Display | __Working__    | [Package](https://www.nuget.org/packages/Bauland.Grove.FourDigitDisplay/) | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Grove/FourDigitDisplay/FourDigitDisplay.md)   |
I2C Color       | __Working__    | [Package](https://www.nuget.org/packages/Bauland.Grove.I2cColorSensor/)   | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Grove/I2cColorSensor/I2cColorSensor.md)       |
LcdRgbBacklight | __Working__    | [Package](https://www.nuget.org/packages/Bauland.Grove.LcdRgbBacklight/)  | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Grove/LcdRgbBacklight/LcdRgbBacklight.md)     |
LedSocket       | __Working__    | [Package](https://www.nuget.org/packages/Bauland.Grove.Led/)              | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Grove/Led/Led.md)                             | from GHI
LightSensor     | __Working__    | [Package](https://www.nuget.org/packages/Bauland.Grove.LightSensor/)      | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Grove/LightSensor/LightSensor.md)             | from GHI
Relay           | __Working__    | [Package](https://www.nuget.org/packages/Bauland.Grove.Relay/)            | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Grove/Relay/Relay.md)                         | from GHI
Rotary Angle    | __Working__    | [Package](https://www.nuget.org/packages/Bauland.Grove.RotaryAngleSensor/)| [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Grove/RotaryAngleSensor/RotaryAngleSensor.md) | from GHI
Rtc             | __Working__    | [Package](https://www.nuget.org/packages/Bauland.Grove.Rtc/)              | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Grove/Rtc/Rtc.md)                             |
SerialBluetooth3| __Working__    | [Package](https://www.nuget.org/packages/Bauland.Grove.SerialBluetooth3/) | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Grove/SerialBluetooth3/SerialBluetooth3.md)   | 
ServoMotor      | __Working__    |                                                                                                   | Not yet                                                                                                                 | from GHI
Sound           | __Working__    | [Package](https://www.nuget.org/packages/Bauland.Grove.SoundSensor/)      | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Grove/SoundSensor/SoundSensor.md)             | from GHI
Temperature     | __Working__    | [Package](https://www.nuget.org/packages/Bauland.Grove.SoundSensor/)      | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Grove/TemperatureSensor/TemperatureSensor.md) | from GHI
Thumb Joystick  | __Working__    | [Package](https://www.nuget.org/packages/Bauland.Grove.ThumbJoystick/)    | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Grove/Thumb_Joystick/Thumb_Joystick.md)       |
TouchSensor     | __Working__    | [Package](https://www.nuget.org/packages/Bauland.Grove.TemperatureSensor/)| [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Grove/TouchSensor/TouchSensor.md)             | from GHI 
UltrasonicRanger| __Working__    | [Package](https://www.nuget.org/packages/Bauland.Grove.UltrasonicRanger/) | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Grove/UltrasonicRanger/UltrasonicRanger.md)   |

### Mikro Click

Module                | State          | Package     | Documentation
--------------------- | -------------- | ----------- | -------------
Bluetooth LE P module | :construction: |             | Not yet

### Others

Module          | State          | Package                                                                                                                  | Documentation
--------------- | -------------- | ------------------------------------------------------------------------------------------------------------------------ | -------------
HC-SR04         | __Working__    | [Package](https://www.nuget.org/packages/Bauland.Others.HCSR04/)          | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Others/HCSR04/HC-SR04.md)
LedStrip        | __Working__    | [Package](https://www.nuget.org/packages/Bauland.Others.LedStrip/)        | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Others/LedStrip_APA102/LedStrip.md)
LedStripEffects | __Working__    | [Package](https://www.nuget.org/packages/Bauland.Others.LedStripEffects/) | [Documentation](https://github.com/bauland/TinyClrLib/blob/master/Modules/Others/LedStripEffects/LedStripEffects.md)
RC522 Rfid      | :construction: |                                                                                                                          | Not yet

***
:construction: : Work In Progress
***
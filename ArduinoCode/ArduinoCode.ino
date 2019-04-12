#define VERSION 2

#include "Wire.h"
#include "LCD.h"
#include "NewLiquidCrystal_I2C.h"

#define I2C_ADDR 0x27  // Character LCD I2C address
//My LCD is a 20x4, and this program is designed for that. If your screen is a different width, it should be able to handle.
#define SCREEN_WIDTH 20
#define SCREEN_HEIGHT 4
NewLiquidCrystal_I2C lcd(I2C_ADDR, 2, 1, 0, 4, 5, 6, 7); // Initialise with LCD pin mappings on I2C device along with I2C address

//Custom datastructure to hold time information
struct Timecode {
  int seconds = 0;
  int minutes = 0;
  int hours = 0;
};
Timecode durationtime; //contains duration of the song
Timecode positiontime; //contains current position into song

//http://forum.arduino.cc/index.php?topic=209407.0
boolean isValidNumber(String str) {
  bool isADigit = true;
  for (byte i = 0; i < str.length(); i++)
  {
    if (!isDigit(str.charAt(i))) {
      isADigit = false;
    }
  }
  return isADigit;
}

void setup()
{
  Serial.begin(115200); //initialise serial port
  lcd.begin (SCREEN_WIDTH, SCREEN_HEIGHT); // Initialise LCD screen

  // LCD Backlight ON
  lcd.setBacklightPin(3, POSITIVE);
  lcd.setBacklight(HIGH);

  lcd.home();
  lcd.print("Awaiting");
  lcd.setCursor(0, 1); //start of 2nd line
  lcd.print("Connection...");

  //we are good to go!
  Serial.println("Online and ready.");
}

//given a timecode, convert it into seconds.
long convert_to_seconds(Timecode tcode) {
  long seconds = tcode.seconds;
  seconds += tcode.minutes * 60;
  seconds += tcode.hours * 60 * 60;
  return seconds;
}

String input; // for incoming serial data
/* -=List of accepted commands=-
    Commands that take parameters will accept them on newlines.

   ver: Prints the software version
   title: Sets the title of the song
   artist: Sets the artist of the song
   d: Sets the duration of the song.
   p: Sets the current position into a song
   c: Clears the LCD screen
*/
void loop()
{
  if (Serial.available() > 0) {
    input = Serial.readStringUntil('\n');
    input.trim();

if (input == "ver") { //prints the software version
Serial.print("Music Readout I2C Character Display Interface (version ");
Serial.print(VERSION);
Serial.println(")");
}
else if (input == "c") { //clears the screen
  lcd.clear();
  Serial.println("OK");
  }
else    if (input == "title") { //sets the song title
      Serial.print("?");
      for (;;) {
        if (Serial.available() > 0) {
          input = Serial.readStringUntil('\n');
          input.trim();

          lcd.home(); // go home on LCD
          //Write as much as the song title as possible
          for (int i = 0; i < SCREEN_WIDTH; i++)
          {
            //print the text...
            if (i < input.length()) lcd.print(input[i]);
            //until it runs out, at which point we pad it with empty spaces.
            else { lcd.print(" ");}
          }
          break;
        }
      }
      Serial.println("OK");
    }

    else if (input == "artist") { //sets the song artist
      Serial.print("?");
      for (;;) {
        if (Serial.available() > 0) {
          input = Serial.readStringUntil('\n');
          input.trim();

          lcd.setCursor(0, 1); //start of 2nd line
          //Write as much as the song author as possible
          for (int i = 0; i < SCREEN_WIDTH; i++)
          {
            //print the text...
            if (i < input.length()) lcd.print(input[i]);
            //until it runs out, at which point we pad it with empty spaces.
            else { lcd.print(" ");}
          }
          break;
        }
      }
      Serial.println("OK");
    }

    else if (input == "p") //position updater
    {
      Serial.print("h");
      for (;;) {
        if (Serial.available() > 0) {
          input = Serial.readStringUntil('\n');
          input.trim();

          if (isValidNumber(input)) positiontime.hours = input.toInt();
          break;
        }
      }
      Serial.print("m");
      for (;;) {
        if (Serial.available() > 0) {
          input = Serial.readStringUntil('\n');
          input.trim();

          if (isValidNumber(input)) positiontime.minutes = input.toInt();
          break;
        }
      }
      Serial.print("s");
      for (;;) {
        if (Serial.available() > 0) {
          input = Serial.readStringUntil('\n');
          input.trim();

          if (isValidNumber(input)) positiontime.seconds = input.toInt();
          break;
        }
      }

      lcd.setCursor(0, 2); // go to start of 3rd line
      int allocatedspace = (SCREEN_WIDTH - 1) / 2; // calculate the amount of space available to a 'side'
      //construct line to output (e.g. 0:2:14)
      String outputline = "";
      outputline.concat(positiontime.hours);
      outputline.concat(":");
      outputline.concat(positiontime.minutes);
      outputline.concat(":");
      outputline.concat(positiontime.seconds);
      for (int i = 0; i < allocatedspace - outputline.length(); i++) lcd.print(" "); //padding with empty space
      lcd.print(outputline);
      lcd.print("/");

      //Progress Bar
      lcd.setCursor(0, 3); //4th line
      //convert the duration and length into seconds
      long value = convert_to_seconds(positiontime);
      long maxvalue = convert_to_seconds(durationtime);
      //adjust the bar to fit the space it has
      value = map(value, 0, maxvalue, 0, SCREEN_WIDTH - 5);
      maxvalue = SCREEN_WIDTH - 5;
      //construct the bar as a string
      outputline = "["; //start with the bar prefix
      for (int i = 0; i <= value; i++) outputline.concat("="); //the currently filled portion of the bar
      outputline.concat(">"); //pointer
      for (int i = 0; i <= maxvalue - value; i++) outputline.concat(" "); //the empty portion of the bar
      outputline.concat("]"); //end of bar
      lcd.print(outputline);

      Serial.println("OK");
    }

    else if (input == "d") { //sets the song length in the h:m:s format separated by newlines
      Serial.print("h");
      for (;;) {
        if (Serial.available() > 0) {
          input = Serial.readStringUntil('\n');
          input.trim();

          if (isValidNumber(input)) durationtime.hours = input.toInt();
          break;
        }
      }
      Serial.print("m");
      for (;;) {
        if (Serial.available() > 0) {
          input = Serial.readStringUntil('\n');
          input.trim();

          if (isValidNumber(input)) durationtime.minutes = input.toInt();
          break;
        }
      }
      Serial.print("s");
      for (;;) {
        if (Serial.available() > 0) {
          input = Serial.readStringUntil('\n');
          input.trim();

          if (isValidNumber(input)) durationtime.seconds = input.toInt();
          break;
        }
      }

      int allocatedspace = (SCREEN_WIDTH - 1) / 2; // calculate the amount of space available to a 'side'
      lcd.setCursor(allocatedspace + 1, 2); // go to halfway of the 3rd line
      //construct time string (e.g. 1:5:36)
      String outputline = "";
      outputline.concat(durationtime.hours);
      outputline.concat(":");
      outputline.concat(durationtime.minutes);
      outputline.concat(":");
      outputline.concat(durationtime.seconds);
      lcd.print(outputline);

      Serial.println("OK");
    }
  }
}

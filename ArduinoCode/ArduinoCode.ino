#include "Wire.h"
#include "LCD.h"
#include "NewLiquidCrystal_I2C.h"

#define I2C_ADDR 0x27  // Character LCD I2C address
#define SCREEN_WIDTH 20
#define SCREEN_HEIGHT 4
NewLiquidCrystal_I2C lcd(I2C_ADDR, 2, 1, 0, 4, 5, 6, 7); // Initialise with LCD pin mappings on I2C device along with I2C address

//Custom datastructure to hold information about a song
struct Timecode {
  int seconds = 0;
  int minutes = 0;
  int hours = 0;
};
struct Song {
  String title = "Awaiting";
  String artist = "connection...";
  Timecode duration;
  Timecode length;
};
Song song;

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
  lcd.begin (SCREEN_WIDTH, SCREEN_HEIGHT); // My LCD is a 20x4, change for your LCD if needed

  // LCD Backlight ON
  lcd.setBacklightPin(3, POSITIVE);
  lcd.setBacklight(HIGH);

  //Draw initial screen state
  DrawScreen();

  //we are good to go!
  Serial.println("Online and ready.");
}

//given a timecode, convert it into seconds.
long convert_to_seconds(Timecode tcode){
  long seconds = tcode.seconds;
  seconds += tcode.minutes*60;
  seconds += tcode.hours*60*60;
  return seconds;
}
//Redraws the entire screen
void DrawScreen()
{
  lcd.home(); // go home on LCD
  lcd.clear(); //clear screen

  //Write as much as the song title as possible
  for (int i = 0; i < song.title.length(); i++)
  {
    //check we don't exceed screen width
    if (i < SCREEN_WIDTH) lcd.print(song.title[i]);
  }

  lcd.setCursor(0, 1); //start of 2nd line
  //Write as much as the song author as possible
  for (int i = 0; i < song.artist.length(); i++)
  {
    //check we don't exceed screen width
    if (i < SCREEN_WIDTH) lcd.print(song.artist[i]);
  }

  lcd.setCursor(0, 2); // go to start of 3rd line
  int allocatedspace = (SCREEN_WIDTH - 1) / 2; // calculate the amount of space available to each 'side'
  //construct line to output (e.g. 0:2:14)
  String outputline = "";
  outputline.concat(song.duration.hours);
  outputline.concat(":");
  outputline.concat(song.duration.minutes);
  outputline.concat(":");
  outputline.concat(song.duration.seconds);
  for (int i = 0; i < allocatedspace - outputline.length(); i++) lcd.print(" "); //padding with empty space
  lcd.print(outputline);
  lcd.print("/");
  //construct time string (e.g. 1:5:36)
  outputline = "";
  outputline.concat(song.length.hours);
  outputline.concat(":");
  outputline.concat(song.length.minutes);
  outputline.concat(":");
  outputline.concat(song.length.seconds);
  lcd.print(outputline);

  lcd.setCursor(0, 3); //4th line
  //convert the duration and length into seconds
  long value = convert_to_seconds(song.duration);
  long maxvalue = convert_to_seconds(song.length);
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
}


String input; // for incoming serial data
/* -=List of accepted commands=-
    Commands that take parameters will accept them on newlines.

   title: Sets the title of the song
   artist: Sets the artist of the song
   length: Sets the length of the song.
   d: Sets the current duration into a song
   u: Redraws LCD screen with set information.
*/
void loop()
{
  if (Serial.available() > 0) {
    input = Serial.readStringUntil('\n');
    input.trim();
    Serial.println(input);

    if (input == "title") { //sets the song title
      Serial.print("?");
      for (;;) {
        if (Serial.available() > 0) {
          input = Serial.readStringUntil('\n');
          input.trim();

          song.title = input;
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

          song.artist = input;
          break;
        }
      }
      Serial.println("OK");
    }

    else if (input == "length") { //sets the song length in the h:m:s format separated by newlines
      Serial.print("h");
      for (;;) {
        if (Serial.available() > 0) {
          input = Serial.readStringUntil('\n');
          input.trim();

          if (isValidNumber(input)) song.length.hours = input.toInt();
          break;
        }
      }
      Serial.print("m");
      for (;;) {
        if (Serial.available() > 0) {
          input = Serial.readStringUntil('\n');
          input.trim();

          if (isValidNumber(input)) song.length.minutes = input.toInt();
          break;
        }
      }
      Serial.print("s");
      for (;;) {
        if (Serial.available() > 0) {
          input = Serial.readStringUntil('\n');
          input.trim();

          if (isValidNumber(input)) song.length.seconds = input.toInt();
          break;
        }
      }

      Serial.println("OK");
    }

    else if (input == "d") //duration updater
    {
      Serial.print("h");
      for (;;) {
        if (Serial.available() > 0) {
          input = Serial.readStringUntil('\n');
          input.trim();

          if (isValidNumber(input)) song.duration.hours = input.toInt();
          break;
        }
      }
      Serial.print("m");
      for (;;) {
        if (Serial.available() > 0) {
          input = Serial.readStringUntil('\n');
          input.trim();

          if (isValidNumber(input)) song.duration.minutes = input.toInt();
          break;
        }
      }
      Serial.print("s");
      for (;;) {
        if (Serial.available() > 0) {
          input = Serial.readStringUntil('\n');
          input.trim();

          if (isValidNumber(input)) song.duration.seconds = input.toInt();
          break;
        }
      }

      Serial.println("OK");
    }

    else if (input == "u") DrawScreen(); //redraw screen
  }
}

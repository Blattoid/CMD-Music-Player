#include "Wire.h"
#include "LCD.h"
#include "NewLiquidCrystal_I2C.h"

#define I2C_ADDR 0x27  // Character LCD I2C address
#define SCREEN_WIDTH 20
#define SCREEN_HEIGHT 4
NewLiquidCrystal_I2C lcd(I2C_ADDR, 2, 1, 0, 4, 5, 6, 7); // Initialise with LCD pin mappings on I2C device along with I2C address

//Custom datastructure to hold information about a song
struct Timecode {
  int seconds;
  int minutes;
  int hours;
};
struct Song {
  String title;
  String author;
  Timecode duration;
  Timecode length;
};
Song song;

void setup()
{
  lcd.begin (SCREEN_WIDTH, SCREEN_HEIGHT); // My LCD is a 20x4, change for your LCD if needed

  // LCD Backlight ON
  lcd.setBacklightPin(3, POSITIVE);
  lcd.setBacklight(HIGH);
  lcd.home(); // go home on LCD

  //Draw initial screen state
  DrawScreen();
}

//Redraws the entire screen
void DrawScreen()
{
  lcd.home(); // go home on LCD

  //Write as much as the song title as possible
  for (int i = 0; i < song.title.length(); i++)
  {
    //check we don't exceed screen width
    if (i < SCREEN_WIDTH) lcd.print(song.title[i]);
  }

  lcd.setCursor(0, 1); //start of 2nd line
  //Write as much as the song author as possible
  for (int i = 0; i < song.author.length(); i++)
  {
    //check we don't exceed screen width
    if (i < SCREEN_WIDTH) lcd.print(song.author[i]);
  }

  lcd.setCursor(0, 2); // go to start of 3rd line
}

void loop()
{
  DrawScreen();
  delay(200);
}

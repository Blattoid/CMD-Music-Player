#include "Wire.h"
#include "LCD.h"
#include "NewLiquidCrystal_I2C.h"

#define I2C_ADDR 0x27  // Character LCD I2C address
#define SCREEN_WIDTH 20
#define SCREEN_HEIGHT 4
NewLiquidCrystal_I2C lcd(I2C_ADDR, 2, 1, 0, 4, 5, 6, 7); // Initialise with LCD pin mappings on I2C device along with I2C address

#define RedLedPin 5
bool ledState = false;

int cursor_position[2] = {0, 0}; //A copy of the cursor position for use by the song title scroller
//The reason we keep a copy is that if the timer interrupts after we set the cursor position, then it will be changed.
//This way, we can revert the cursor position to whatever it once was.

void setup()
{
  pinMode(RedLedPin, OUTPUT);
  lcd.begin (SCREEN_WIDTH, SCREEN_HEIGHT); // My LCD is a 20x4, change for your LCD if needed

  // LCD Backlight ON
  lcd.setBacklightPin(3, POSITIVE);
  lcd.setBacklight(HIGH);
  lcd.home(); // go home on LCD

  //Draw initial screen state
  lcd.print("Unknown");
  lcd.setCursor(0, 1); //start of 2nd line
  for (int i = 0; i < SCREEN_WIDTH; i++) lcd.print("-");
  lcd.home();
}

//Variables to hold song information sent from music player
String Song_Title = "Unknown";
String Song_Author = "Unknown";
int Song_Duration_Hours = 0;
int Song_Duration_Minutes = 0;
int Song_Duration_Seconds = 0;
int Song_Length_Hours = 0;
int Song_Length_Minutes = 0;
int Song_Length_Seconds = 0;

//Redraws the entire screen
void UpdateScreen()
{
  lcd.home (); // go home on LCD
  
  //Write as much as the song title as possible
  for (int i = 0; i < SongTitle.length(); i++)
  {
    //check we don't exceed screen width
    if (i < SCREEN_WIDTH)    lcd.print(SongTitle[i]);
  }
  
  lcd.setCursor(0, 1); //start of 2nd line
  //Write as much as the song author as possible
    for (int i = 0; i < SongTitle.length(); i++)
  {
    //check we don't exceed screen width
    if (i < SCREEN_WIDTH)    lcd.print(SongAuthor[i]);
  }
  
  lcd.setCursor(0, 2); // go to start of 3rd line
}
void loop()
{

  delay(200);
}

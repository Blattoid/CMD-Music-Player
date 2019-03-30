#include "Wire.h"
#include "LCD.h"
#include "NewLiquidCrystal_I2C.h"

#define I2C_ADDR 0x27  // Character LCD I2C address
#define SCREEN_WIDTH 20
#define SCREEN_HEIGHT 4
NewLiquidCrystal_I2C lcd(I2C_ADDR, 2, 1, 0, 4, 5, 6, 7); // Initialise with LCD pin mappings on I2C device along with I2C address

#define RedLedPin 5
bool ledState = false;

void setup()
{
  pinMode(RedLedPin,OUTPUT);
  lcd.begin (SCREEN_WIDTH, SCREEN_HEIGHT); // My LCD is a 20x4, change for your LCD if needed

  // LCD Backlight ON
  lcd.setBacklightPin(3, POSITIVE);
  lcd.setBacklight(HIGH);
  lcd.home(); // go home on LCD
  
  //Draw initial screen state
  lcd.print("'Unknown' by 'Unknown'.");
  lcd.setCursor(0,1); //start of 2nd line
  for (int i = 0; i< SCREEN_WIDTH; i++) lcd.print("-");
  
  //set timer1 interrupt at 3z
  TCCR1A = 0;// set entire TCCR1A register to 0
  TCCR1B = 0;// same for TCCR1B
  TCNT1  = 0;//initialize counter value to 0
  // set compare match register for 3hz increments
  OCR1A = 5207;// = (16*10^6) / (3*1024) - 1 (must be <65536)
  // turn on CTC mode
  TCCR1B |= (1 << WGM12);
  // Set CS10 and CS12 bits for 1024 prescaler
  TCCR1B |= (1 << CS12) | (1 << CS10);  
  // Finally, enable timer compare interrupt
  TIMSK1 |= (1 << OCIE1A);
}

//Variables to hold song information sent from music player
String SongTitle = "Unknown";
String SongAuthor = "Unknown";
int SongDuration_Hours = 0;
int SongDuration_Minutes = 0;
int SongDuration_Seconds = 0;

void loop()
{
  lcd.home (); // go home on LCD
  lcd.setCursor (0, 2); // go to start of 3rd line
  lcd.print(millis());

  delay(200);
}

// Overflow interrupt on Timer1 is executed ~3 times per second
SIGNAL(TIMER1_COMPA_vect)
{
  digitalWrite(RedLedPin, ledState);
  ledState = !ledState;
  SongTitleScrollUpdate();
}
int ScrollProgress = 0;
void SongTitleScrollUpdate(){
  String SongString = "'"+SongTitle+"' by '"+SongAuthor+"'";
  if (ScrollProgress > SongString.length()) return;
  lcd.setCursor(ScrollProgress,0);
  lcd.print(SongTitle[ScrollProgress]);
  lcd.setCursor(cursor_position[0], cursor_position[1]);
}

uint8_t[2] cursor_position = [0,0]; //A copy of the cursor position for use by the song title scroller
//The reason we keep a copy is that if the timer interrupts after we set the cursor position, then it will be changed.
//This way, we can revert the cursor position to whatever it once was.
void SetLCDCursor(uint8_t col, uint8_t row)
{
  cursor_position[0] = col; //X
  cursor_position[1] = row; //Y
  
}

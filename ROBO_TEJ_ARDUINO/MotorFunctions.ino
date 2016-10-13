#include <Servo.h>


Servo sL;
Servo sR;
Servo s1;
bool turnOn = true;
bool straight = true;
bool changeDirection = false;
bool changeSpeed = false;


char incomingByte;
int a, b, c;



int angle = 0;
int angleMod = 10;
unsigned long pM = 0;
unsigned long pM1 = 0;
unsigned long interval = 828;


void setup()
{
  sL.attach(10);
  sR.attach(11);
  s1.attach(3);
  s1.write(0);
  delay(1000);
  Serial.begin(38400);
  pinMode(2, OUTPUT);
  digitalWrite(2, HIGH);
  delay(500);
  digitalWrite(2, LOW);
}

void loop()
{
  if (Serial.available() == 4)
  {
    incomingByte = Serial.read(); int xa = (incomingByte - 48);
    incomingByte = Serial.read(); int xb = (incomingByte - 48);
    incomingByte = Serial.read(); int xc = (incomingByte - 48); incomingByte = Serial.read();

    if (xa < 6 && xa > 0 && xb > -1 && xb < 10 && xc > -1 && xc < 10)
    {
      a = xa; b = xb * 10; c = xc * 10;
    }
  }

  if (a == 1)
  {
    digitalWrite(2, HIGH);
    LeftTread(90 - b);
    RightTread(90 - c);
  }
  else if (a == 2)
  {
    digitalWrite(2, LOW);
    LeftTread(90 + b);
    RightTread(90 + b);
  }
  else if (a == 3)
  {
    LeftTread(90 - b);
    RightTread(90 + c);
  }
  else if (a == 4 )
  {
    LeftTread(90 + b);
    RightTread(90 - c);
  }


  unsigned long cM = millis();

  if (cM - pM >= interval)
  {
    pM = cM;
    if (s1.read() == 0)
    {
      angleMod = 10;
      //s1.write(180);
    }
    else
    {
      angleMod = -10;
      //s1.write(0);
    }
  }

  if (cM - pM1 >= interval / (180 / abs(angleMod)))
  {
    pM1 = cM;
    Serial.println(String(angle) + ":" + String(analogRead(0)) + ":" + String(analogRead(1))); 
    angle += angleMod;
    if (angle > 180)
      angle = 180;
    else if (angle < 0)
      angle = 0;
  }



}


void LeftTread(int num)
{
  sL.write(num);
}

void RightTread(int num)
{
  sR.write(num);
}



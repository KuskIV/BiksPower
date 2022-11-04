import requests
import json
from re import M
import RPI.GPIO as GPIO
from datetime import datetime

# SETUP
CLAMP = "CLAMP"
Plug_SurfacePro = "Plug_SurfacePro"
Plug_SurfaceBook = "Plug_SurfaceBook"

Toggle_detection = False
HW_start = ""
HW_end = ""

Plug_Pins = {
    Plug_SurfaceBook : 38,
    Plug_SurfacePro : 40
}
Active_measures = { #False meaning is has not started
    CLAMP : False,
    Plug_SurfacePro : False,
    Plug_SurfaceBook : False
}
GPIO.setmode(GPIO.BOARD)
GPIO.setup(Plug_Pins[Plug_SurfacePro], GPIO.OUT)
GPIO.setup(Plug_Pins[Plug_SurfaceBook], GPIO.OUT)

def PingServer():

    headers = {
        'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/107.0.1418.26',
    }

    response = requests.get('http://www.stemlevelup.com/api/RaspberryPi', headers=headers, verify=False)
    print(response)
    return json.loads(response)

def Switch(measures):
    for measure in measures:
        if(measure.name == CLAMP):
            Active_measures[CLAMP] == measure.state
        if(measure.name == Plug_SurfaceBook):
            Active_measures[Plug_SurfaceBook] == measure.state
        if(measure.name == Plug_SurfacePro):
            Active_measures[Plug_SurfacePro] == measure.state 

def Update():
    if(Active_measures[Plug_SurfaceBook]):
        GPIO.output(Plug_Pins[Plug_SurfaceBook], GPIO.HIGH)
    else:
        GPIO.output(Plug_Pins[Plug_SurfaceBook], GPIO.LOW)

    if(Active_measures[Plug_SurfacePro]):
        GPIO.output(Plug_Pins[Plug_SurfacePro], GPIO.HIGH)
    else:
        GPIO.output(Plug_Pins[Plug_SurfacePro], GPIO.LOW)

    if(Active_measures[CLAMP] and not Toggle_detection):
        Toggle_detection = True
        HW_start = datetime.now()
    elif(Toggle_detection and not Active_measures[CLAMP]):
        Toggle_detection = False
        HW_end = datetime.now()
        ExportMeasures(HW_start, HW_end)

def ExportMeasures(fromDate, toDate):
    
    return 0

while(True):
    measures = PingServer()
    Switch(measures)
    Update()

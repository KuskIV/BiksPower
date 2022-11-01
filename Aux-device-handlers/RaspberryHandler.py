import http.client
import json
from re import M
import RPI.GPIO as GPIO
from datetime import datetime

# SETUP
CLAMP = "CLAMP"
Plug_SurfacePro = "Plug_SurfacePro"
Plug_SurfaceBook = "Plug_SurfaceBook"

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
    conn = http.client.HTTPSConnection("localhost", 7166)
    payload = ''
    headers = {}
    conn.request("GET",  "/api/RaspberryPi", payload, headers)
    res = conn.getresponse()
    data = res.read()
    print(data.decode("utf-8"))
    return json.loads(data)

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

    if(Active_measures[CLAMP]):
        HW_start = datetime.now()
    else:
        HW_end = datetime.now() 

while(True):
    measures = PingServer()
    Switch(measures)
    Update()

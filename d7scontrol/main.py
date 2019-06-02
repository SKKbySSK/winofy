# coding:utf-8
import grove_d7s
import time
import subprocess

#sensor instance
sensor = grove_d7s.GroveD7s()

sensor.__init__()

while sensor.isReady()==False:
    time.sleep(1.0)

    while True:
        time.sleep(10)
        si=sensor.getInstantaneusSI()
        pga=sensor.getInstantaneusPGA()
        axis=sensor.GetOffsetAxis()



        if si == None and pga == None:
            continue

    SendingData="winofy record -SI "+ str(si) +"-axis "+str(axis) +"-temp" +str(temp)+"-humidity"+str(hum)+"-token"+token
    subprocess.call(SendingData.split())
        


        